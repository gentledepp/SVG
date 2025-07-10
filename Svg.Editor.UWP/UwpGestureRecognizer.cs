using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;

namespace Svg.Editor.UWP
{
    public class UwpGestureRecognizer : IGestureRecognizer, IDisposable
    {
        private readonly Subject<UserGesture> _gesturesSubject = new Subject<UserGesture>();
        public IObservable<UserGesture> RecognizedGestures => _gesturesSubject.AsObservable();

        private readonly ManipulationInputProcessor _inputProcessor;

        public UwpGestureRecognizer(UIElement control)
        {
            var gestureRecognizer = new GestureRecognizer();
            _inputProcessor = new ManipulationInputProcessor(gestureRecognizer, control);
        }

        public void Dispose()
        {
            _inputProcessor.Dispose();
        }
    }

    internal class ManipulationInputProcessor : IDisposable
    {
        private readonly GestureRecognizer _recognizer;
        private readonly UIElement _element;
        private TransformGroup _cumulativeTransform;
        private MatrixTransform _previousTransform;
        private CompositeTransform _deltaTransform;

        public ManipulationInputProcessor(GestureRecognizer gestureRecognizer, UIElement referenceFrame)
        {
            _recognizer = gestureRecognizer;
            _element = referenceFrame;

            // Initialize the transforms that will be used to manipulate the shape
            InitializeTransforms();

            // The GestureSettings property dictates what manipulation events the
            // Gesture Recognizer will listen to.  This will set it to a limited
            // subset of these events.
            _recognizer.GestureSettings = GenerateDefaultSettings();

            // Set up pointer event handlers. These receive input events that are used by the gesture recognizer.
            _element.PointerPressed += OnPointerPressed;
            _element.PointerMoved += OnPointerMoved;
            _element.PointerReleased += OnPointerReleased;
            _element.PointerCanceled += OnPointerCanceled;

            // Set up event handlers to respond to gesture recognizer output
            _recognizer.ManipulationStarted += OnManipulationStarted;
            _recognizer.ManipulationUpdated += OnManipulationUpdated;
            _recognizer.ManipulationCompleted += OnManipulationCompleted;
            _recognizer.ManipulationInertiaStarting += OnManipulationInertiaStarting;
        }

        public void InitializeTransforms()
        {
            _cumulativeTransform = new TransformGroup();
            _deltaTransform = new CompositeTransform();
            _previousTransform = new MatrixTransform { Matrix = Windows.UI.Xaml.Media.Matrix.Identity };

            _cumulativeTransform.Children.Add(_previousTransform);
            _cumulativeTransform.Children.Add(_deltaTransform);

            _element.RenderTransform = _cumulativeTransform;
        }

        // Return the default GestureSettings for this sample
        private GestureSettings GenerateDefaultSettings()
        {
            return GestureSettings.ManipulationTranslateX |
                GestureSettings.ManipulationTranslateY |
                GestureSettings.ManipulationRotate |
                GestureSettings.ManipulationTranslateInertia |
                GestureSettings.ManipulationRotateInertia;
        }

        // Route the pointer pressed event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            // Set the pointer capture to the element being interacted with so that only it
            // will fire pointer-related events
            _element.CapturePointer(args.Pointer);

            // Feed the current point into the gesture recognizer as a down event
            _recognizer.ProcessDownEvent(args.GetCurrentPoint(_element));
        }

        // Route the pointer moved event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            // Feed the set of points into the gesture recognizer as a move event
            _recognizer.ProcessMoveEvents(args.GetIntermediatePoints(_element));
        }

        // Route the pointer released event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            // Feed the current point into the gesture recognizer as an up event
            _recognizer.ProcessUpEvent(args.GetCurrentPoint(_element));

            // Release the pointer
            _element.ReleasePointerCapture(args.Pointer);
        }

        // Route the pointer canceled event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
        {
            _recognizer.CompleteGesture();
            _element.ReleasePointerCapture(args.Pointer);
        }

        // When a manipulation begins, change the color of the object to reflect
        // that a manipulation is in progress
        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
        }

        // Process the change resulting from a manipulation
        private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            _previousTransform.Matrix = _cumulativeTransform.Value;

            // Get the center point of the manipulation for rotation
            Point center = new Point(e.Position.X, e.Position.Y);
            _deltaTransform.CenterX = center.X;
            _deltaTransform.CenterY = center.Y;

            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the rotation, X, and Y changes
            _deltaTransform.Rotation = e.Delta.Rotation;
            _deltaTransform.TranslateX = e.Delta.Translation.X;
            _deltaTransform.TranslateY = e.Delta.Translation.Y;
        }

        // When a manipulation that's a result of inertia begins, change the color of the
        // the object to reflect that inertia has taken over
        private void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
        }

        // When a manipulation has finished, reset the color of the object
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
        }

        // Modify the GestureSettings property to only allow movement on the X axis
        public void LockToXAxis()
        {
            _recognizer.CompleteGesture();
            _recognizer.GestureSettings |= GestureSettings.ManipulationTranslateY | GestureSettings.ManipulationTranslateX;
            _recognizer.GestureSettings ^= GestureSettings.ManipulationTranslateY;
        }

        // Modify the GestureSettings property to only allow movement on the Y axis
        public void LockToYAxis()
        {
            _recognizer.CompleteGesture();
            _recognizer.GestureSettings |= GestureSettings.ManipulationTranslateY | GestureSettings.ManipulationTranslateX;
            _recognizer.GestureSettings ^= GestureSettings.ManipulationTranslateX;
        }

        // Modify the GestureSettings property to allow movement on both the the X and Y axes
        public void MoveOnXAndYAxes()
        {
            _recognizer.CompleteGesture();
            _recognizer.GestureSettings |= GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;
        }

        // Modify the GestureSettings property to enable or disable inertia based on the passed-in value
        public void UseInertia(bool inertia)
        {
            if (!inertia)
            {
                _recognizer.CompleteGesture();
                _recognizer.GestureSettings ^= GestureSettings.ManipulationTranslateInertia | GestureSettings.ManipulationRotateInertia;
            }
            else
            {
                _recognizer.GestureSettings |= GestureSettings.ManipulationTranslateInertia | GestureSettings.ManipulationRotateInertia;
            }
        }

        public void Reset()
        {
            _element.RenderTransform = null;
            _recognizer.CompleteGesture();
            InitializeTransforms();
            _recognizer.GestureSettings = GenerateDefaultSettings();
        }

        public void Dispose()
        {
            // Unregister pointer event handlers. These receive input events that are used by the gesture recognizer.
            _element.PointerPressed -= OnPointerPressed;
            _element.PointerMoved -= OnPointerMoved;
            _element.PointerReleased -= OnPointerReleased;
            _element.PointerCanceled -= OnPointerCanceled;

            // Unregister event handlers to respond to gesture recognizer output
            _recognizer.ManipulationStarted -= OnManipulationStarted;
            _recognizer.ManipulationUpdated -= OnManipulationUpdated;
            _recognizer.ManipulationCompleted -= OnManipulationCompleted;
            _recognizer.ManipulationInertiaStarting -= OnManipulationInertiaStarting;
        }
    }
}

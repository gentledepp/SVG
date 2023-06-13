using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SkiaSharp.Views.Windows;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using GestureRecognizer = Windows.UI.Input.GestureRecognizer;
using IGestureRecognizer = Svg.Editor.Interfaces.IGestureRecognizer;
using Point = Windows.Foundation.Point;
using PointF = Svg.Interfaces.PointF;
using SizeF = Svg.Interfaces.SizeF;

namespace Svg.Editor.Views.UWP
{
    public class UwpGestureRecognizer : IGestureRecognizer, IInputEventDetector, IDisposable
    {
        private readonly ManipulationInputProcessor _inputProcessor;

        public void OnNext(UserInputEvent e)
        {
            // do nothing - everything is handled in the manipulationinputprocessor directly
        }

        public IObservable<UserGesture> RecognizedGestures => _inputProcessor.RecognizedGestures;
        public IObservable<UserInputEvent> UserInputEvents => _inputProcessor.UserInputEvents;

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
        // Why 960, you ask?
        // One wheel-step is defined as 120 (see: https://msdn.microsoft.com/en-us/library/windows/desktop/ms645617(v=vs.85).aspx)
        // The faster the wheel is scrolled, the higher the value will be, but it maxes out at 960
        private const float MaxMouseWheelStep = 960;

        private readonly GestureRecognizer _recognizer;
        private readonly UIElement _element;
        private TransformGroup _cumulativeTransform;
        private MatrixTransform _previousTransform;
        private CompositeTransform _deltaTransform;

        private readonly Subject<UserGesture> _gesturesSubject = new Subject<UserGesture>();
        private readonly Subject<UserInputEvent> _inputEventSubject = new Subject<UserInputEvent>();
        private Point _startPoint;
        private Point _currentPoint;

        // DIPs = pixels / (DPI/96.0), see: https://msdn.microsoft.com/en-us/library/windows/desktop/ff684173(v=vs.85).aspx
        private static float PixelDensityFactor => DisplayInformation.GetForCurrentView().LogicalDpi / 96;

        public IObservable<UserGesture> RecognizedGestures => _gesturesSubject.AsObservable();
        public IObservable<UserInputEvent> UserInputEvents => _inputEventSubject.AsObservable();

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

            _element.Tapped += ElementOnTapped;
            _element.RightTapped += ElementRightTapped;
            _element.DoubleTapped += ElementOnDoubleTapped;
            _element.PointerWheelChanged += ElementOnPointerWheelChanged;

            // Set up event handlers to respond to gesture recognizer output
            _recognizer.ManipulationStarted += OnManipulationStarted;
            _recognizer.ManipulationUpdated += OnManipulationUpdated;
            _recognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        private void ElementRightTapped(object sender, RightTappedRoutedEventArgs args)
        {
            var dpi = 1.0;
            if (_element is SKXamlCanvas c)
            {
                dpi = c.Dpi;
            }
            var position = args.GetPosition(_element);
            _gesturesSubject.OnNext(
                new LongPressGesture(PointF.Create((float)(position.X * dpi), (float)(position.Y * dpi))));
        }

        private void ElementOnPointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(_element);
            var wheelDelta = pointerPoint.Properties.MouseWheelDelta;

            _inputEventSubject.OnNext(new ScaleEvent(ScaleStatus.Scaling, 1 + wheelDelta / MaxMouseWheelStep, (float) pointerPoint.Position.X, (float) pointerPoint.Position.Y)
            {
                ChangeFocus = true
            });
        }

        private void ElementOnDoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            var dpi = 1.0;
            if (_element is SKXamlCanvas c)
            {
                dpi = c.Dpi;
            }
            var position = args.GetPosition(_element);
            _gesturesSubject.OnNext(
                new DoubleTapGesture(PointF.Create((float)(position.X * dpi), (float)(position.Y * dpi))));
        }

        private void ElementOnTapped(object sender, TappedRoutedEventArgs args)
        {
            var dpi = 1.0;
            if (_element is SKXamlCanvas c)
            {
                dpi = c.Dpi;
            }
            var position = args.GetPosition(_element);
            _gesturesSubject.OnNext(
                new TapGesture(PointF.Create((float)(position.X * dpi), (float)(position.Y * dpi))));
        }

        public void InitializeTransforms()
        {
            _cumulativeTransform = new TransformGroup();
            _deltaTransform = new CompositeTransform();
            _previousTransform = new MatrixTransform { Matrix = Microsoft.UI.Xaml.Media.Matrix.Identity };

            _cumulativeTransform.Children.Add(_previousTransform);
            _cumulativeTransform.Children.Add(_deltaTransform);

            _element.RenderTransform = _cumulativeTransform;
        }

        // Return the default GestureSettings for this sample
        private GestureSettings GenerateDefaultSettings()
        {
            return GestureSettings.ManipulationTranslateX |
                GestureSettings.ManipulationTranslateY |
                GestureSettings.ManipulationMultipleFingerPanning;
        }

        // Route the pointer pressed event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            // Set the pointer capture to the element being interacted with so that only it
            // will fire pointer-related events
            _element.CapturePointer(args.Pointer);

            // Feed the current point into the gesture recognizer as a down event
           // _recognizer.ProcessDownEvent(args.GetCurrentPoint(_element));

            var pointerPoint = args.GetCurrentPoint(_element);
            var pointerPosition = pointerPoint.Position;
            var pointerPointF = PointF.Create((float) pointerPosition.X, (float) pointerPosition.Y);

            _inputEventSubject.OnNext(new PointerEvent(EventType.PointerDown, pointerPointF, pointerPointF, pointerPointF, 1));

            _currentPoint = _startPoint = pointerPosition;
        }

        // Route the pointer moved event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(_element);

            // return here if no relevant pointer is pressed
            if (!(pointerPoint.Properties.IsLeftButtonPressed || pointerPoint.Properties.IsMiddleButtonPressed))
                return;

            var previousPointF = PointF.Create((float) _currentPoint.X, (float) _currentPoint.Y);
            _currentPoint = pointerPoint.Position;
            var currentPointF = PointF.Create((float) _currentPoint.X, (float) _currentPoint.Y);
            var startPointF = PointF.Create((float) _startPoint.X, (float) _startPoint.Y);
            var delta = currentPointF - previousPointF;
            if (pointerPoint.Properties.IsMiddleButtonPressed)
                _inputEventSubject.OnNext(new MoveEvent(startPointF, previousPointF, currentPointF, delta, 2));

            // Feed the set of points into the gesture recognizer as a move event
            //if (pointerPoint.Properties.IsLeftButtonPressed)
            //    _recognizer.ProcessMoveEvents(args.GetIntermediatePoints(_element));
        }

        // Route the pointer released event to the gesture recognizer.
        // The points are in the reference frame of the canvas that contains the rectangle element.
        private void OnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            // Feed the current point into the gesture recognizer as an up event
            //_recognizer.ProcessUpEvent(args.GetCurrentPoint(_element));

            // Release the pointer
            _element.ReleasePointerCapture(args.Pointer);

            _inputEventSubject.OnNext(new PointerEvent(EventType.PointerUp, PointF.Empty, PointF.Empty, PointF.Empty, 0));
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
            var dpi = 1.0;
            if (_element is SKXamlCanvas c)
            {
                dpi = c.Dpi;
            }

            _gesturesSubject.OnNext(
                DragGesture.Enter(PointF.Create((float)(e.Position.X * dpi), (float)(e.Position.Y * dpi))));
        }

        // Process the change resulting from a manipulation
        private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            var pixelDensityFactor = PixelDensityFactor;
            var position = PointF.Create((float) e.Position.X, (float) e.Position.Y) * pixelDensityFactor;
            var delta = SizeF.Create((float) e.Cumulative.Translation.X * pixelDensityFactor, (float) e.Cumulative.Translation.Y * pixelDensityFactor);
            var start = PointF.Create((float) _startPoint.X * pixelDensityFactor, (float) _startPoint.Y * pixelDensityFactor);
            var distance = Math.Sqrt(Math.Pow(delta.Width, 2) + Math.Pow(delta.Height, 2));
            _gesturesSubject.OnNext(new DragGesture(position, start, delta, distance));

            //_previousTransform.Matrix = _cumulativeTransform.Value;

            //// Get the center point of the manipulation for rotation
            //Point center = new Point(e.Position.X, e.Position.Y);
            //_deltaTransform.CenterX = center.X;
            //_deltaTransform.CenterY = center.Y;

            //// Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            //// the rotation, X, and Y changes
            //_deltaTransform.Rotation = e.Delta.Rotation;
            //_deltaTransform.TranslateX = e.Delta.Translation.X;
            //_deltaTransform.TranslateY = e.Delta.Translation.Y;
        }

        // When a manipulation has finished, reset the color of the object
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _gesturesSubject.OnNext(DragGesture.Exit);
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
            // Unregister pointer event handlers
            _element.PointerPressed -= OnPointerPressed;
            _element.PointerMoved -= OnPointerMoved;
            _element.PointerReleased -= OnPointerReleased;
            _element.PointerCanceled -= OnPointerCanceled;

            _element.Tapped -= ElementOnTapped;
            _element.DoubleTapped -= ElementOnDoubleTapped;
            _element.PointerWheelChanged -= ElementOnPointerWheelChanged;

            // Unregister event handlers
            _recognizer.ManipulationStarted -= OnManipulationStarted;
            _recognizer.ManipulationUpdated -= OnManipulationUpdated;
            _recognizer.ManipulationCompleted -= OnManipulationCompleted;
        }
    }
}

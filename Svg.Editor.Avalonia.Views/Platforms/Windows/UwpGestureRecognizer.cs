using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using IGestureRecognizer = Svg.Editor.Interfaces.IGestureRecognizer;
using PointF = Svg.Interfaces.PointF;
using SizeF = Svg.Interfaces.SizeF;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using TappedEventArgs = Avalonia.Input.TappedEventArgs;
using Avalonia;

namespace Svg.Editor.Avalon.Views.Platforms.Windows;
public class UwpGestureRecognizer : IGestureRecognizer, IInputEventDetector, IDisposable
{
    private readonly ManipulationInputProcessor _inputProcessor;

    public void OnNext(UserInputEvent e)
    {
        // do nothing - everything is handled in the manipulationinputprocessor directly
    }

    public IObservable<UserGesture> RecognizedGestures => _inputProcessor.RecognizedGestures;
    public IObservable<UserInputEvent> UserInputEvents => _inputProcessor.UserInputEvents;

    public UwpGestureRecognizer(Control control)
    {
        var gestureRecognizer = new CustomGestureRecognizer();

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
    private const float MaxMouseWheelStep = 12;

    private readonly CustomGestureRecognizer _recognizer;
    private readonly Control _element;
    private TransformGroup _cumulativeTransform;
    private MatrixTransform _previousTransform;

    private readonly Subject<UserGesture> _gesturesSubject = new Subject<UserGesture>();
    private readonly Subject<UserInputEvent> _inputEventSubject = new Subject<UserInputEvent>();
    private Point _startPoint;
    private Point _currentPoint;
    private bool _isManipulated = false;

    // DIPs = pixels / (DPI/96.0), see: https://msdn.microsoft.com/en-us/library/windows/desktop/ff684173(v=vs.85).aspx
    private static float PixelDensityFactor => 1;

    public IObservable<UserGesture> RecognizedGestures => _gesturesSubject.AsObservable();
    public IObservable<UserInputEvent> UserInputEvents => _inputEventSubject.AsObservable();

    public ManipulationInputProcessor(CustomGestureRecognizer gestureRecognizer, Control referenceFrame)
    {
        _recognizer = gestureRecognizer;
        _element = referenceFrame;

        // Initialize the transforms that will be used to manipulate the shape
        InitializeTransforms();

        // The GestureSettings property dictates what manipulation events the
        // Gesture Recognizer will listen to.  This will set it to a limited
        // subset of these events.
        //_element.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateRailsY;

        // Set up pointer event handlers. These receive input events that are used by the gesture recognizer.
        _element.PointerPressed += OnPointerPressed;
        _element.PointerMoved += OnPointerMoved;
        _element.PointerReleased += OnPointerReleased;
        _element.PointerCaptureLost += OnPointerCanceled;

        _element.Tapped += ElementOnTapped;
        _element.DoubleTapped += ElementOnDoubleTapped;
        _element.PointerWheelChanged += ElementOnPointerWheelChanged;
        //_element.ManipulationStarted += ElementOnManipulateStarted;
        //_element.ManipulationCompleted += ElementOnManipulateCompleted;

        //Set up event handlers to respond to gesture recognizer output
        _recognizer.OnManipulationStarted += OnManipulationStarted;
        _recognizer.OnManipulationDelta += OnManipulationUpdated;
        _recognizer.OnManipulationCompleted += OnManipulationCompleted;
    }

    private void ElementOnManipulateCompleted(object sender, GestureRecognizerEventArgs e)
    {
        _isManipulated = false;
    }


    private void ElementOnManipulateStarted(object sender, GestureRecognizerEventArgs e)
    {
        _isManipulated = true;
    }

    private void ElementOnPointerWheelChanged(object sender, PointerWheelEventArgs args)
    {
        var pointerPoint = args.GetCurrentPoint(_element);
        var wheelDelta = (float)args.Delta.Y;

        _inputEventSubject.OnNext(new ScaleEvent(ScaleStatus.Scaling, 1+wheelDelta / MaxMouseWheelStep,
      (float)pointerPoint.Position.X, (float)pointerPoint.Position.Y)
        {
            ChangeFocus = true
        });
    }

    private void ElementOnDoubleTapped(object sender, TappedEventArgs args)
    {
        var dpi = PixelDensityFactor;
        //if (_element is SKXamlCanvas c)
        //{
        //    dpi = c.Dpi;
        //}

        var position = args.GetPosition(_element);
        _gesturesSubject.OnNext(
            new DoubleTapGesture(PointF.Create((float)(position.X * dpi), (float)(position.Y * dpi))));
    }

    private void ElementOnTapped(object sender, TappedEventArgs args)
    {
        var dpi = PixelDensityFactor;
        //if (_element is SKXamlCanvas c)
        //{
        //    dpi = c.Dpi;
        //}

        var position = args.GetPosition(_element);
        _gesturesSubject.OnNext(
            new TapGesture(PointF.Create((float)(position.X * dpi), (float)(position.Y * dpi))));
    }

    public void InitializeTransforms()
    {
        _cumulativeTransform = new TransformGroup();
        _previousTransform = new MatrixTransform { Matrix = global::Avalonia.Matrix.Identity };

        _cumulativeTransform.Children.Add(_previousTransform);

        _element.RenderTransform = _cumulativeTransform;
    }


    // Route the pointer pressed event to the gesture recognizer.
    // The points are in the reference frame of the canvas that contains the rectangle element.
    private void OnPointerPressed(object sender, PointerPressedEventArgs args)
    {
        // Set the pointer capture to the element being interacted with so that only it
        // will fire pointer-related events
        //_element.CapturePointer(args.Pointer);

        // Feed the current point into the gesture recognizer as a down event
        // _recognizer.ProcessDownEvent(args.GetCurrentPoint(_element));

        var pointerPoint = args.GetCurrentPoint(_element);
        var pointerPosition = pointerPoint.Position;
        var pointerPointF = PointF.Create((float)pointerPosition.X, (float)pointerPosition.Y);

        _inputEventSubject.OnNext(new PointerEvent(EventType.PointerDown, pointerPointF, pointerPointF,
            pointerPointF, 1));

        _currentPoint = _startPoint = pointerPosition;
    }

    // Route the pointer moved event to the gesture recognizer.
    // The points are in the reference frame of the canvas that contains the rectangle element.
    private void OnPointerMoved(object sender, PointerEventArgs args)
    {
        var pointerPoint = args.GetCurrentPoint(_element);

        // return here if no relevant pointer is pressed
        if (!(pointerPoint.Properties.IsLeftButtonPressed || pointerPoint.Properties.IsMiddleButtonPressed))
            return;

        var previousPointF = PointF.Create((float)_currentPoint.X, (float)_currentPoint.Y);
        _currentPoint = pointerPoint.Position;
        var currentPointF = PointF.Create((float)_currentPoint.X, (float)_currentPoint.Y);
        var startPointF = PointF.Create((float)_startPoint.X, (float)_startPoint.Y);
        var delta = currentPointF - previousPointF;
        if (pointerPoint.Properties.IsMiddleButtonPressed)
            _inputEventSubject.OnNext(new MoveEvent(startPointF, previousPointF, currentPointF, delta, 2));

        // Feed the set of points into the gesture recognizer as a move event
        if (pointerPoint.Properties.IsLeftButtonPressed)
        {
            if (!_isManipulated)
            {
                _recognizer.ProcessStart(args.GetIntermediatePoints(_element).First().Position);
                _isManipulated = true;
                return;
            }
            _recognizer.ProcessMove(args.GetIntermediatePoints(_element).First());
        }
    }

    // Route the pointer released event to the gesture recognizer.
    // The points are in the reference frame of the canvas that contains the rectangle element.
    private void OnPointerReleased(object sender, PointerReleasedEventArgs args)
    {
        // Feed the current point into the gesture recognizer as an up event
        _recognizer.ProcessUp(args.GetCurrentPoint(_element));
        _isManipulated = false;

        var endPoint = args.GetPosition(_element);

        //var deltaPoint = endPoint - _startPoint;
        //var pointerF = PointF.Create((float)deltaPoint.X, (float)deltaPoint.Y);
        var pointerF = PointF.Create((float)endPoint.X, (float)endPoint.Y);


        // Release the pointer
        args.Pointer.Capture(null);

        _inputEventSubject.OnNext(
            new PointerEvent(EventType.PointerUp, pointerF, pointerF, pointerF, 0));
    }

    // Route the pointer canceled event to the gesture recognizer.
    // The points are in the reference frame of the canvas that contains the rectangle element.
    private void OnPointerCanceled(object sender, PointerCaptureLostEventArgs args)
    {
        //_recognizer.ProcessUp(args.);
        _isManipulated = false;
        //_element.ReleasePointerCapture(args.Pointer);
    }

    // When a manipulation begins, change the color of the object to reflect
    // that a manipulation is in progress
    private void OnManipulationStarted(object sender, GestureRecognizerEventArgs e)
    {
        var dpi = PixelDensityFactor;
        //if (_element is SKXamlCanvas c)
        //{
        //    dpi = c.Dpi;
        //}

        _gesturesSubject.OnNext(
            DragGesture.Enter(PointF.Create((float)(e.Point.X * dpi), (float)(e.Point.Y * dpi))));
    }

    // Process the change resulting from a manipulation
    private void OnManipulationUpdated(object sender, GestureRecognizerEventArgs e)
    {
        var previousPointF = PointF.Create((float)_startPoint.X, (float)_startPoint.Y);
        var currentPointF = PointF.Create((float)e.Point.X, (float)e.Point.Y);
        var deltaV = currentPointF - previousPointF;

        var pixelDensityFactor = PixelDensityFactor;
        var position = PointF.Create((float)e.Point.X, (float)e.Point.Y) * pixelDensityFactor;
        var delta = SizeF.Create((float)deltaV.X * pixelDensityFactor,
            deltaV.Y * pixelDensityFactor);
        var start = PointF.Create((float)_startPoint.X * pixelDensityFactor,
            (float)_startPoint.Y * pixelDensityFactor);
        var distance = Math.Sqrt(Math.Pow(delta.Width, 2) + Math.Pow(delta.Height, 2));
        _gesturesSubject.OnNext(new DragGesture(position, start, delta, distance));
    }

    // When a manipulation has finished, reset the color of the object
    private void OnManipulationCompleted(object sender, GestureRecognizerEventArgs e)
    {
        _gesturesSubject.OnNext(DragGesture.Exit);
    }

    public void Dispose()
    {
        // Unregister pointer event handlers
        _element.PointerPressed -= OnPointerPressed;
        _element.PointerMoved -= OnPointerMoved;
        _element.PointerReleased -= OnPointerReleased;
        _element.PointerCaptureLost -= OnPointerCanceled;

        _element.Tapped -= ElementOnTapped;
        _element.DoubleTapped -= ElementOnDoubleTapped;
        _element.PointerWheelChanged -= ElementOnPointerWheelChanged;

        // Unregister event handlers
        _recognizer.OnManipulationStarted -= OnManipulationStarted;
        _recognizer.OnManipulationDelta -= OnManipulationUpdated;
        _recognizer.OnManipulationCompleted -= OnManipulationCompleted;
    }
}
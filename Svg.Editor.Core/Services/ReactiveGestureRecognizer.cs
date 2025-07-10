using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Svg.Editor.Events;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Services
{
    public class ReactiveGestureRecognizer : IDisposable, IGestureRecognizer
    {
        private readonly Subject<UserGesture> _recognizedGestures = new Subject<UserGesture>();
        private readonly Subject<UserInputEvent> _detectedInputEvents = new Subject<UserInputEvent>();
        private readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();
        private readonly IScheduler _mainScheduler;
        private readonly IScheduler _backgroundScheduler;

        #region Public properties

        public void OnNext(UserInputEvent e)
        {
            _detectedInputEvents.OnNext(e);
        }

        /// <summary>
        /// Observable for recognized gestures.
        /// </summary>
        public IObservable<UserGesture> RecognizedGestures => _recognizedGestures.AsObservable();

        /// <summary>
        /// Minimum distance in pixels that is required to start a drag gesture.
        /// </summary>
        public double DragMinDistance { get; set; } = 10.0;

        /// <summary>
        /// Time in seconds which the pointer has to stay down until a long press gesture will be triggered.
        /// </summary>
        public double LongPressDuration { get; set; } = 0.66;

        /// <summary>
        /// Maximum distance in pixels that is ignored when tapping, to accomodate tiny shivers.
        /// </summary>
        public double TouchThreshold { get; set; } = 20.0;

        /// <summary>
        /// Maximum time in seconds for a valid tap gesture.
        /// </summary>
        public double TapTimeout { get; set; } = 0.33;

        /// <summary>
        /// Maximum time in seconds for a valid double tap gesture.
        /// </summary>
        public double DoubleTapTimeout { get; set; } = 0.2;

        #endregion

        public ReactiveGestureRecognizer(ISchedulerProvider schedulerProvider)
        {
            if (schedulerProvider == null) throw new ArgumentNullException(nameof(schedulerProvider));

            _mainScheduler = schedulerProvider.MainScheduer;
            _backgroundScheduler = schedulerProvider.BackgroundScheduler;

            SubscribeTo(_detectedInputEvents);
        }

        private void SubscribeTo(IObservable<UserInputEvent> detectedInputEvents)
        {
            if (detectedInputEvents == null) throw new ArgumentNullException(nameof(detectedInputEvents));

            var pointerEvents = detectedInputEvents.OfType<PointerEvent>();
            var enterEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerDown);
            var exitEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerUp || pe.EventType == EventType.Cancel);
            var interactionWindows = pointerEvents.Window(enterEvents, _ => exitEvents);

            #region tap gestures

            _subscriptions["tap"] = interactionWindows
            .SelectMany(window =>
            {
                return window
                .Where(
                    pe =>
                        pe.EventType == EventType.PointerUp &&
                        PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(TapTimeout), 1, _backgroundScheduler)
                .Take(1);
            })
            .Where(l => l.Any())
            .Select(l => new TapGesture(l.First().Pointer1Position))
            .Timestamp()
            .PairWithPrevious()
            .Throttle(TimeSpan.FromSeconds(DoubleTapTimeout), _backgroundScheduler)
            .Select
            (
                tup =>
                    tup.Item1.Value == null ||
                    tup.Item2.Timestamp - tup.Item1.Timestamp > TimeSpan.FromSeconds(DoubleTapTimeout) ||
                    !PositionEquals(tup.Item1.Value.Position, tup.Item2.Value.Position, TouchThreshold)
                        ? tup.Item2.Value
                        : new DoubleTapGesture(tup.Item2.Value.Position)
            )
            .ObserveOn(_mainScheduler)
            .Subscribe(tg => _recognizedGestures.OnNext(tg));

            #endregion

            #region long press gesture

            _subscriptions["longpress"] = interactionWindows
            .SelectMany(window =>
            {
                return window
                .Where(pe => pe.EventType == EventType.PointerDown || pe.EventType == EventType.PointerUp ||
                    !PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(LongPressDuration), 2)
                .Take(1);
            })
            .ObserveOn(_mainScheduler)
            .Subscribe
            (
                l =>
                {
                    if (l.Count == 1 && l.First().PointerCount == 1) _recognizedGestures.OnNext(new LongPressGesture(l.First().Pointer1Position));
                },
                ex => Debug.WriteLine(ex.Message)
            );

            #endregion

            #region drag gesture

            _subscriptions["drag"] = interactionWindows.Subscribe(window =>
            {
                // create this subject for controlling lifetime of the subscription
                var dragLifetime = new Subject<Unit>();
                var dragSubscription = window
                    .Where(pe => pe.EventType == EventType.Move && pe.PointerCount == 1)
                    // if we get a window without move events, we want to dispose subscription entirely,
                    // else we would propagate an unneccessary DragGesture.Exit gesture
                    .DefaultIfEmpty(new PointerEvent(EventType.Cancel, PointF.Empty, PointF.Empty, PointF.Empty, 0))
                    .Select((pe, i) =>
                    {
                        if (i == 0 && pe.EventType != EventType.Cancel)
                            _recognizedGestures.OnNext(DragGesture.Enter(pe.Pointer1Down));
                        // if we had an empty window, it defaults to EventType.Cancel and we dispose subscription
                        if (pe.EventType == EventType.Cancel) dragLifetime.OnCompleted();
                        return pe;
                    })
                    .ObserveOn(_mainScheduler)
                    .Subscribe
                    (
                        pe =>
                        {
                            var deltaPoint = pe.Pointer1Position - pe.Pointer1Down;
                            var delta = SizeF.Create(deltaPoint.X, deltaPoint.Y);

                            // selection only counts if width and height are not too small
                            var dist = Math.Sqrt(Math.Pow(delta.Width, 2) + Math.Pow(delta.Height, 2));

                            if (dist > DragMinDistance)
                            {
                                _recognizedGestures.OnNext(new DragGesture(pe.Pointer1Position, pe.Pointer1Down,
                                    delta, dist));
                            }
                        },
                        ex => { },
                        () => _recognizedGestures.OnNext(DragGesture.Exit)
                    );

                // bind completion of dragLifetime to disposing of our subscription
                Observable.Using(() => dragSubscription, _ => dragLifetime).Subscribe();
            });

            #endregion
        }

        private static bool PositionEquals(PointF start, PointF position, double threshold = 0)
        {
            var delta = position - start;
            return Math.Abs(delta.X) <= threshold && Math.Abs(delta.Y) <= threshold;
        }

        public void Dispose()
        {
            foreach (var disposable in _subscriptions.Values) disposable.Dispose();
        }
    }
}

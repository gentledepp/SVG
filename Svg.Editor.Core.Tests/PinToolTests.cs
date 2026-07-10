using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;
using Svg.Editor.Core.Test;
using Svg.Editor.Core.Test.Mocks;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Core.Tests
{
    [TestFixture]
    public class PinToolTests : SvgDrawingCanvasTestBase
    {
        private MockTextInputService _textMock;
        private Mock<IPinInputService> _pinInputServiceMock;


        override protected void SetupOverride()
        {
            _pinInputServiceMock = new Mock<IPinInputService>();

            var pinToolProperties = new Dictionary<string, object>
            {
                {"pinsizenames", new[] {"Small", "Medium", "Large", "ExtraLarge" } }
            };

            Canvas.LoadTools(() => new PinTool(pinToolProperties, SvgEngine.Resolve<IUndoRedoService>()));
        

            // register mock text input service
            _textMock = new MockTextInputService();
            SvgEngine.Register<ITextInputService>(() => _textMock);
            SvgEngine.Register<IPinInputService>(() => _pinInputServiceMock.Object);
        }

        [Test]
        public async Task WhenUserLongPresses_CreatesHoleyPinAtDefaultSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;
            var childCountBefore = Canvas.Document.Children.Count;

            // Act
            await LongPress(PointF.Create(50, 50));

            // Assert
            Assert.AreEqual(childCountBefore + 1, Canvas.Document.Children.Count);
            var pin = Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            Assert.AreEqual("Medium", pin.CustomAttributes[PinTool.PinSizeAttributeKey]);
            Assert.AreEqual("Holey", pin.CustomAttributes[PinTool.PinFillAttributeKey]);
        }

        [Test]
        public async Task WhenUserLongPresses_WithNonDefaultSelectedPinSize_UsesThatSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;
            pinTool.SelectedPinSize = PinTool.PinSize.Large;

            // Act
            await LongPress(PointF.Create(50, 50));

            // Assert
            var pin = Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            Assert.AreEqual("Large", pin.CustomAttributes[PinTool.PinSizeAttributeKey]);
        }

        [Test]
        public async Task WhenUserTapsOnExistingPin_SelectsIt()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;

            var position = PointF.Create(50, 50);
            await LongPress(position);
            var pin = Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Act
            await Tap(position);

            // Assert
            CollectionAssert.Contains(Canvas.SelectedElements, pin);
        }

        [Test]
        public async Task WhenUserTapsOnBlankArea_DoesNotSelectAnything()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;

            await LongPress(PointF.Create(50, 50));

            // Act - tap somewhere well away from the pin we just created
            await Tap(PointF.Create(400, 400));

            // Assert
            Assert.IsEmpty(Canvas.SelectedElements);
        }

        [Test]
        public async Task ChangePinSizeCommand_WithoutSelection_ChangesGlobalSelectedPinSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;

            _pinInputServiceMock
                .Setup(p => p.GetUserInput(It.IsAny<IEnumerable<string>>(), It.IsAny<int>()))
                .ReturnsAsync(PinTool.PinSize.Large);

            var command = pinTool.Commands.Single();

            // Act
            await WaitForToolCommandsChanged(() => command.Execute(null));

            // Assert
            Assert.AreEqual(PinTool.PinSize.Large, pinTool.SelectedPinSize);
        }

        [Test]
        public async Task ChangePinSizeCommand_WithSelection_ChangesOnlySelectedPinSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;

            var position = PointF.Create(50, 50);
            await LongPress(position);
            var pin = Canvas.Document.Children[Canvas.Document.Children.Count - 1] as SvgVisualElement;
            Canvas.SelectedElements.Add(pin);

            var formerGlobalSize = pinTool.SelectedPinSize;

            _pinInputServiceMock
                .Setup(p => p.GetUserInput(It.IsAny<IEnumerable<string>>(), It.IsAny<int>()))
                .ReturnsAsync(PinTool.PinSize.Small);

            var command = pinTool.Commands.Single();

            // Act
            await WaitForCanvasInvalidated(() => command.Execute(null));

            // Assert
            Assert.AreEqual(formerGlobalSize, pinTool.SelectedPinSize, "Global pin size should be untouched when a specific pin is selected");
            var updatedPin = Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            Assert.AreEqual("Small", updatedPin.CustomAttributes[PinTool.PinSizeAttributeKey]);
        }

        [Test]
        public async Task WhenUserDoubleTapsOnExistingHoleyPin_FillsPinWithEnteredText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var pinTool = Canvas.Tools.OfType<PinTool>().Single();
            Canvas.ActiveTool = pinTool;

            var position = PointF.Create(50, 50);
            await LongPress(position);

            // PinTool calls GetUserInput("Please enter 1 or 2 characters.", maxTextLength: 2),
            // relying on defaults for the middle params, so MockTextInputService.F only ever
            // sees (title, textValue) where textValue is that unset default - not any text we
            // actually want typed in. Override F directly to control what comes back.
            _textMock.F = (title, textValue) => new TextTool.TextProperties { Text = "AB", FontSizeIndex = 0 };

            // Act
            await DoubleTap(position);

            // Assert
            var pin = Canvas.Document.Children[Canvas.Document.Children.Count - 1];
            Assert.AreEqual("Filled", pin.CustomAttributes[PinTool.PinFillAttributeKey]);
        }

        // --- Gesture helpers -------------------------------------------------

        private async Task Tap(PointF position)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, position, position, position, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, position, position, position, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
        }

        private async Task DoubleTap(PointF position)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, position, position, position, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, position, position, position, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, position, position, position, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, position, position, position, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
        }

        // ReactiveGestureRecognizer's long-press buffer (.Buffer(TimeSpan.FromSeconds(LongPressDuration), 2))
        // doesn't take a scheduler argument, unlike the tap/double-tap buffers, so it runs on
        // real time and TestScheduler.AdvanceBy has no effect on it. We wait past the actual
        // threshold with a real delay instead. LongPressDuration is ~0.66s in the recognizer;
        // padded here to reduce flakiness on slower CI machines.
        private static readonly TimeSpan LongPressRealDelay = TimeSpan.FromMilliseconds(900);

        private async Task LongPress(PointF position)
        {
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, position, position, position, 1));
            await Task.Delay(LongPressRealDelay);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, position, position, position, 1));
        }

        // --- Command synchronization helpers ---------------------------------
        // ChangePinSizeCommand.Execute is "async void", so it can't be awaited directly.
        // Instead of assuming the mocked GetUserInput task completes its continuation
        // synchronously (a Moq implementation detail, not a guarantee), these wait on the
        // actual completion signal the command emits, with a timeout as a safety net against
        // a hang if that signal is never raised.

        private Task WaitForToolCommandsChanged(Action action, int timeoutMs = 2000)
            => WaitForEvent(
                h => Canvas.ToolCommandsChanged += h,
                h => Canvas.ToolCommandsChanged -= h,
                action, timeoutMs, nameof(Canvas.ToolCommandsChanged));

        private Task WaitForCanvasInvalidated(Action action, int timeoutMs = 2000)
            => WaitForEvent(
                h => Canvas.CanvasInvalidated += h,
                h => Canvas.CanvasInvalidated -= h,
                action, timeoutMs, nameof(Canvas.CanvasInvalidated));

        private static async Task WaitForEvent(
            Action<EventHandler> subscribe,
            Action<EventHandler> unsubscribe,
            Action action,
            int timeoutMs,
            string eventNameForError)
        {
            var tcs = new TaskCompletionSource<bool>();
            void Handler(object s, EventArgs e) => tcs.TrySetResult(true);

            subscribe(Handler);
            try
            {
                action();
                var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
                if (completed != tcs.Task)
                {
                    throw new TimeoutException(
                        $"Timed out after {timeoutMs}ms waiting for {eventNameForError} to fire.");
                }
            }
            finally
            {
                unsubscribe(Handler);
            }
        }
    }
}

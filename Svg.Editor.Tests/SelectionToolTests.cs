using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class SelectionToolTests : SvgDrawingCanvasTestBase
    {
        [Test]
        public async Task IfUserTapsCanvas_AndTapPositionIntersectsWithElement_SelectsElement()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            await Canvas.AddItemInScreenCenter(child);
            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.AreSame(child, Canvas.SelectedElements.Single());
        }

        [Test]
        public async Task IfUserTapsCanvas_AndTapPositionDoesNotIntersectWithSelectedElements_Deselects()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            await Canvas.AddItemInScreenCenter(child);

            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));

            // Act
            var pt2 = PointF.Create(0, 0);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt2, pt2, pt2, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt2, pt2, pt2, 1));

            // Assert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task IfUserDrawsSelectionRectangle_AndElementsAreContained_ElementsAreSelected()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            var b1 = element1.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            await Canvas.AddItemInScreenCenter(element2);
            var b2 = element2.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var start = PointF.Create(b1.Left - 1, b1.Top - 1);
            var end = PointF.Create(b2.Right + 1, b2.Bottom + 1);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, end, end, 1));

            // Assert
            Assert.AreEqual(2, Canvas.SelectedElements.Count);
            Assert.AreSame(element2, Canvas.SelectedElements.First(), "z-index wrong?");
            Assert.AreSame(element1, Canvas.SelectedElements.Last(), "z-index wrong?");
        }

        [Test]
        public async Task IfUserDrawsSelectionRectangle_AndElementsAreNotContained_ElementsAreNotSelected()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            var b1 = element1.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            await Canvas.AddItemInScreenCenter(element2);
            var b2 = element2.GetBoundingBox(Canvas.GetCanvasTransformationMatrix());

            // Preassert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);

            // Act
            var start = PointF.Create(b1.Left + 1, b1.Top + 1);
            var end = PointF.Create(b2.Right - 1, b2.Bottom - 1);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, start, start, start, 1));
            await Canvas.OnEvent(new MoveEvent(start, start, end, end - start, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, start, end, end, 1));

            // Assert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task ElementsAreSelected_AndDeleteCommandExecuted_ElementsAreRemoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
            };
            Canvas.Document.Children.Add(element1);
            var d = LoadDocument("nested_transformed_text.svg");
            var element2 = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            await Canvas.AddItemInScreenCenter(element2);
            Canvas.SelectedElements.Add(element1);
            Canvas.SelectedElements.Add(element2);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == element1));
            Assert.True(Canvas.Document.Children.Any(x => x == element2));

            // Act
            tool.Commands.First().Execute(null);

            // Assert
            Assert.False(Canvas.Document.Children.Any(x => x == element1));
            Assert.False(Canvas.Document.Children.Any(x => x == element2));
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;
using Svg.Pathing;
using Svg.Transforms;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class SelectionToolTests : SvgDrawingCanvasTestBase
    {
        protected override void SetupOverride()
        {
            Canvas.LoadTools(() => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()));
        }

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
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
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
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
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
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
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

        [Test]
        public async Task PolygonIsSelectedInside_ShouldNotSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;


            var point1 = PointF.Create(0, 400);
            var point2 = PointF.Create(800, 600);
            var point3 = PointF.Create(800, 0);
            var point4 = PointF.Create(-800, 0);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[]{point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y } );

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                Stroke = new SvgColourServer(Color.Create(23, 23, 23)),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(400, 350);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelectedOutside_ShouldNotSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var point1 = PointF.Create(100, 200);
            var point2 = PointF.Create(200, 100);
            var point3 = PointF.Create(400, 500);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(700, 400);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelectedOnBorder_ShouldSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var point1 = PointF.Create(100, 200);
            var point2 = PointF.Create(200, 100);
            var point3 = PointF.Create(400, 500);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                Stroke = new SvgColourServer(Color.Create(23, 23, 23)),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(210, 110);
            var pt2 = PointF.Create(400, 400);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt2, pt2, pt2, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt2, pt2, pt2, 1));

            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelected_WhenChildPolygonIsTappedOn_ShouldSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;

            var point1 = PointF.Create(100, 200);
            var point2 = PointF.Create(200, 100);
            var point3 = PointF.Create(400, 500);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };

            var point4 = PointF.Create(50, 100);
            var point5 = PointF.Create(100, 500);
            var point6 = PointF.Create(200, 250);

            var collectionPoints2 = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point4.X, point4.Y, point5.X, point5.Y, point6.X, point6.Y });


            polygon.Children.Add(
                new SvgPolygon()
                {
                    Points = collectionPoints2,
                    StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                    FillOpacity = 0
                });

            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(55, 105);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelectedOnVertikalLine_ShouldSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;


            var point1 = PointF.Create(0, 400);
            var point2 = PointF.Create(800, 600);
            var point3 = PointF.Create(820, 0);
            var point4 = PointF.Create(-800, 0);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                Stroke = new SvgColourServer(Color.Create(23, 23, 23)),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(800, 400);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelectedOnHorizontalLine_ShouldSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;


            var point1 = PointF.Create(0, 400);
            var point2 = PointF.Create(800, 600);
            var point3 = PointF.Create(820, 0);
            var point4 = PointF.Create(50, 0);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                Stroke = new SvgColourServer(Color.Create(23, 23, 23)),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(400, 0);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
        }

        [Test]
        public async Task PolygonIsSelectedOnHorizontalLine_WhenClickOutsisde_ShouldSelect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;


            var point1 = PointF.Create(0, 400);
            var point2 = PointF.Create(800, 600);
            var point3 = PointF.Create(820, 0);
            var point4 = PointF.Create(-800, 0);

            var collectionPoints = new SvgPointCollection();
            collectionPoints.AddRange(new SvgUnit[] { point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, point4.X, point4.Y });

            var polygon = new SvgPolygon()
            {
                Points = collectionPoints,
                Stroke = new SvgColourServer(Color.Create(23, 23, 23)),
                StrokeWidth = new SvgUnit(SvgUnitType.Pixel, 10),
                FillOpacity = 0
            };
            Canvas.Document.Children.Add(polygon);

            // Preassert
            Assert.True(Canvas.Document.Children.Any(x => x == polygon));


            // Act
            var pt1 = PointF.Create(400, -5);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler)SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
        }
    }
}


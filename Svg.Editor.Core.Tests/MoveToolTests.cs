using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class MoveToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            Canvas.LoadTools(
                () => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new MoveTool(SvgEngine.Resolve<IUndoRedoService>()));
        }

        [Test]
        public async Task IfPointerIsMoved_AndNoElementIsSelected_NothingIsMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().First(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            var transforms = child.Transforms.Clone();

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);

            // Act
            await Move(PointF.Create(100, 200), PointF.Create(200, 100));

            // Assert
            Assert.AreEqual(transforms, child.Transforms);
        }

        [Test]
        public async Task IfPointerIsMoved_And1ElementIsSelected_ElementIsMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle {X = 50, Y = 50, Width = 50, Height = 50, Fill=new SvgColourServer(Color.Create(255,0,0))};
            var child1 = new SvgRectangle {X = 250, Y = 150, Width = 100, Height = 25, Fill = new SvgColourServer(Color.Create(255, 0, 0)) };
            var child2 = new SvgRectangle {X = 150, Y = 250, Width = 150, Height = 150, Fill = new SvgColourServer(Color.Create(255, 0, 0)) };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);

            // Act
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Assert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);
        }

        [Test]
        public async Task IfPointerIsMoved_AndElementsAreSelected_ElementsAreMoved()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<SelectionTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle {X = 50, Y = 50, Width = 50, Height = 50, Fill = new SvgColourServer(Color.Create(255, 0, 0)) };
            var child1 = new SvgRectangle {X = 250, Y = 150, Width = 100, Height = 25, Fill = new SvgColourServer(Color.Create(255, 0, 0)) };
            var child2 = new SvgRectangle {X = 150, Y = 250, Width = 150, Height = 150, Fill = new SvgColourServer(Color.Create(255, 0, 0)) };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);
            Canvas.SelectedElements.Add(child2);
            var transforms = child.Transforms.Clone();
            var transforms1 = child1.Transforms.Clone();
            var transforms2 = child2.Transforms.Clone();

            // Preassert
            Assert.AreEqual(transforms, child.Transforms);
            Assert.AreEqual(transforms1, child1.Transforms);
            Assert.AreEqual(transforms2, child2.Transforms);

            // Act
            await Move(PointF.Create(75, 75), PointF.Create(200, 100));

            // Assert
            Assert.AreNotEqual(transforms, child.Transforms);
            Assert.AreNotEqual(transforms1, child1.Transforms);
            Assert.AreNotEqual(transforms2, child2.Transforms);
        }

    }
}

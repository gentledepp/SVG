using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class ArrangeToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            Canvas.LoadTools(() => new ArrangeTool(SvgEngine.Resolve<IUndoRedoService>()));
        }

        [Test]
        public async Task IfNoElementIsSelected_ArrangeNotAvailable()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();

            // Assert
            foreach (var command in tool.Commands)
            {
                Assert.False(command.CanExecute(null));
            }
        }

        [Test]
        public async Task If1ElementIsSelected_AndSendBackwardsIsSelected_ElementIsSwappedWithPrecursor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Send backward").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task If1ElementIsSelected_AndSendToBackIsSelected_ElementIsSentToBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.Document.Children.Add(child);
            Canvas.SelectedElements.Add(child);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Send to back").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
        }


        [Test]
        public async Task If1ElementIsSelected_AndBringForwardIsSelected_ElementIsSwappedWithSuccessor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Bring forward").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
        }
        [Test]
        public async Task If1ElementIsSelected_AndBringToFrontIsSelected_ElementIsBroughtToFront()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Bring to front").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndSendBackwardsIsSelected_ElementsAreSwappedWithPrecursor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child2);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) > Canvas.Document.Children.IndexOf(child1));

            // Act
            tool.Commands.First(x => x.Name == "Send backward").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) < Canvas.Document.Children.IndexOf(child1));
        }

        [Test]
        public async Task IfMoreElementsAreSelected_AndSendToBackIsSelected_ElementsAreSentToBack()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.Document.Children.Add(child);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child2);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) > Canvas.Document.Children.IndexOf(child1));

            // Act
            tool.Commands.First(x => x.Name == "Send to back").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child2) < Canvas.Document.Children.IndexOf(child1));
        }


        [Test]
        public async Task IfMoreElementsAreSelected_AndBringForwardIsSelected_ElementsAreSwappedWithSuccessor()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) < Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Bring forward").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) > Canvas.Document.Children.IndexOf(child2));
        }
        [Test]
        public async Task IfMoreElementsAreSelected_AndBringToFrontIsSelected_ElementsAreBroughtToFront()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var tool = Canvas.Tools.OfType<ArrangeTool>().Single();
            Canvas.ActiveTool = tool;

            var child = new SvgRectangle { X = 50, Y = 50, Width = 50, Height = 50 };
            var child1 = new SvgRectangle { X = 250, Y = 150, Width = 100, Height = 25 };
            var child2 = new SvgRectangle { X = 150, Y = 250, Width = 150, Height = 150 };
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            Canvas.Document.Children.Add(child);
            Canvas.Document.Children.Add(child1);
            Canvas.Document.Children.Add(child2);
            Canvas.SelectedElements.Add(child);
            Canvas.SelectedElements.Add(child1);

            // Preassert
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) < Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) < Canvas.Document.Children.IndexOf(child2));

            // Act
            tool.Commands.First(x => x.Name == "Bring to front").Execute(null);

            // Assert
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child1));
            Assert.True(Canvas.Document.Children.IndexOf(child) > Canvas.Document.Children.IndexOf(child2));
            Assert.True(Canvas.Document.Children.IndexOf(child1) > Canvas.Document.Children.IndexOf(child2));
        }

    }
}

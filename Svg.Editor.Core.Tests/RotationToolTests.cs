using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Core.Test
{
    [TestFixture]
    public class RotationToolTests : SvgDrawingCanvasTestBase
    {
        [SetUp]
        protected override void SetupOverride()
        {
            Canvas.LoadTools(
                () => new GridTool(null, SvgEngine.Resolve<IUndoRedoService>()),
                () => new MoveTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new SelectionTool(SvgEngine.Resolve<IUndoRedoService>()),
                () => new RotationTool(null, SvgEngine.Resolve<IUndoRedoService>()));
        }

        [Test]
        public async Task NoElementSelected_Rotate_HasNoEffect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            var matrix = element1.Transforms.GetMatrix();

            // Act
            await Canvas.OnEvent(new RotateEvent(0, 0, RotateStatus.Start, 2));
            await Canvas.OnEvent(new RotateEvent(45, 45, RotateStatus.Rotating, 2));
            await Canvas.OnEvent(new RotateEvent(10, 55, RotateStatus.Rotating, 2));
            await Canvas.OnEvent(new RotateEvent(35, 90, RotateStatus.Rotating, 2));
            await Canvas.OnEvent(new RotateEvent(0, 90, RotateStatus.End, 2));

            // Assert
            Assert.AreEqual(matrix, element1.Transforms.GetMatrix());
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }


        [Test]
        public async Task MultipleElementsSelected_Rotate_HasNoEffect()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 200),
                Y = new SvgUnit(SvgUnitType.Pixel, 200),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            var matrix1 = element1.Transforms.GetMatrix();

            var element2 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 300),
                Y = new SvgUnit(SvgUnitType.Pixel, 300),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element2);
            var matrix2 = element2.Transforms.GetMatrix();

            // Act
            await Rotate(45, 10);

            // Assert
            Assert.AreEqual(matrix1, element1.Transforms.GetMatrix());
            Assert.AreEqual(matrix2, element2.Transforms.GetMatrix());
            Assert.AreEqual(0, Canvas.SelectedElements.Count);
        }
        
        [Test]
        public async Task SingleElementSelected_Rotate_RotatesElementUsingSvgMatrix()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 20),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            matrix.RotateAt(55f, PointF.Create(15, 10), MatrixOrder.Prepend);

            // Act
            await Rotate(45, 10);


            // Assert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");
        }
        
        [Test]
        public async Task SingleElementSelected_RotateTranslateTranslate_ElementsAreRotatedAndTranslatedCorrectly()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var gt = Canvas.Tools.OfType<GridTool>().FirstOrDefault();
            if (gt != null)
                gt.IsSnappingEnabled = false;
            Canvas.ActiveTool = Canvas.Tools.OfType<SelectionTool>().Single();

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 30),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            matrix.RotateAt(90f, PointF.Create(15, 15), MatrixOrder.Prepend);

            // Act
            var from = PointF.Create(15, 10);
            var to = PointF.Create(100, 100);

            await Rotate(45, 45); // rotate to 90 degree
            await Move(@from, to); // move away
            await Move(to, @from); // move back

            // Assert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");
        }
        
        [Test]
        public async Task SingleElementSelected_RotateTranslateTranslateRotate_ElementsAreRotatedAndTranslatedCorrectly()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var gt = Canvas.Tools.OfType<GridTool>().FirstOrDefault();
            if (gt != null)
                gt.IsSnappingEnabled = false;
            Canvas.ActiveTool = Canvas.Tools.OfType<SelectionTool>().Single();

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 30),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            matrix.RotateAt(90f, PointF.Create(15, 15), MatrixOrder.Prepend);

            // Act
            var from = PointF.Create(15, 10);
            var to = PointF.Create(100, 100);

            await Rotate(10, 35); // rotate to 45 degree
            await Move(@from, to); // move away
            await Move(to, @from); // move back
            await Rotate(35, 10); // rotate to 90 degree

            // Assert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");
        }
        
        [Test]
        public async Task SingleElementSelected_RotatesByStepSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var gt = Canvas.Tools.OfType<GridTool>().Single();
            var rt = Canvas.Tools.OfType<RotationTool>().Single();
            gt.IsSnappingEnabled = false;
            rt.RotationStep = 30;

            Canvas.ActiveTool = Canvas.Tools.OfType<SelectionTool>().Single();

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 30),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            matrix.RotateAt(30f, PointF.Create(15, 15), MatrixOrder.Prepend);

            // Act
            await Rotate(10, 33); // rotate to 43 degree => should remain 30 deg

            // Assert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");
        }

        [Test]
        public async Task SingleElementSelected_RotatesByStepSize_2()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var gt = Canvas.Tools.OfType<GridTool>().Single();
            var rt = Canvas.Tools.OfType<RotationTool>().Single();
            gt.IsSnappingEnabled = false;
            rt.RotationStep = 30;

            Canvas.ActiveTool = Canvas.Tools.OfType<SelectionTool>().Single();

            var element1 = new SvgRectangle()
            {
                X = new SvgUnit(SvgUnitType.Pixel, 0),
                Y = new SvgUnit(SvgUnitType.Pixel, 0),
                Width = new SvgUnit(SvgUnitType.Pixel, 30),
                Height = new SvgUnit(SvgUnitType.Pixel, 30),
                Fill = new SvgColourServer(Color.Create(255, 0, 0))
            };
            Canvas.Document.Children.Add(element1);
            Canvas.SelectedElements.Add(element1);
            var matrix = element1.Transforms.GetMatrix();
            matrix.RotateAt(30f, PointF.Create(15, 15), MatrixOrder.Prepend);

            // Act
            await Rotate(+10, +33, -10, -5, +4); // rotate to 43 degree => should remain 30 deg

            // Assert
            var actual = element1.Transforms.GetMatrix();
            AssertAreEqual(matrix, actual);
            Assert.AreEqual(1, Canvas.SelectedElements.Count);
            Assert.IsTrue(Canvas.SelectedElements.Single() == element1, "must still be selected");
        }


        private static void AssertAreEqual(Matrix expected, Matrix actual)
        {
            AssertAreEqual(expected.ScaleX, actual.ScaleX);
            AssertAreEqual(expected.ScaleY, actual.ScaleY);
            AssertAreEqual(expected.OffsetX, actual.OffsetX);
            AssertAreEqual(expected.OffsetY, actual.OffsetY);
            AssertAreEqual(expected.SkewX, actual.SkewX);
            AssertAreEqual(expected.SkewY, actual.SkewY);
        }

        private static void AssertAreEqual(float a, float b)
        {
            Assert.AreEqual(a, b, 0.01);
        }
    }
}

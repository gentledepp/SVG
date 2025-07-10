using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Tests
{
    [TestFixture]
    public class TextToolTests : SvgDrawingCanvasTestBase
    {
        private MockTextInputService _textMock;

        protected override void SetupOverride()
        {
            SvgEngine.Register<ToolFactoryProvider>(() => new ToolFactoryProvider(new Func<ITool>[]
            {
                () => new TextTool(new Dictionary<string, object>
                {
                    { TextTool.FontSizesKey, new [] { 12f, 16f, 20f, 24f, 36f, 48f } },
                    { TextTool.FontSizeNamesKey, new [] { "12px", "16px", "20px", "24px", "36px", "48px" } },
                    { TextTool.SelectedFontSizeIndexKey, 1 },
                }, SvgEngine.Resolve<IUndoRedoService>()),
            }));

            // register mock text input service
            _textMock = new MockTextInputService();
            SvgEngine.Register<ITextInputService>(() => _textMock);
            
        }

        [Test]
        public async Task WhenUserTapsToBlankArea_CreatesText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        public async Task WhenUserTapsOnExistingText_EditsText()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "this is a test",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        public async Task WhenUserTapsOnExistingText_AndOnlyChangesFontSize_EditsFontSize()
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "hello",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = "hello", FontSizeIndex = 0 };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(1, texts.Count);
            var txt = texts.First();
            Assert.AreEqual("hello", txt.Text);
            Assert.AreEqual(txtTool.FontSizes.First(), txt.FontSize.Value);
            Assert.AreEqual(SvgUnitType.Pixel, txt.FontSize.Type);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase((string) null)]
        public async Task WhenUserTapsOnExistingText_AndEntersEmpty_RemovesText(string theText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;
            Canvas.Document.Children.Add(new SvgText()
            {
                Text = "this is a test",
                X = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                Y = new SvgUnitCollection() { new SvgUnit(SvgUnitType.Pixel, 5) },
                FontSize = new SvgUnit(SvgUnitType.Pixel, 20),
            });

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText };

            // Act
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, PointF.Create(10, 10), PointF.Create(10, 10), PointF.Create(10, 10), 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);
        }

        [Test]
        [TestCase("", "  ")]
        [TestCase(" ", "  ")]
        [TestCase("\t", "  ")]
        [TestCase(null, "  ")]
        [TestCase("hello from svg", "hello from svg")]
        public async Task WhenUserTapsNestedTextSpan_EditsTextSpan(string theText, string expectedText)
        {
            // Arrange
            await Canvas.EnsureInitialized();
            var txtTool = Canvas.Tools.OfType<TextTool>().Single();
            Canvas.ActiveTool = txtTool;

            var d = LoadDocument("nested_transformed_text.svg");
            var child = d.Children.OfType<SvgVisualElement>().Single(c => c.Visible && c.Displayable);
            Canvas.ScreenWidth = 800;
            Canvas.ScreenHeight = 500;
            await Canvas.AddItemInScreenCenter(child);

            _textMock.F = (x, y) => new TextTool.TextProperties { Text = theText };
            
            var txt = Canvas.Document.Descendants().OfType<SvgText>().Single();

            // Act
            var pt1 = PointF.Create(370, 260);
            await Canvas.OnEvent(new PointerEvent(EventType.PointerDown, pt1, pt1, pt1, 1));
            await Canvas.OnEvent(new PointerEvent(EventType.PointerUp, pt1, pt1, pt1, 1));
            ((TestScheduler) SchedulerProvider.BackgroundScheduler).AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            // Assert
            var texts = Canvas.Document.Children.OfType<SvgTextBase>().ToList();
            Assert.AreEqual(0, texts.Count);

            var nestedText = Canvas.Document.Descendants().OfType<SvgTextSpan>().Single();
            Assert.AreEqual(expectedText, nestedText.Text);

            //assert that after saving and loading the text is still the same
            using (var ms = new MemoryStream())
            {
                Canvas.Document.Write(ms);

                var str = System.Text.Encoding.UTF8.GetString(ms.ToArray());

                ms.Seek(0, SeekOrigin.Begin);

                var loadedDoc = SvgDocument.Open<SvgDocument>(ms);
                var nestedLoaded = loadedDoc.Descendants().OfType<SvgTextSpan>().Single();
                Assert.AreEqual(expectedText, nestedLoaded.Text);
                var parent = nestedLoaded.Parent;
                Assert.AreEqual(0, parent.Nodes.OfType<SvgContentNode>().Count());
            }
        }
    }
}

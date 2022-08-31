using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Svg.Editor.Tests;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SaveDocumentTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
            Svg.SvgEngine.Register<IFileLoader>(() => new FileLoader());
        }

        [Test]
        public void SavingDocument_KeepsInheritedAttributesIntact()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                StrokeDashArray = SvgUnitCollection.Inherit,
                                Fill = SvgColourServer.Inherit,
                                Stroke = SvgColourServer.Inherit
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("3 3", g.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreEqual("#ff0000", r.Fill.ToString());
            Assert.AreEqual("3 3", r.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", r.Stroke.ToString());
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        [Test]
        public void SavingDocument_KeepsUnserAttributesIntact()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                StrokeDashArray = null,
                                Fill = null,
                                Stroke = null
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("3 3", g.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreEqual("#ff0000", r.Fill.ToString());
            Assert.AreEqual("3 3", r.StrokeDashArray.ToString());
            Assert.AreEqual("#00ff00", r.Stroke.ToString());
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        [Test]
        public void SavingDocument_KeepsNoneIfNoneIsSetExplicitly()
        {
            // Arrange
            var doc = new SvgDocument()
            {
                Children =
                {
                    new SvgGroup()
                    {
                        Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                        Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                        Children =
                        {
                            new SvgRectangle()
                            {
                                X = 100,
                                Y = 150,
                                Width = 300,
                                Height = 50,
                                Fill = SvgPaintServer.None,
                                Stroke = SvgPaintServer.None
                            }
                        }
                    }
                }
            };
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            // Assert
            Assert.IsNotNull(doc2);
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            Assert.AreEqual("#ff0000", g.Fill.ToString());
            Assert.AreEqual("#00ff00", g.Stroke.ToString());

            var r = g.Children.OfType<SvgRectangle>().Single();
            Assert.AreEqual(100, r.X.Value);
            Assert.AreEqual(150, r.Y.Value);
            Assert.AreEqual(300, r.Width.Value);
            Assert.AreEqual(50, r.Height.Value);
            Assert.AreSame(SvgPaintServer.None, r.Fill);
            Assert.AreSame(SvgPaintServer.None, r.Stroke);
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }
        
        /*
         * style="fill:none;fill-opacity:0;stroke:none"
         */

        [Test]
        public void WhenSavingDocument_KeepNamespacesIntact()
        {
            var fileLoader = SvgEngine.Resolve<IFileLoader>();
            var document = fileLoader.Load("Bends_01.svg");
            SvgDocument doc2 = null;

            // Act
            using (var ms = new MemoryStream())
            {
                document.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
            }

            Assert.True(doc2.Children.First(c => c.ElementName == "sodipodi:namedview").Children.Any(c => c.ElementName == "inkscape:grid"));
        }

        [Test]
        public void CanSaveEmptyDocument()
        {
            // Arrange
            var doc = new SvgDocument();
            SvgDocument doc2 = null;
            var expectedSvg = "﻿<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?><svg xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" version=\"1.1\" width=\"100%\" height=\"100%\" preserveAspectRatio=\"xMidYMid\" xmlns=\"http://www.w3.org/2000/svg\" />";
            var svg = string.Empty;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms);
                svg = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Assert
            Assert.IsNotNull(doc2);
            Assert.AreEqual(expectedSvg, svg);
        }

        [Ignore("test case file got lost... 🤷‍♂️")]
        [Test]
        [TestCase("nested_transformed_text.svg")]
        public void CanLoad_Save_AndReload_Document(string testFile)
        {
            var fileLoader = SvgEngine.Resolve<IFileLoader>();
            var document = fileLoader.Load(testFile);
            SvgDocument document2 = null;

            var saved1 = string.Empty;
            var saved2 = string.Empty;

            // Act
            using (var ms = new MemoryStream())
            {
                document.Write(ms);
                saved1 = Encoding.UTF8.GetString(ms.ToArray());
                ms.Seek(0, SeekOrigin.Begin);
                document2 = SvgDocument.Open<SvgDocument>(ms);
            }

            using (var ms = new MemoryStream())
            {
                document2.Write(ms);
                saved2 = Encoding.UTF8.GetString(ms.ToArray());
            }

            Assert.AreEqual(saved1, saved2);
        }

        private static void AssertInheritedAttribute(SvgRectangle r, string attributeName)
        {
            string val;
            if (r.TryGetAttribute("attributeName", out val))
                Assert.AreEqual("inherit", val);
        }
    }
}

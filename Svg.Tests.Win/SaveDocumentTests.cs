using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Svg.Editor.Tests;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    /// <summary>
    /// Tests for document saving functionality, ensuring that attributes, inheritance,
    /// and document structure are preserved during save/load operations.
    /// </summary>
    [TestFixture]
    public class SaveDocumentTests
    {
        /// <summary>
        /// Initializes the SVG platform and registers the file loader before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
            Svg.SvgEngine.Register<IFileLoader>(() => new FileLoader());
        }

        /// <summary>
        /// Verifies that when saving a document with inherited attributes, the inheritance
        /// is preserved and the computed values are correctly applied to child elements.
        /// </summary>
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
            doc2.ShouldNotBeNull();
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            g.Fill.ToString().ShouldBe("#ff0000");
            g.StrokeDashArray.ToString().ShouldBe("3 3");
            g.Stroke.ToString().ShouldBe("#00ff00");

            var r = g.Children.OfType<SvgRectangle>().Single();
            r.X.Value.ShouldBe(100);
            r.Y.Value.ShouldBe(150);
            r.Width.Value.ShouldBe(300);
            r.Height.Value.ShouldBe(50);
            r.Fill.ToString().ShouldBe("#ff0000");
            r.StrokeDashArray.ToString().ShouldBe("3 3");
            r.Stroke.ToString().ShouldBe("#00ff00");
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        /// <summary>
        /// Verifies that when saving a document with unset attributes (null values),
        /// the inheritance behavior is correctly preserved after save/load operations.
        /// </summary>
        [Test]
        public void SavingDocument_KeepsUnsetAttributesIntact()
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
            doc2.ShouldNotBeNull();
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            g.Fill.ToString().ShouldBe("#ff0000");
            g.StrokeDashArray.ToString().ShouldBe("3 3");
            g.Stroke.ToString().ShouldBe("#00ff00");

            var r = g.Children.OfType<SvgRectangle>().Single();
            r.X.Value.ShouldBe(100);
            r.Y.Value.ShouldBe(150);
            r.Width.Value.ShouldBe(300);
            r.Height.Value.ShouldBe(50);
            r.Fill.ToString().ShouldBe("#ff0000");
            r.StrokeDashArray.ToString().ShouldBe("3 3");
            r.Stroke.ToString().ShouldBe("#00ff00");
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        /// <summary>
        /// Verifies that when an element explicitly sets attributes to "none",
        /// this value is preserved during save/load operations rather than being inherited.
        /// </summary>
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
            doc2.ShouldNotBeNull();
            var g = doc2.Children.OfType<SvgVisualElement>().Single();
            g.Fill.ToString().ShouldBe("#ff0000");
            g.Stroke.ToString().ShouldBe("#00ff00");

            var r = g.Children.OfType<SvgRectangle>().Single();
            r.X.Value.ShouldBe(100);
            r.Y.Value.ShouldBe(150);
            r.Width.Value.ShouldBe(300);
            r.Height.Value.ShouldBe(50);
            r.Fill.ShouldBeSameAs(SvgPaintServer.None);
            r.Stroke.ShouldBeSameAs(SvgPaintServer.None);
            AssertInheritedAttribute(r, "stroke");
            AssertInheritedAttribute(r, "fill");
            AssertInheritedAttribute(r, "stroke-dasharray");
        }

        /// <summary>
        /// Verifies that XML namespaces are preserved during save/load operations,
        /// ensuring compatibility with tools like Inkscape and Sodipodi.
        /// </summary>
        [Test]
        public void WhenSavingDocument_KeepNamespacesIntact()
        {
            // Arrange
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

            // Assert
            doc2.Children.First(c => c.ElementName == "sodipodi:namedview")
                .Children.ShouldContain(c => c.ElementName == "inkscape:grid");
        }

        /// <summary>
        /// Verifies that an empty SVG document can be saved and produces the expected XML output
        /// with all necessary namespace declarations and default attributes.
        /// </summary>
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
            doc2.ShouldNotBeNull();
            svg.ShouldBe(expectedSvg);
        }

        /// <summary>
        /// Verifies that documents can be loaded, saved, and reloaded while maintaining
        /// identical XML output, ensuring no data loss during round-trip operations.
        /// </summary>
        [Ignore("test case file got lost... 🤷‍♂️")]
        [Test]
        [TestCase("nested_transformed_text.svg")]
        public void CanLoad_Save_AndReload_Document(string testFile)
        {
            // Arrange
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

            // Assert
            saved1.ShouldBe(saved2);
        }

        /// <summary>
        /// Asserts that the specified attribute on the given rectangle element
        /// has the value "inherit" when retrieved from the raw XML attributes.
        /// </summary>
        /// <param name="r">The rectangle element to check</param>
        /// <param name="attributeName">The name of the attribute to verify</param>
        private static void AssertInheritedAttribute(SvgRectangle r, string attributeName)
        {
            if (r.TryGetAttribute(attributeName, out string val))
                val.ShouldBe("inherit");
        }
    }
}
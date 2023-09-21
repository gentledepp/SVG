using FluentAssertions;
using Moq;
using NUnit.Framework;
using Svg.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class SvgRendererTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void CanBeReusedEvenWhenDisposed()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
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
                            rect
                        }
                    }
                }
            };

            var renderer = SvgRenderer.FromNull();

            // Act (render twice - should be reused)
            doc.Draw(renderer);
            doc.Draw(renderer);

            // Assert
            var rcem = rect.RenderCacheEntryMocks.Single();
            rcem.VerifySet(s => s.StrokeBrush = It.IsAny<Brush>(), Times.Once());
            rcem.VerifySet(s => s.FillBrush = It.IsAny<Brush>(), Times.Once());

            renderer.Dispose();

            // Assert 2
            rcem.Verify(s => s.Dispose(), Times.Once());
            rect.RenderCacheEntryMocks.Count.Should().Be(1, "only one cache entry should be created and then cached!");
        }

        [Test]
        public void CanBeSharedByRenderers()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
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
                            rect
                        }
                    }
                }
            };

            var renderer = SvgRenderer.FromNull();
            var renderer2 = SvgRenderer.FromNull();

            doc.Draw(renderer);
            doc.Draw(renderer2);
            doc.Draw(renderer);

            // Act (render twice - should be reused)
            renderer.Dispose();
            doc.Draw(renderer2);

            // Assert
            rect.RenderCacheEntryMocks.Count.Should().Be(2, "only one cache entry per renderer should be created and then cached!");
            var r1 = rect.RenderCacheEntryMocks[0];
            var r2 = rect.RenderCacheEntryMocks[1];
            r1.VerifySet(s => s.StrokeBrush = It.IsAny<Brush>(), Times.Exactly(1), "once per renderer");
            r1.VerifySet(s => s.FillBrush = It.IsAny<Brush>(), Times.Exactly(1), "once per renderer");
            r2.VerifySet(s => s.StrokeBrush = It.IsAny<Brush>(), Times.Exactly(1), "once per renderer");
            r2.VerifySet(s => s.FillBrush = It.IsAny<Brush>(), Times.Exactly(1), "once per renderer");

            renderer2.Dispose();

            // Assert 2

            r1.Verify(s => s.Dispose(), Times.Exactly(1), "once per renderer");
            r2.Verify(s => s.Dispose(), Times.Exactly(1), "once per renderer");
        }

        [Test]
        public void WhenAttributeChanges_BeforeRender_DoesNotResetCache()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
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
                            rect
                        }
                    }
                }
            };
            using var renderer = SvgRenderer.FromNull();

            // Act
            rect.FontFamily = "Arial";
            doc.Draw(renderer);

            // Assert
            rect.GetAttributeChangeToken().Should().Be(Guid.Empty);
        }

        [Test]
        public void WhenAttributeChanges_AfterFirstRender_ResetsCache()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
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
                            rect
                        }
                    }
                }
            };

            using var renderer = SvgRenderer.FromNull();
            doc.Draw(renderer);

            // Act
            rect.FontFamily = "Arial";
            doc.Draw(renderer);

            // Assert
            var act = rect.GetAttributeChangeToken();
            act.Should().NotBe(Guid.Empty);
            rect.RenderCacheEntryMocks.Single().Verify(s => s.SetAttributeChangeToken(act), Times.Once(), "must be called so it can reset its disposable properties");
        }

        [Test]
        public void WhenInheritedAttributeChanges_BeforeRender_DoesNotResetCache()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
            var group = new SvgGroup()
            {
                Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                Children =
                {
                    rect
                }
            };
            var doc = new SvgDocument()
            {
                Children =
                {
                    group
                }
            };

            using var renderer = SvgRenderer.FromNull();
            doc.Draw(renderer);

            // Act
            group.Fill = new SvgColourServer(Color.Create(0, 255, 0));
            doc.Draw(renderer);

            // Assert
            var act = rect.GetAttributeChangeToken();
            act.Should().NotBe(Guid.Empty);
            rect.RenderCacheEntryMocks.Single().Verify(s => s.SetAttributeChangeToken(act), Times.Once(), "must be called so it can reset its disposable properties");
        }

        [Test]
        public void WhenInheritedAttributeChanges_AfterFirstRender_ResetsCache()
        {
            // Arrange
            var rect = new MockableSvgRectangle()
            {
                X = 100,
                Y = 150,
                Width = 300,
                Height = 50,
                StrokeDashArray = null,
                Fill = null,
                Stroke = null
            };
            var group = new SvgGroup()
            {
                Fill = new SvgColourServer(Color.Create(255, 0, 0)),
                StrokeDashArray = new SvgUnitCollection { new SvgUnit(3), new SvgUnit(3) },
                Stroke = new SvgColourServer(Color.Create(0, 255, 0)),

                Children =
                {
                    rect
                }
            };
            var doc = new SvgDocument()
            {
                Children =
                {
                    group
                }
            };

            using var renderer = SvgRenderer.FromNull();
            doc.Draw(renderer);

            // Act
            group.Fill = new SvgColourServer(Color.Create(0, 255, 0));
            doc.Draw(renderer);

            // Assert
            var act = rect.GetAttributeChangeToken();
            act.Should().NotBe(Guid.Empty);
            rect.RenderCacheEntryMocks.Single().Verify(s => s.SetAttributeChangeToken(act), Times.Once(), "must be called so it can reset its disposable properties");
        }

        public class MockableSvgRectangle : SvgRectangle
        {
            private Mock<RenderCacheEntry> CreateMock()
            {
                var r = new Mock<RenderCacheEntry>();
                r.SetupAllProperties();
                DisposableMock = r.As<IDisposable>();
                return r;
            }
            
            public Mock<IDisposable> DisposableMock { get; set; }
            internal List<Mock<RenderCacheEntry>> RenderCacheEntryMocks { get; } = new List<Mock<RenderCacheEntry>>();

            protected override T CreateRenderCacheEntry<T>()
            {
                var r = CreateMock();
                RenderCacheEntryMocks.Add(r);

                return (T)(object)r.Object;
            }

            public Guid GetAttributeChangeToken() => AttributeChangeToken;
        }
    }
}

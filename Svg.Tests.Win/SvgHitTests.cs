using System.Linq;
using Shouldly;
using NUnit.Framework;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public  class SvgHitTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }
        
        [TestCase(0f,0f,5, false)]
        [TestCase(100f,100f,5, true)]
        public void Intersect_Rectangle_WithoutTransformations_IsHit(float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <rect x=""100"" y=""100"" width=""50"" height=""50""  style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }
        
        [TestCase(0f, 0f, 100, 100, false)]
        [TestCase(100f, 100f, 50,50, true)]
        [TestCase(101f, 100f, 50,50, false)]
        [TestCase(100f, 101f, 50,50, false)]
        [TestCase(100f, 100f, 49,50, false)]
        [TestCase(100f, 100f, 50,49, false)]
        [TestCase(99f, 99f, 50,50, false)]
        [TestCase(99f, 99f, 51,51, true)]
        public void Contains_Rectangle_WithoutTransformations_IsHit(float x, float y, float w, float h, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <rect x=""100"" y=""100"" width=""50"" height=""50""  style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, w, h);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Contain, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase(-2000f, -2000f, 5, false)]
        [TestCase(-1900f, -1900f, 5, true)]
        public void Intersect_Rectangle_SvgDocumentHasTransformations_IsHit(float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg transform=""translate(-2000,-2000)"">
    <rect x=""100"" y=""100"" width=""50"" height=""50""  style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase(-2000f, -2000f, 50,50, false)]
        [TestCase(-1900f, -1900f, 50, 50, true)]
        [TestCase(-1899f, -1900f, 50, 50, false)]
        public void Contains_Rectangle_SvgDocumentHasTransformations_IsHit(float x, float y, float w, float h, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg transform=""translate(-2000,-2000)"">
    <rect x=""100"" y=""100"" width=""50"" height=""50""  style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, w, h);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Contain, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase(-2000f, -2000f, 5, false)]
        [TestCase(-1900f, -1900f, 5, true)]
        public void Intersect_Rectangle_HasTransformations_IsHit(float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <rect transform=""translate(-2000,-2000)""
            x=""100"" y=""100"" width=""50"" height=""50""  style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase(-2000f, -2000f, 50, 50, false)]
        [TestCase(-1900f, -1900f, 50, 50, true)]
        [TestCase(-1899f, -1900f, 50, 50, false)]
        public void Contains_Rectangle_HasTransformations_IsHit(float x, float y, float w, float h, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <rect transform=""translate(-2000,-2000)""
        x=""100"" y=""100"" width=""50"" height=""50"" 
        style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, w, h);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Contain, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase(-100f, -100f, 5, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-100f, -100f, 5, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(0f, 0f, 50, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(0f, 0f, 50, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Intersect_Group_WithoutTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <g> 
        <circle cx=""0"" cy=""0"" r=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Intersect, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }

        [TestCase(-100f, -100f, 50, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-100f, -100f, 50, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(-25f, -25f, 75, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(0f, 0f, 50, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Contains_Group_WithoutTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <g> 
        <circle cx=""0"" cy=""0"" r=""25"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Contain, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }
        
        [TestCase(-2100f, -2100f, 5, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-2100f, -2100f, 5, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(-2000f, -2000f, 5, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(-2000f, -2000f, 5, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Intersect_Group_SvgDocumentHasTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg transform=""translate(-2000,-2000)"">
    <g> 
        <circle cx=""0"" cy=""0"" r=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Intersect, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }

        [TestCase(-2100f, -2100f, 75, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-2100f, -2100f, 75, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(-2025f, -2025f, 75, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(-2000f, -2000f, 75, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Contains_Group_SvgDocumentHasTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg transform=""translate(-2000,-2000)"">
    <g> 
        <circle cx=""0"" cy=""0"" r=""25"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Contain, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }

        [TestCase(-2100f, -2100f, 5, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-2100f, -2100f, 5, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(-2000f, -2000f, 5, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(-2000f, -2000f, 5, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Intersect_Group_HasTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <g transform=""translate(-2000,-2000)""> 
        <circle cx=""0"" cy=""0"" r=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Intersect, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }

        [TestCase(-2100f, -2100f, 75, HitTestResultMode.ReturnAllMatchingDescendants, false)]
        [TestCase(-2100f, -2100f, 75, HitTestResultMode.ReturnRootElementOnly, false)]
        [TestCase(-2025f, -2025f, 75, HitTestResultMode.ReturnAllMatchingDescendants, true)]
        [TestCase(-2000f, -2000f, 75, HitTestResultMode.ReturnRootElementOnly, true)]
        public void Contains_Group_HasTransformations_IsHit(float x, float y, float wh, HitTestResultMode hitTestResultMode, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <g transform=""translate(-2000,-2000)""> 
        <circle cx=""0"" cy=""0"" r=""25"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
        <rect x=""0"" y=""0"" width=""50"" height=""50"" style=""fill:rgb(0,0,255);stroke-width:3;stroke:rgb(0,0,0)"" />
    </g>
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgVisualElement>(rect, SelectionType.Contain, hitTestResultMode)
                .ToArray();

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
            {
                if (hitTestResultMode == HitTestResultMode.ReturnAllMatchingDescendants)
                {
                    result.Count().ShouldBe(3);
                    result[0].ShouldBeOfType<SvgRectangle>();
                    result[1].ShouldBeOfType<SvgCircle>();
                    result[2].ShouldBeOfType<SvgGroup>();
                }
                else if (hitTestResultMode == HitTestResultMode.ReturnRootElementOnly)
                {
                    result.Count().ShouldBe(1);
                    result.ShouldAllBe(g => g is SvgGroup);
                }
            }
        }

        [TestCase("outside tap", 75f, 75f, 10, false)]
        [TestCase("bounding box corner tap (outside circle outline) w/o fill", 100f, 100f, 10, false)]
        [TestCase("top border tap w/o fill", 150f, 100f, 10, true)]
        [TestCase("right border tap w/o fill", 200f, 150f, 10, true)]
        [TestCase("bottom border tap w/o fill", 150f, 200f, 10, true)]
        [TestCase("left border tap w/o fill", 100f, 150f, 10, true)]
        [TestCase("center tap w/o fill", 150f, 150f, 10, false)]
        public void Intersect_Circle_WithoutFill_DoesNotHitCenter(string ___, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = @"<svg>
    <circle cx=""150"" cy=""150"" r=""50"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgCircle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("outside tap", 75f, 75f, 10, false)]
        [TestCase("bounding box corner tap (outside ellipse outline) w/o fill", 100f, 120f, 10, false)]
        [TestCase("top border tap w/o fill", 150f, 120f, 10, true)]
        [TestCase("right border tap w/o fill", 200f, 150f, 10, true)]
        [TestCase("bottom border tap w/o fill", 150f, 180f, 10, true)]
        [TestCase("left border tap w/o fill", 100f, 150f, 10, true)]
        [TestCase("center tap w/o fill", 150f, 150f, 10, false)]
        public void Intersect_Ellipse_WithoutFill_DoesNotHitCenter(string ___, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange
            // this is the shape actually produced by EllipseTool.CreateShape (Fill = SvgPaintServer.None)
            var rawSvg = @"<svg>
    <ellipse cx=""150"" cy=""150"" rx=""50"" ry=""30"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgEllipse>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("thin stroke - tap is too far from the exact border to count", 1f, false)]
        [TestCase("thick stroke - fat finger tolerance grows with the visible border", 30f, true)]
        public void Intersect_Rectangle_WithoutFill_WiderStrokeWidensBorderHitTolerance(string ___, float strokeWidth, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"<svg>
    <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:{strokeWidth}"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units left of the rectangle's left edge (x=100): too far for a 1px stroke, within
            // tolerance for a 30px stroke's rendered band
            var rect = RectangleF.Create(85f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [Test]
        public void Intersect_Rectangle_WithoutFill_PercentageStrokeWidthUsesDocumentRelativeTolerance()
        {
            // Arrange
            // svg is 400x300, so per the SVG spec's diagonal formula, a 6% stroke-width resolves to
            // sqrt(400^2+300^2)/sqrt(2) * 6/100 =~ 21.2 device units of tolerance (~10.6 half-width).
            // treating "6" as if it were already pixels (ignoring the % unit) would instead give a
            // bogus ~3 unit half-width tolerance, and this tap would then incorrectly be a miss.
            var rawSvg = @"<svg width=""400"" height=""300"">
    <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:6%"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units left of the rectangle's left edge (x=100)
            var rect = RectangleF.Create(85f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            result.Count().ShouldBe(1);
        }

        [Test]
        public void Intersect_Rectangle_WithoutFillAndWithoutStroke_DoesNotGetExtraTolerance()
        {
            // Arrange
            // no stroke color set at all - HasStroke() must be false, so the (irrelevant) default
            // StrokeWidth must not silently widen the hit area
            var rawSvg = @"<svg>
    <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:none;stroke:none"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(85f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            result.ShouldBeEmpty();
        }

        [TestCase("thin stroke - tap is too far from the exact border to count", 1f, false)]
        [TestCase("thick stroke - fat finger tolerance grows with the visible border", 30f, true)]
        public void Intersect_Line_WiderStrokeWidensBorderHitTolerance(string ___, float strokeWidth, bool expectsHitSuccessful)
        {
            // Arrange
            var rawSvg = $@"<svg>
    <line x1=""100"" y1=""100"" x2=""200"" y2=""100"" style=""stroke:rgb(0,0,0);stroke-width:{strokeWidth}"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units above the horizontal line (y=100): too far for a 1px stroke, within
            // tolerance for a 30px stroke's rendered band
            var rect = RectangleF.Create(145f, 85f, 10, 10);

            // Act
            var result = svg.HitTest<SvgLine>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("thin stroke - tap is too far from the exact border to count", 1f, false)]
        [TestCase("thick stroke - fat finger tolerance grows with the visible border", 30f, true)]
        public void Intersect_Polygon_WithoutFill_WiderStrokeWidensBorderHitTolerance(string ___, float strokeWidth, bool expectsHitSuccessful)
        {
            // Arrange
            // a square drawn as a polygon, same bounds as the rectangle test above
            var rawSvg = $@"<svg>
    <polygon points=""100,100 200,100 200,200 100,200"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:{strokeWidth}"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units left of the left edge (x=100): too far for a 1px stroke, within
            // tolerance for a 30px stroke's rendered band
            var rect = RectangleF.Create(85f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgPolygon>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("thin stroke - tap is too far from the exact border to count", 1f, false)]
        [TestCase("thick stroke - fat finger tolerance grows with the visible border", 30f, true)]
        public void Intersect_Polyline_WithoutFill_WiderStrokeWidensBorderHitTolerance(string ___, float strokeWidth, bool expectsHitSuccessful)
        {
            // Arrange
            // an "L" shape: top edge (100,100)-(200,100), then down to (200,200)
            var rawSvg = $@"<svg>
    <polyline points=""100,100 200,100 200,200"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:{strokeWidth}"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units above the top edge (y=100): too far for a 1px stroke, within
            // tolerance for a 30px stroke's rendered band
            var rect = RectangleF.Create(145f, 85f, 10, 10);

            // Act
            var result = svg.HitTest<SvgPolyline>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("thin stroke - tap is too far from the exact border to count", 1f, false)]
        [TestCase("thick stroke - fat finger tolerance grows with the visible border", 30f, true)]
        public void Intersect_Path_WithoutFill_WiderStrokeWidensBorderHitTolerance(string ___, float strokeWidth, bool expectsHitSuccessful)
        {
            // Arrange
            // same "L" shape as the polyline test above, drawn as a path
            var rawSvg = $@"<svg>
    <path d=""M100,100 L200,100 L200,200"" style=""fill:none;stroke:rgb(0,0,0);stroke-width:{strokeWidth}"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap 10 units above the top edge (y=100): too far for a 1px stroke, within
            // tolerance for a 30px stroke's rendered band
            var rect = RectangleF.Create(145f, 85f, 10, 10);

            // Act
            var result = svg.HitTest<SvgPath>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        // ---- precise fill-aware interior hit testing (PR #76 review) ---------------------------------

        [TestCase("center tap w fill hits interior", 145f, 145f, 10, true)]
        [TestCase("bounding box corner tap w fill (inside bbox, outside circle) is a miss", 105f, 105f, 10, false)]
        public void Intersect_Circle_WithFill_HitsInteriorNotBoundingBoxCorner(string ___, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange - filled circle: interior selectable, but only inside the actual outline (not the bbox corners)
            var rawSvg = @"<svg>
    <circle cx=""150"" cy=""150"" r=""50"" style=""fill:lime;stroke:rgb(0,0,0);stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgCircle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("center tap w fill hits interior", 145f, 145f, 10, true)]
        [TestCase("bounding box corner tap w fill (inside bbox, outside ellipse) is a miss", 102f, 122f, 10, false)]
        public void Intersect_Ellipse_WithFill_HitsInteriorNotBoundingBoxCorner(string ___, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange - filled ellipse: interior selectable, but only inside the actual outline
            var rawSvg = @"<svg>
    <ellipse cx=""150"" cy=""150"" rx=""50"" ry=""30"" style=""fill:lime;stroke:rgb(0,0,0);stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgEllipse>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("tap inside the filled body hits", 110f, 145f, 10, true)]
        [TestCase("tap in the concave notch (inside bbox, outside the shape) is a miss", 175f, 115f, 10, false)]
        public void Intersect_ConcavePolygon_WithFill_HitsBodyNotNotch(string ___, float x, float y, float wh, bool expectsHitSuccessful)
        {
            // Arrange - an "L" shaped (concave) polygon. Its bounding box is 100,100-200,200 but the top-right
            // quadrant is a notch that is NOT part of the shape. A filled shape must only be hit inside its
            // real outline, not anywhere in the bounding box.
            var rawSvg = @"<svg>
    <polygon points=""100,100 100,200 200,200 200,170 130,170 130,100"" style=""fill:lime;stroke:purple;stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, wh, wh);

            // Act
            var result = svg.HitTest<SvgPolygon>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("opaque fill - interior is a hit", "1", true)]
        [TestCase("fully transparent fill (no visible background) - interior is a miss", "0", false)]
        public void Intersect_Rectangle_FillOpacityDeterminesInteriorHit(string ___, string fillOpacity, bool expectsHitSuccessful)
        {
            // Arrange - no stroke, so the only way to hit the interior is via a *visible* fill. fill-opacity:0
            // renders with no background and must therefore behave like an unfilled shape.
            var rawSvg = $@"<svg>
    <rect x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:red;fill-opacity:{fillOpacity};stroke:none"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap dead center of the rectangle
            var rect = RectangleF.Create(145f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("non-zero fill rule - the star center is filled", "nonzero", true)]
        [TestCase("even-odd fill rule - the star center is a hole", "evenodd", false)]
        public void Intersect_SelfIntersectingPolygon_FillRuleDeterminesInterior(string ___, string fillRule, bool expectsHitSuccessful)
        {
            // Arrange - a pentagram (self-intersecting). Its central pentagon is filled under the non-zero rule
            // but is a hole under the even-odd rule, so the fill-rule alone decides whether a tap in the center
            // is a hit. Vertices are the 5 points of a star, centered on (150,150).
            var rawSvg = $@"<svg width=""300"" height=""300"">
    <polygon points=""150,50 209,231 55,119 245,119 91,231"" style=""fill:lime;fill-rule:{fillRule};stroke:purple;stroke-width:1"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            // tap the center of the star (well away from any edge, so only the fill decides the outcome)
            var rect = RectangleF.Create(145f, 145f, 10, 10);

            // Act
            var result = svg.HitTest<SvgPolygon>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [TestCase("tap in the hole (inner square) - happens to be excluded, but for the wrong reason", 145f, 145f, false)]
        [TestCase("tap in the donut body (between outer and inner squares) - WRONGLY excluded - this is the real bug", 110f, 145f, false)]
        public void Intersect_MultiContourDonutPath_ObservesActualGetLinesBehavior(string ___, float x, float y, bool expectsHitSuccessful)
        {
            // Arrange - a "donut": an outer square with an inner square subpath cut out via fill-rule evenodd.
            // Correct SVG semantics: the hole (130,130-170,170) is unfilled, the ring between the two squares
            // IS filled - so a tap in the donut body (110,145) should be a HIT.
            // IntersectsWith's own NOTE flags the known limitation: PathData.GetLines() flattens both subpaths
            // into one vertex ring (no subpath break), so this does not evaluate the two contours independently
            // - the merged ring ends up self-intersecting instead of forming a real donut.
            // Measured directly (do not assume): the hole tap happens to still read as a miss, but the donut-body
            // tap - which should be a hit - is ALSO wrongly read as a miss. So the current implementation doesn't
            // just fail to carve out the hole; for this shape it fails to detect the filled body at all. This
            // test pins that actual (broken) behavior so it's a visible, tracked limitation rather than a
            // rediscovered surprise - fixing GetLines() to preserve subpath breaks is the real fix, out of scope here.
            var rawSvg = @"<svg>
    <path d=""M100,100 L200,100 L200,200 L100,200 Z M130,130 L170,130 L170,170 L130,170 Z"" style=""fill:lime;fill-rule:evenodd;stroke:none"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(x, y, 10, 10);

            // Act
            var result = svg.HitTest<SvgPath>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants);

            // Assert
            if (!expectsHitSuccessful)
                result.ShouldBeEmpty();
            else
                result.Count().ShouldBe(1);
        }

        [Test]
        public void Intersect_TwoLayeredRectangles_OnlySelectsTopLayerRectangle()
        {
            // Arrange
            // "top" is declared last, so it is rendered on top of (and occludes) "bottom"
            var rawSvg = @"<svg>
    <rect id=""bottom"" x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:rgb(255,0,0);stroke:none"" />
    <rect id=""top"" x=""100"" y=""100"" width=""100"" height=""100"" style=""fill:rgb(0,0,255);stroke:none"" />
</svg>";
            var svg = SvgDocument.FromSvg<SvgDocument>(rawSvg);
            var rect = RectangleF.Create(150f, 150f, 10, 10);

            // Act
            var result = svg.HitTest<SvgRectangle>(rect, SelectionType.Intersect, HitTestResultMode.ReturnAllMatchingDescendants)
                .ToArray();

            // Assert
            // both rectangles are hit, but the top-most (highest z-order) one must be reported first,
            // so that a caller who only cares about a single selection (e.g. result.First()) selects "top"
            result.Count().ShouldBe(2);
            result[0].ID.ShouldBe("top");
        }
    }
}

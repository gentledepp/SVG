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
    }
}

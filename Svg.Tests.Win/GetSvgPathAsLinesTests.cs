using System.Linq;
using NUnit.Framework;
using Shouldly;
using Svg.Interfaces;
using Svg.Pathing;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class GetSvgPathAsLinesTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [Test]
        public void GivenSvgPathSegmentList_ReturnsAllVisibleLines()
        {
            //Arrange
            var svgPathSegmentList = new SvgPathSegmentList
            {
                new SvgMoveToSegment(PointF.Create(50, 250)),
                new SvgLineSegment(PointF.Create(50, 250), PointF.Create(50, 50)),
                new SvgLineSegment(PointF.Create(50, 50), PointF.Create(250, 50)),
                new SvgClosePathSegment(),
                new SvgMoveToSegment(PointF.Create(8, 650))
            };
            
            var expected = new []
            {
                (PointF.Create(50,250), PointF.Create(50,50)),
                (PointF.Create(50,50), PointF.Create(250,50)),
                (PointF.Create(250,50), PointF.Create(50,250)),
            }.ToList();

            //Act
            var actual = svgPathSegmentList.GetLines();

            //Assert
            actual.ShouldBe(expected);

        }
    }
}
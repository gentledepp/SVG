using NUnit.Framework;
using Shouldly;
using Svg.Interfaces;

namespace Svg.Tests.Win
{
    [TestFixture]
    public class WebRequestSvcTests
    {
        [SetUp]
        public void SetUp()
        {
            SvgPlatform.Init();
        }

        [TestCase(@"file://C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg", true,
            @"C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg")]
        [TestCase(@"file:///C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg", true,
            @"C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg")]
        [TestCase(@"file:////C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg", true,
            @"C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg")]
        [TestCase(@"C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg", true,
            @"C:\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg")]
        [TestCase(@"images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg", false,
            @"\images\67983e10-8e8e-4456-83f1-b4f71f72ec9d.jpg")]
        public void CanGetFullPath(string path, bool isRooted, string expected)
        {
            // Arrange
            var wr = new WebRequestSvc();
            var fs = SvgEngine.Resolve<IFileSystem>();
            var defaultStoragePath = fs.GetDefaultStoragePath();

            // Act
            var result = wr.EnsureFullPath(path);

            // Assert
            if (isRooted)
                result.ShouldBe(expected);
            else
                result.ShouldBe(defaultStoragePath + expected);
        }
    }
}

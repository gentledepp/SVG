using NUnit.Framework;
using Svg;
using Svg.Editor.Tests;

[assembly:SvgService(typeof(IFileLoader), typeof(FileLoader))]
namespace Svg.Editor.Tests
{
    public class FileLoader : IFileLoader
    {
        public SvgDocument Load(string fileName)
        {
            var path = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", fileName);
            using (var str = System.IO.File.OpenRead(path))
            {
                return SvgDocument.Open<SvgDocument>(str);
            }
        }
    }
}

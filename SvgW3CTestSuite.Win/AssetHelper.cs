using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SvgW3CTestSuite.Assets
{
    public static class AssetHelper
    {
        public static IEnumerable<string> GetAllSvgFiles()
        {
            var assembly = typeof(AssetHelper).GetTypeInfo().Assembly;
            return
                assembly.GetManifestResourceNames()
                    .Where(r => r.StartsWith("SvgW3CTestSuite.Assets.svg.") && r.EndsWith(".svg"));
        }

        public static IEnumerable<string> GetAllPngFiles()
        {
            var assembly = typeof(AssetHelper).GetTypeInfo().Assembly;
            return
                assembly.GetManifestResourceNames()
                    .Where(r => r.StartsWith("SvgW3CTestSuite.Assets.png.") && r.EndsWith(".png"));
        }

        public static Stream GetResource(string name)
        {
            var assembly = typeof(AssetHelper).GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(name);
        }

        public static string GetPngForSvg(string svgFile)
        {
            var pngName = svgFile.Replace(".svg.", ".png.").Replace(".svg", ".png");
            return pngName;
            //var assembly = typeof(AssetHelper).GetTypeInfo().Assembly;
            //return assembly.GetManifestResourceStream(pngName);
        }
    }
}

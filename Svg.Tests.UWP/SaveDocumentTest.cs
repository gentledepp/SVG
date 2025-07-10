using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Svg.Tests.UWP
{
    public class SaveDocumentTest
    {
        public SaveDocumentTest()
        {
            SvgPlatform.Init();
        }

        [Fact]
        public void CanSaveEmptyDocument()
        {
            // Arrange
            var doc = new SvgDocument();
            SvgDocument doc2 = null;
            //var expectedSvg = "﻿<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?><svg width=\"100%\" height=\"100%\" preserveAspectRatio=\"xMidYMid\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" version=\"1.1\" />";
            var expectedSvg = "﻿<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?><svg width=\"100%\" height=\"100%\" preserveAspectRatio=\"xMidYMid\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" />";
            var svg = string.Empty;

            // Act
            using (var ms = new MemoryStream())
            {
                doc.Write(ms);
                ms.Seek(0, SeekOrigin.Begin);
                doc2 = SvgDocument.Open<SvgDocument>(ms, new Dictionary<string, string>());
                svg = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Assert
            Assert.NotNull(doc2);
            Assert.Equal(expectedSvg, svg);
        }
    }
}

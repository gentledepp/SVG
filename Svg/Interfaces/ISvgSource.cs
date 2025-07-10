using System;
using System.IO;

namespace Svg.Interfaces
{
    public interface ISvgSource
    {
        Stream GetStream();
        Stream GetFileRelativeTo(Uri relativePath);
    }
}

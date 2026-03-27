
using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace Svg.DeepZoom
{
    public interface ITileGenerator
    {
        Task GenerateTilesAsync(string sourceImagePath, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff");

        Task GenerateTilesAsync(Stream sourceImageStream, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff", int maxParallelTasks = -1, SKEncodedImageFormat? imageFormat = null, int quality = -1);

        Task GenerateTilesAsync(SvgDocument document, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff", int maxParallelTasks = -1, SKEncodedImageFormat? imageFormat = null, int quality = -1, int overdrawMargin = 0);

        Task GenerateTilesAsync(SvgDocument document, int targetWidth, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff", int maxParallelTasks = -1, SKEncodedImageFormat? imageFormat = null, int quality = -1, int overdrawMargin = 0);
    }
}

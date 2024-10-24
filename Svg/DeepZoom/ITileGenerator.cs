
using System;
using System.IO;
using System.Threading.Tasks;

namespace Svg.DeepZoom
{
    public interface ITileGenerator
    {
        Task GenerateTilesAsync(string sourceImagePath, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff");

        Task GenerateTilesAsync(Stream sourceImageStream, Func<string, string, Task<Stream>> tileOutputStreamProvider, IProgress<int> progress = null, string backgroundColor = "#ffffff", int maxParallelTasks = -1);


    }
}
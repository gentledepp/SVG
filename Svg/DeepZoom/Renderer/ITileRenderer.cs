using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Svg.DeepZoom
{
    public interface ITileRenderer : IDisposable
    {
        void SetDimensions(int width, int height);

        void RenderBitmap(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1);

        SKBitmap RenderBitmap(Func<string, string, Stream> tileProvider, float offsetX, float offsetY, float zoomFactor = 1);

        Task RenderBitmapAsync(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1);

        Task<SKBitmap> RenderBitmapAsync(Func<string, string, Task<Stream>> tileProvider, float offsetX,
            float offsetY, float zoomFactor = 1);

    }
}
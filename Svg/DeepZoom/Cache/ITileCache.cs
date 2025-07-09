using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace Svg.DeepZoom{
    public interface ITileCache : IDisposable
    {
        TileCacheItem GetOrCreate(string key, Func<SKBitmap> itemProvider);
        Task<TileCacheItem> GetOrCreateAsync(string key, Func<Task<SKBitmap>> itemProvider);
        bool TryGetValue(string key, out TileCacheItem item);
        void Remove(string key);

    }
}
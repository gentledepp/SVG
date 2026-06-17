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

        /// <summary>
        /// Disposes the bitmaps of items that have left the cache (eviction, expiry, removal).
        /// Must be called at a point where no tile is being drawn (e.g. the start of a render),
        /// so an in-flight draw can never reference a just-disposed bitmap.
        /// </summary>
        void DrainPendingDisposals();
    }
}
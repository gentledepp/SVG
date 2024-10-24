using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace Svg.DeepZoom
{
    public class TileCache : ITileCache
    {

        private ConcurrentDictionary<string, TileCacheItem> _cache = new();
        private readonly Timer _cleanupTimer;
        private readonly TileCacheOptions _options;
        private readonly int _maximalTiles;

        public TileCache(TileCacheOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _cleanupTimer = new Timer(CleanUp, null, _options.CleanupInterval, _options.CleanupInterval);
            _maximalTiles = _options.MaximalTiles;
        }

        public TileCacheItem GetOrCreate(string key, Func<SKBitmap> itemProvider)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (itemProvider == null) throw new ArgumentNullException(nameof(itemProvider));

            if (TryGetValue(key, out var tileItem))
                return tileItem;

            var newItem = new TileCacheItem(itemProvider(), _options.CleanupInterval);
           
            if (_cache.Count >= _maximalTiles)
            {
                return newItem;
            }
            _cache.TryAdd(key, newItem);

            return newItem;
        }

        public async Task<TileCacheItem> GetOrCreateAsync(string key, Func<Task<SKBitmap>> itemProvider)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (itemProvider == null) throw new ArgumentNullException(nameof(itemProvider));

            if (TryGetValue(key, out var tileItem))
                return tileItem;

            var newItem = new TileCacheItem(await itemProvider(), _options.CleanupInterval);
            if (_cache.Count >= _maximalTiles)
            {
                return newItem;
            }
            _cache.TryAdd(key, newItem);

            return newItem;
        }

        public bool TryGetValue(string key, out TileCacheItem item)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            // if caching is disabled
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (!cacheItem.IsExpired)
                {
                    item = cacheItem;
                    return true;
                }

                _cache.TryRemove(key, out _);
            }

            item = default;
            return false;
        }

        public void Remove(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _cache.TryRemove(key, out _);
        }

        private void CleanUp(object state)
        {
            foreach (var key in _cache.Keys)
            {
                if (_cache.TryGetValue(key, out var cacheItem) && cacheItem.IsExpired)
                {
                    cacheItem.Dispose();
                }
            }
        }

        public void Dispose()
        {
            foreach (var tileCacheItem in _cache)
            {
                tileCacheItem.Value?.Dispose();
            }
            _cleanupTimer?.Dispose();
        }
    }
}
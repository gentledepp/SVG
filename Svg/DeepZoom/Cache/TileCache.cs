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
        private readonly object _evictionLock = new();
        private long _tickCounter = 0;

        // Items that have left the cache but whose SKBitmap is not yet disposed. Disposal is
        // deferred to DrainPendingDisposals (called at render start) so the background cleanup
        // timer can never free a bitmap that a concurrent draw still references.
        private readonly ConcurrentQueue<TileCacheItem> _pendingDispose = new();

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

            var newItem = new TileCacheItem(itemProvider(), _options.CleanupInterval, Interlocked.Increment(ref _tickCounter));
            AddWithEviction(key, newItem);
            return newItem;
        }

        public async Task<TileCacheItem> GetOrCreateAsync(string key, Func<Task<SKBitmap>> itemProvider)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (itemProvider == null) throw new ArgumentNullException(nameof(itemProvider));

            if (TryGetValue(key, out var tileItem))
                return tileItem;

            var newItem = new TileCacheItem(await itemProvider(), _options.CleanupInterval, Interlocked.Increment(ref _tickCounter));
            AddWithEviction(key, newItem);
            return newItem;
        }

        public bool TryGetValue(string key, out TileCacheItem item)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (!cacheItem.IsExpired)
                {
                    cacheItem.Touch(Interlocked.Increment(ref _tickCounter));
                    item = cacheItem;
                    return true;
                }

                if (_cache.TryRemove(key, out var expired))
                    _pendingDispose.Enqueue(expired);
            }

            item = default;
            return false;
        }

        private void AddWithEviction(string key, TileCacheItem newItem)
        {
            lock (_evictionLock)
            {
                if (_cache.Count >= _maximalTiles)
                    EvictLru();
                _cache.TryAdd(key, newItem);
            }
        }

        private void EvictLru()
        {
            string lruKey = null;
            long oldest = long.MaxValue;
            foreach (var kvp in _cache)
            {
                if (kvp.Value.LastAccessTick < oldest)
                {
                    oldest = kvp.Value.LastAccessTick;
                    lruKey = kvp.Key;
                }
            }

            if (lruKey != null && _cache.TryRemove(lruKey, out var evicted))
                _pendingDispose.Enqueue(evicted);
        }

        public void Remove(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_cache.TryRemove(key, out var removed))
                _pendingDispose.Enqueue(removed);
        }

        private void CleanUp(object state)
        {
            foreach (var key in _cache.Keys)
            {
                if (_cache.TryGetValue(key, out var cacheItem) && cacheItem.IsExpired
                    && _cache.TryRemove(key, out var expired))
                    _pendingDispose.Enqueue(expired);
            }
        }

        public void DrainPendingDisposals()
        {
            while (_pendingDispose.TryDequeue(out var item))
                item?.Dispose();
        }

        public void Dispose()
        {
            DrainPendingDisposals();
            foreach (var tileCacheItem in _cache)
            {
                tileCacheItem.Value?.Dispose();
            }
            _cleanupTimer?.Dispose();
        }
    }
}
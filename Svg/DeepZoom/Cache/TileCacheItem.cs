using System;
using SkiaSharp;

namespace Svg.DeepZoom
{
    public class TileCacheItem : IDisposable
    {
        public SKBitmap Tile { get; }

        public DateTime ExpirationTime { get; }

        public bool IsExpired => DateTime.Now >= ExpirationTime;

        public long LastAccessTick { get; private set; }

        public TileCacheItem(SKBitmap tile, TimeSpan expirationTime, long initialTick)
        {
            Tile = tile;
            ExpirationTime = DateTime.Now.Add(expirationTime);
            LastAccessTick = initialTick;
        }

        internal void Touch(long tick) => LastAccessTick = tick;

        public void Dispose()
        {
            Tile?.Dispose();
        }
    }
}
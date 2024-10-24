using System;
using SkiaSharp;

namespace Svg.DeepZoom
{
    public class TileCacheItem : IDisposable
    {
        public SKBitmap Tile { get; }

        public DateTime ExpirationTime { get; }

        public bool IsExpired => DateTime.Now >= ExpirationTime;

        public TileCacheItem(SKBitmap tile, TimeSpan expirationTime)
        {
            Tile = tile;
            ExpirationTime = DateTime.Now.Add(expirationTime);
        }

        public void Dispose()
        {
            Tile?.Dispose();
        }
    }
}
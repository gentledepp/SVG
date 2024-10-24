using System;

namespace Svg.DeepZoom
{

    public class TileCacheOptions
    {
        public TileCacheOptions()
        {
        }

        public TileCacheOptions(TimeSpan cleanupInterval)
        {
            CleanupInterval = cleanupInterval;
        }

        public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(1);

        public int MaximalTiles { get; set; } = 1000;

    }
}
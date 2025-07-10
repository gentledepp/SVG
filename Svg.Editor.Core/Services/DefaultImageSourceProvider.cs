using System;
using System.Collections.Concurrent;
using System.Linq;
using Svg.Interfaces;

namespace Svg.Editor.Services
{
    public class DefaultImageSourceProvider : IImageSourceProvider
    {
        private static readonly Lazy<IEmbeddedResourceRegistry> Registry = new Lazy<IEmbeddedResourceRegistry>(() => SvgEngine.Resolve<IEmbeddedResourceRegistry>());
        private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

        public  virtual string GetImage(string image, SizeF dimension = null)
        {
            var options = new SaveAsPngOptions() {ImageDimension = dimension};
            return GetImage(image, options);
        }

        public virtual string GetImage(string image, SaveAsPngOptions options)
        {
            if (image == null)
                return GetDefaultImage();

            var resource = Registry.Value.EmbeddedResouceNames.FirstOrDefault(r => r.EndsWith(image));
            // if this is a local resource file
            if (resource != null && resource.EndsWith(".svg"))
            {
                var cache = SvgEngine.TryResolve<ISvgCachingService>();
                if (cache != null)
                {
                    if (!options.Force && Cache.ContainsKey(resource))
                        return Cache[resource];

                    var cached = cache.GetCachedPng(resource, options);
                    Cache.AddOrUpdate(resource, cached, (o, n) => n);
                    return cached;
                }
            }

            return image;
        }

        protected virtual string GetDefaultImage()
        {
            return null;
        }
    }
}
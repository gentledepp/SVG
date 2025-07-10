using System;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Editor.Services
{
    public class EmbeddedResourceSvgSourceFactory : ISvgSourceFactory
    {
        private readonly IEmbeddedResourceRegistry _registry;

        public EmbeddedResourceSvgSourceFactory(IEmbeddedResourceRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            _registry = registry;
        }

        public ISvgSource Create(string path)
        {
            var type = _registry.EmbeddedResourceTypes.FirstOrDefault(t => path.StartsWith(t.Namespace, StringComparison.OrdinalIgnoreCase));
            if(type == null)
                throw new InvalidOperationException($"No type was found whose namespace is the start of the path '{path}' ('{string.Join(",", _registry.EmbeddedResourceTypes.Select(t => t.FullName))}')");

            return new EmbeddedResourceSource(path, type.GetTypeInfo().Assembly);
        }
    }
}

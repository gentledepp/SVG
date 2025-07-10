using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class EmbeddedResourceSource : ISvgSource
    {
        private readonly string _name;
        private readonly Assembly _assembly;

        public EmbeddedResourceSource(string name, Assembly assembly)
        {
            _name = name;
            _assembly = assembly;
        }

        public Stream GetStream()
        {
            return _assembly.GetManifestResourceStream(_name);
        }

        public Stream GetFileRelativeTo(Uri relativePath)
        {
            var str = relativePath.OriginalString;
            if (str.StartsWith("#"))
                return null;

            var path = GetParent(Path.GetFileNameWithoutExtension(_name));

            // remove hash if there is any
            var hash = relativePath.OriginalString.Substring(relativePath.OriginalString.LastIndexOf('#'));

            if (!string.IsNullOrWhiteSpace(hash))
            {
                str = str.Contains(hash)
                    ? str.Substring(0, str.Length - hash.Length)
                    : str;
            }

            while (str.StartsWith("../"))
            {
                path = GetParent(path);
                str = str.Substring(3);
            }

            var fullPath = path == null ? str : $"{path}.{str.Replace('/','.')}";
            try
            {
                return _assembly.GetManifestResourceStream(fullPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetParent(string resourceString)
        {
            var arr = resourceString.Split('.');
            return string.Join(".", arr.Take(arr.Length - 1));
        }

        public static ISvgSource Create(string name, Assembly assembly)
        {
            return new EmbeddedResourceSource(name, assembly);
        }
    }
}
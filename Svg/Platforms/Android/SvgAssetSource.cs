using System;
using System.IO;
using Android.Content.Res;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SvgAssetSource : ISvgSource
    {
        private readonly string _filePath;
        private readonly AssetManager _assetManager;

        public SvgAssetSource(string filePath, AssetManager assetManager)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            if (assetManager == null) throw new ArgumentNullException(nameof(assetManager));
            _filePath = filePath;
            _assetManager = assetManager;
        }

        public Stream GetStream()
        {
            return _assetManager.Open(_filePath);
        }

        public Stream GetFileRelativeTo(Uri relativePath)
        {
            var str = relativePath.OriginalString;
            if (str.StartsWith("#"))
                return null;

            var path = Path.GetDirectoryName(_filePath);

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
                path = Path.GetDirectoryName(path);
                str = str.Substring(3);
            }

            var fullPath = path == null ? str : Path.Combine(path, str);
            try
            {
                return _assetManager.Open(fullPath);
            }
            catch (Java.IO.FileNotFoundException)
            {
                return null;
            }
        }
    }
}
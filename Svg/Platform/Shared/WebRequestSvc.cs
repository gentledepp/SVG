using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Svg.Interfaces;

namespace Svg
{
    public class WebRequestSvc : IWebRequest
    {
        public Stream GetResponse(Uri uri)
        {
            if (string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase))
            {
                var filePath = EnsureFullPath(uri.OriginalString);
                return File.OpenRead(filePath);
            }

            return Task.Run(async () =>
            {
                var httpRequest = WebRequest.Create(uri);
                return await httpRequest.GetRequestStreamAsync();
            }).Result;
        }

        internal string EnsureFullPath(string path)
        {
            var fs = SvgEngine.Resolve<IFileSystem>();
            // removing "file://" from the beginning
            var filePath = path.StartsWith("file://") ? path.Substring(7) : path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                filePath = filePath.Trim('/');

            // Path.Combine and Path.IsPathRooted do not work on Android so we implemented them
            return IsPathRooted(filePath) ? filePath : PathCombine(fs.GetDefaultStoragePath(), filePath);
        }

        public string PathCombine(string first, string second)
        {
            first = first?.Trim() ?? "";
            second = second?.Trim() ?? "";

            return first.TrimEnd('/').TrimEnd('\\') + Path.DirectorySeparatorChar + second.TrimStart('\\').TrimStart('/');
        }

        public bool IsPathRooted(string path)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.IsPathRooted(path)
                : path.StartsWith(new string(Path.DirectorySeparatorChar, 1));
        }
    }

}
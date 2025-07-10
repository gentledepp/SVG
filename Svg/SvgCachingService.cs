using System;
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public class SvgCachingService : ISvgCachingService
    {
        private readonly Func<string, ISvgSource> _sourceProvider;

        private Lazy<IFileSystem> _fs = new Lazy<IFileSystem>(() => SvgEngine.Resolve<IFileSystem>());

        public SvgCachingService()
        {
            _sourceProvider = path => SvgEngine.Resolve<ISvgSourceFactory>().Create(path);
        }

        public string GetCachedPng(string svgFilePath, SaveAsPngOptions options)
        {
            if (svgFilePath == null) throw new ArgumentNullException(nameof(svgFilePath));
            if (options == null) throw new ArgumentNullException(nameof(options));

            // load svg from FS
            var document = SvgDocument.Open<SvgDocument>(_sourceProvider(svgFilePath));

            // apply changes to svg
            options.PreprocessAction?.Invoke(document);

            var dimension = GetDimension(options) ?? document.CalculateDocumentBounds().Size;
            options.ImageDimension = dimension;

            // save svg as png
            using (var bmp = SvgEngine.Factory.CreateBitmap((int)dimension.Width, (int)dimension.Height))
            {
                Action<SvgDocument,Bitmap,Color> draw = options.DrawingAction ?? ((s,b,c) => s.Draw(b, c));

                draw(document, bmp, options.BackgroundColor);
                var fs = _fs.Value;
                var path = GetCachedPngPath(svgFilePath, options);
                if (fs.FileExists(path))
                {
                    // if re-creation is forced
                    if (options.Force)
                        fs.DeleteFile(path);
                    else
                        return path;
                }

                using (var stream = fs.OpenWrite(path))
                {
                    bmp.SavePng(stream);
                }

                return path;
            }
        }

        public void Clear()
        {
            var fs = _fs.Value;
            var dirPath = fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache");
            if (fs.FolderExists(dirPath))
                fs.DeleteFolder(dirPath);
        }
        
        private string GetCachedPngPath(string svgFilePath, SaveAsPngOptions options)
        {
            var fs = _fs.Value;
            var dim = GetDimension(options);
            fs.EnsureDirectoryExists(fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache"));
            var name = Path.GetFileNameWithoutExtension(svgFilePath).ToLower().Replace(".", "_");
            var filename = options.NamingConvention(svgFilePath, options) ?? $"{name}_{(int)dim.Width}px_{(int)dim.Height}px.png";

            return fs.PathCombine(fs.GetDefaultStoragePath(), "SvgCache", filename);
        }

        private SizeF GetDimension(SaveAsPngOptions options)
        {
            return options.ImageDimension;
        }
    }
}

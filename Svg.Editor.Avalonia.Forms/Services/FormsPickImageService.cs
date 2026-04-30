using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Avalon.Forms.Services
{
    public class FormsPickImageService : IPickImageService
    {
        public static Window MainWindow { get; set; }
        private Window _mainWindow;

        public FormsPickImageService()
        {
            _mainWindow= MainWindow;
        }

        public async Task<string> PickZipFilePathAsync()
        {
            var options = new FilePickerOpenOptions
            {
                Title = "Select Tile Zip",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Zip Files")
                    {
                        Patterns = new[] { "*.zip" },
                        MimeTypes = new[] { "application/zip" }
                    }
                }
            };

            var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(options);
            if (result == null || result.Count == 0)
                return null;

            return result[0].Path.LocalPath;
        }

        public async Task<string> PickImageOrZipPathAsync(int maxPixelDimension)
        {
            var options = new FilePickerOpenOptions
            {
                Title = "Select Image or Tile Zip",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Images & Zips")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.svg", "*.zip" },
                        MimeTypes = new[] { "image/*", "application/zip" }
                    }
                }
            };

            var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(options);
            if (result == null || result.Count == 0)
                return null;

            var selectedFile = result[0];
            var name = selectedFile.Name;

            if (name.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
                return selectedFile.Path.LocalPath;

            try
            {
                var fs = SvgEngine.Resolve<IFileSystem>();
                var path = name.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase) ? "background.svg" : "background.png";
                var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);

                if (fs.FileExists(fullPath))
                    fs.DeleteFile(fullPath);

                using (var inStream = await selectedFile.OpenReadAsync())
                using (var outStream = fs.OpenWrite(fullPath))
                {
                    await inStream.CopyToAsync(outStream);
                }

                return path;
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error picking file: {ex.Message}");
                return null;
            }
        }

        public async Task<string> PickImagePathAsync(int maxPixelDimension)
        {
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = "Select Image",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                     new FilePickerFileType("Images")
                     {
                         Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.svg" },
                         MimeTypes = new[] { "image/*" }
                     }
                 }
            };

            var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);

            if (result == null || result.Count == 0)
                return null;

            var selectedFile = result[0];

            try
            {
                var fs = SvgEngine.Resolve<IFileSystem>();
                var path = selectedFile.Name.EndsWith(".svg")? "background.svg" : "background.png";
                var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);

                if (fs.FileExists(fullPath))
                    fs.DeleteFile(fullPath);

                using (var inStream = await selectedFile.OpenReadAsync())
                using (var outStream = fs.OpenWrite(fullPath))
                {
                    await inStream.CopyToAsync(outStream);
                }

                return path;
            }
            catch (System.Exception ex)
            {
                // Handle or log error
                System.Diagnostics.Debug.WriteLine($"Error picking image: {ex.Message}");
                return null;
            }
        }
    }
}

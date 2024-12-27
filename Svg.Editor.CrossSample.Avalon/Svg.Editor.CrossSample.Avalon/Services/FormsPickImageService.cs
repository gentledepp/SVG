// using System.Threading.Tasks;
// using Avalonia.Controls;
// using Avalonia.Platform.Storage;
// using Svg.Editor.CrossSample.Avalon;
// using Svg.Editor.Interfaces;
// using Svg.Editor.Sample.Avalon.Services;
// using Svg.Editor.Tools;
// using Svg.Interfaces;
//
// namespace Svg.Editor.Sample.Avalon.Services
// {
//     public class FormsPickImageService : IPickImageService
//     {
//         private Window _mainWindow;
//
//         public FormsPickImageService()
//         {
//             _mainWindow= StrokeStyleOptionsInputService.GetWindow();
//         }
//
//         public async Task<string> PickImagePathAsync(int maxPixelDimension)
//         {
//             var filePickerOptions = new FilePickerOpenOptions
//             {
//                 Title = "Select Image",
//                 AllowMultiple = false,
//                 FileTypeFilter = new[]
//                 {
//                     new FilePickerFileType("Images")
//                     {
//                         Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" },
//                         MimeTypes = new[] { "image/*" }
//                     }
//                 }
//             };
//
//             var result = await _mainWindow.StorageProvider.OpenFilePickerAsync(filePickerOptions);
//
//             if (result == null || result.Count == 0)
//                 return null;
//
//             var selectedFile = result[0];
//
//             try
//             {
//                 var fs = SvgEngine.Resolve<IFileSystem>();
//                 var path = "background.png";
//                 var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);
//
//                 if (fs.FileExists(fullPath))
//                     fs.DeleteFile(fullPath);
//
//                 using (var inStream = await selectedFile.OpenReadAsync())
//                 using (var outStream = fs.OpenWrite(fullPath))
//                 {
//                     await inStream.CopyToAsync(outStream);
//                 }
//
//                 return path;
//             }
//             catch (System.Exception ex)
//             {
//                 // Handle or log error
//                 System.Diagnostics.Debug.WriteLine($"Error picking image: {ex.Message}");
//                 return null;
//             }
//         }
//     }
// }

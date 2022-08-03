using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using IFileSystem = Svg.Interfaces.IFileSystem;

namespace Svg.Editor.Samples.Forms
{
    public class FormsPickImageService : IPickImageService
    {
        public async Task<string> PickImagePathAsync(int maxPixelDimension)
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();

            var fs = SvgEngine.Resolve<IFileSystem>();
            var path = "background.png";

            var fullPath = fs.PathCombine(fs.GetDefaultStoragePath(), path);

            if (fs.FileExists(fullPath))
                fs.DeleteFile(fullPath);

            using Stream sourceStream = await photo.OpenReadAsync();
            using var outStream = fs.OpenWrite(fullPath);

            await sourceStream.CopyToAsync(outStream);

            return path;
        }
    }
}

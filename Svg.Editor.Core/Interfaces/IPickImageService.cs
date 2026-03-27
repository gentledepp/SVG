using System.Threading.Tasks;

namespace Svg.Editor.Interfaces
{
    public interface IPickImageService
    {
        Task<string> PickImagePathAsync(int maxPixelDimension);

        /// <summary>Returns the absolute path of a user-selected zip file, or null if cancelled.</summary>
        Task<string> PickZipFilePathAsync();

        /// <summary>
        /// Opens a single file picker that accepts images (png/jpg/svg) and zip files.
        /// Returns the path to the selected file, or null if cancelled.
        /// The returned path is absolute for zip files and relative-to-storage for images.
        /// </summary>
        Task<string> PickImageOrZipPathAsync(int maxPixelDimension);
    }
}
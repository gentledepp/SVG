using System.Threading.Tasks;

namespace Svg.Editor.Interfaces
{
    public interface IPickImageService
    {
        Task<string> PickImagePathAsync(int maxPixelDimension);
    }
}
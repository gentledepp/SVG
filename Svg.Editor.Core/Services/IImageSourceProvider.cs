using Svg.Interfaces;

namespace Svg.Editor.Services
{
    public interface IImageSourceProvider
    {
        string GetImage(string image, SizeF dimension = null);
        string GetImage(string image, SaveAsPngOptions options);
    }
}
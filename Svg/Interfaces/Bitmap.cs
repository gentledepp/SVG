
using System.IO;
using Svg.Interfaces;

namespace Svg
{
    public abstract class Bitmap : Image
    {
        public static Bitmap Create(int width, int height)
        {
            return SvgEngine.Factory.CreateBitmap(width, height);
        }

        public static Bitmap Create(Image image)
        {
            return SvgEngine.Factory.CreateBitmap(image);
        }

        public abstract BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat);
        public abstract void SavePng(Stream stream, int quality = 76);
        public abstract void Dispose();

        public abstract int Width { get; protected set; }
        public abstract int Height { get; protected set; }
        public abstract void SaveJpeg(Stream stream, int quality = 76);
    }
}
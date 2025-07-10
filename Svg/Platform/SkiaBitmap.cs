
using System;
using System.IO;
using SkiaSharp;
using RectangleF = Svg.Interfaces.RectangleF;

namespace Svg.Platform
{

    public class SkiaBitmap : Bitmap
    {
        protected readonly SKBitmap _image;

        public SkiaBitmap(int width, int height) : this(new SKBitmap(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul))
        {
        }

        public SkiaBitmap(Image inputImage) : this(new SKBitmap(((SkiaBitmap) inputImage)._image.Info))
        {
        }

        public SkiaBitmap(SKBitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            _image = bitmap;
            Width = _image.Width;
            Height = _image.Height;
        }

        protected SkiaBitmap()
        {

        }

        public SKBitmap Image
        {
            get { return _image; }
        }

        public override void Dispose()
        {
            _image.Dispose();
        }

        public override BitmapData LockBits(RectangleF rectangle, ImageLockMode lockmode, PixelFormat pixelFormat)
        {
            throw new NotImplementedException();
        }

        public override void SavePng(Stream stream, int quality = 76)
        {
            using (var img = SKImage.FromBitmap(_image))
            {
                var data = img.Encode(SKEncodedImageFormat.Png, quality);
                data.SaveTo(stream);
            }
        }

        public override void SaveJpeg(Stream stream, int quality = 76)
        {
            using (var img = SKImage.FromBitmap(_image))
            {
                var data = img.Encode(SKEncodedImageFormat.Jpeg, quality);
                data.SaveTo(stream);
            }
        }

        public override int Width { get; protected set; }
        public override int Height { get; protected set; }
    }
}

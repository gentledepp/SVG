using System;
using SkiaSharp;

namespace Svg.Platform
{
    public class SkiaTextureBrush : SkiaBrushBase, TextureBrush, IDisposable
    {
        private SkiaBitmap _image;
        private SKShader _shader;

        public SkiaTextureBrush(SkiaBitmap image)
        {
            _image = image;
        }

        public override void Dispose()
        {
            base.Dispose();
            _image?.Dispose();
            _image = null;
            _shader?.Dispose();
            _shader = null;
        }
        // TODO LX what about Transform?
        public Matrix Transform { get; set; }

        protected override SKPaint CreatePaint()
        {
            var paint = new SKPaint();
            if (_shader != null)
            {
                _shader.Dispose();
                _shader = null;
            }

            _shader = SKShader.CreateBitmap(_image.Image, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
            
            paint.Shader = _shader;
            paint.IsAntialias = true;
            return paint;
        }
    }
}

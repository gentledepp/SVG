using System;
using SkiaSharp;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaColor : Color
    {
        private SKColor _inner;

        public SkiaColor()
        {
            _inner = new SKColor();
        }

        public SkiaColor(byte r, byte g, byte b)
        {
            _inner = new SKColor(r, g, b, 255);
        }

        public SkiaColor(byte a, byte r, byte g, byte b)
        {
            _inner = new SKColor(r, g, b, a);
        }


        public SkiaColor(byte a, Color baseColor)
        {
            var b = ((SkiaColor)baseColor)._inner;
            _inner = new SKColor(b.Red, b.Green, b.Blue, a);
        }

        public SkiaColor(SKColor inner)
        {
            _inner = inner;
        }

        public override string Name => "none";
        public override bool IsKnownColor => false;
        public override bool IsSystemColor => false;
        public override bool IsNamedColor => false;
        public override bool IsEmpty => false;
        public override byte A => _inner.Alpha;
        public override byte R => _inner.Red;
        public override byte G => _inner.Green;
        public override byte B => _inner.Blue;
        public override float GetBrightness()
        {
            throw new NotImplementedException();
        }
        public override float GetSaturation()
        {
            throw new NotImplementedException();
        }
        public override float GetHue()
        {
            throw new NotImplementedException();
        }
        public override int ToArgb()
        {
            return B + G * 0x100 + R * 0x10000 + A * 0x1000000;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return ToArgb();
        }

        public static implicit operator SkiaColor(SKColor other)
        {
            return new SkiaColor(other);
        }

        public static implicit operator SKColor(SkiaColor other)
        {
            return other._inner;
        }
    }
}

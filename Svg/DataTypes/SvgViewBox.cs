using System;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// It is often desirable to specify that a given set of graphics stretch to fit a particular container element. The viewBox attribute provides this capability.
    /// </summary>
    //[TypeConverter(typeof(SvgViewBoxConverter))]
    public struct SvgViewBox
    {
        public static readonly SvgViewBox Empty = new SvgViewBox();

        /// <summary>
        /// Gets or sets the position where the viewport starts horizontally.
        /// </summary>
        public float MinX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the position where the viewport starts vertically.
        /// </summary>
        public float MinY
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width of the viewport.
        /// </summary>
        public float Width
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the height of the viewport.
        /// </summary>
        public float Height
        {
            get;
            set;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SvgViewBox"/> to <see cref="RectangleF"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator RectangleF(SvgViewBox value)
        {
            return RectangleF.Create(value.MinX, value.MinY, value.Width, value.Height);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="RectangleF"/> to <see cref="SvgViewBox"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator SvgViewBox(RectangleF value)
        {
            value = value ?? RectangleF.Empty;
            return new SvgViewBox(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgViewBox"/> struct.
        /// </summary>
        /// <param name="minX">The min X.</param>
        /// <param name="minY">The min Y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public SvgViewBox(float minX, float minY, float width, float height) : this()
        {
            MinX = minX;
            MinY = minY;
            Width = width;
            Height = height;
        }

        #region Equals and GetHashCode implementation
        public override bool Equals(object obj)
        {
            return (obj is SvgViewBox) && Equals((SvgViewBox) obj);
        }

        public bool Equals(SvgViewBox other)
        {
            return Math.Abs(MinX - other.MinX) < 0.01
                && Math.Abs(MinY - other.MinY) < 0.01
                && Math.Abs(Width - other.Width) < 0.01
                && Math.Abs(Height - other.Height) < 0.01;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            unchecked
            {
                hashCode += 1000000007 * MinX.GetHashCode();
                hashCode += 1000000009 * MinY.GetHashCode();
                hashCode += 1000000021 * Width.GetHashCode();
                hashCode += 1000000033 * Height.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(SvgViewBox lhs, SvgViewBox rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SvgViewBox lhs, SvgViewBox rhs)
        {
            return !(lhs == rhs);
        }
        #endregion

        public SvgViewBox DeepCopy()
        {
            return this == SvgViewBox.Empty ? SvgViewBox.Empty : new SvgViewBox(MinX, MinY, Width, Height);
        }

        public void AddViewBoxTransform(SvgAspectRatio aspectRatio, ISvgRenderer renderer)
        {
            AddViewBoxTransform(aspectRatio, renderer, RectangleF.Create(0, 0, Width, Height));
        }

        public void AddViewBoxTransform(SvgAspectRatio aspectRatio, ISvgRenderer renderer, SvgFragment frag)
        {
            var x = (frag == null ? 0 : frag.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, frag));
            var y = (frag == null ? 0 : frag.Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, frag));

            var width = (frag == null ? Width : frag.Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, frag));
            var height = (frag == null ? Height : frag.Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, frag));

            AddViewBoxTransform(aspectRatio, renderer, RectangleF.Create(x, y, width, height));
        }

        /// <summary>
        /// Applies the viewbox given the specified bounds
        /// </summary>
        /// <remarks>
        /// a good explanation of SVG viewboxes can be found here:
        /// http://tutorials.jenkov.com/svg/svg-viewport-view-box.html
        /// </remarks>
        /// <param name="aspectRatio"></param>
        /// <param name="renderer"></param>
        /// <param name="bounds"></param>
        public void AddViewBoxTransform(SvgAspectRatio aspectRatio, ISvgRenderer renderer, RectangleF bounds)
        {
            var x = bounds.X;
            var y = bounds.Y;
            var width = bounds.Width;
            var height = bounds.Height;

            if (Equals(Empty))
            {
                renderer.TranslateTransform(x, y);
                return;
            }

            float scaleX;
            float scaleY;
            float minX;
            float minY;

            CalculateTransform(aspectRatio, width, height, out scaleX, out scaleY, out minX, out minY);

            renderer.SetClip(new Region(RectangleF.Create(x, y, width, height)), CombineMode.Intersect);
            renderer.TranslateTransform(x, y, MatrixOrder.Prepend);
            renderer.TranslateTransform(minX, minY, MatrixOrder.Prepend);
            renderer.ScaleTransform(scaleX, scaleY, MatrixOrder.Prepend);
        }

        public void CalculateTransform(SvgAspectRatio aspectRatio, float width, float height, out float scaleX, out float scaleY, out float minX, out float minY)
        {
            scaleX = width / Width;
            scaleY = height / Height;
            minX = -MinX;
            minY = -MinY;

            if (aspectRatio == null) aspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid, false);
            if (aspectRatio.Align != SvgPreserveAspectRatio.none)
            {
                if (aspectRatio.Slice)
                {
                    scaleX = Math.Max(scaleX, scaleY);
                    scaleY = Math.Max(scaleX, scaleY);
                }
                else
                {
                    scaleX = Math.Min(scaleX, scaleY);
                    scaleY = Math.Min(scaleX, scaleY);
                }
                var viewMidX = Width / 2 * scaleX;
                var viewMidY = Height / 2 * scaleY;
                var midX = width / 2;
                var midY = height / 2;

                switch (aspectRatio.Align)
                {
                    case SvgPreserveAspectRatio.xMinYMin:
                        break;
                    case SvgPreserveAspectRatio.xMidYMin:
                        minX += midX - viewMidX;
                        break;
                    case SvgPreserveAspectRatio.xMaxYMin:
                        minX += width - Width * scaleX;
                        break;
                    case SvgPreserveAspectRatio.xMinYMid:
                        minY += midY - viewMidY;
                        break;
                    case SvgPreserveAspectRatio.xMidYMid:
                        minX += midX - viewMidX;
                        minY += midY - viewMidY;
                        break;
                    case SvgPreserveAspectRatio.xMaxYMid:
                        minX += width - Width * scaleX;
                        minY += midY - viewMidY;
                        break;
                    case SvgPreserveAspectRatio.xMinYMax:
                        minY += height - Height * scaleY;
                        break;
                    case SvgPreserveAspectRatio.xMidYMax:
                        minX += midX - viewMidX;
                        minY += height - Height * scaleY;
                        break;
                    case SvgPreserveAspectRatio.xMaxYMax:
                        minX += width - Width * scaleX;
                        minY += height - Height * scaleY;
                        break;
                }
            }
        }
    }
}
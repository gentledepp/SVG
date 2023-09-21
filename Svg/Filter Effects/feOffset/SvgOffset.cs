using System;

using System.Collections.Generic;
using Svg.Interfaces;

namespace Svg.FilterEffects
{
	/// <summary>
	/// Note: this is not used in calculations to bitmap - used only to allow for svg xml output
	/// </summary>
    [SvgElement("feOffset")]
	public class SvgOffset : SvgFilterPrimitive
    {
        private SvgAttributeCollection.Attribute<SvgUnit> _dx;
        private SvgAttributeCollection.Attribute<SvgUnit> _dy;


        /// <summary>
		/// The amount to offset the input graphic along the x-axis. The offset amount is expressed in the coordinate system established by attribute ‘primitiveUnits’ on the ‘filter’ element.
		/// If the attribute is not specified, then the effect is as if a value of 0 were specified.
		/// Note: this is not used in calculations to bitmap - used only to allow for svg xml output
		/// </summary>
		[SvgAttribute("dx")]
		public SvgUnit Dx
        {
            get { return (_dx ??= this.Attributes.GetAttribute<SvgUnit>("dx")).GetValue(); }
            set { this.Attributes["dx"] = value; }
        }


        /// <summary>
        /// The amount to offset the input graphic along the y-axis. The offset amount is expressed in the coordinate system established by attribute ‘primitiveUnits’ on the ‘filter’ element.
        /// If the attribute is not specified, then the effect is as if a value of 0 were specified.
        /// Note: this is not used in calculations to bitmap - used only to allow for svg xml output
        /// </summary>
        [SvgAttribute("dy")]
        public SvgUnit Dy
        {
            get { return (_dy ??= this.Attributes.GetAttribute<SvgUnit>("dy")).GetValue(); }
            set { this.Attributes["dy"] = value; }
        }



        public override void Process(ImageBuffer buffer)
		{
            var inputImage = buffer[this.Input];
            var result = Bitmap.Create(inputImage.Width, inputImage.Height);

            var pts = new PointF[] { PointF.Create(this.Dx.ToDeviceValue(null, UnitRenderingType.Horizontal, null), 
                                                this.Dy.ToDeviceValue(null, UnitRenderingType.Vertical, null)) };
            buffer.Transform.TransformVectors(pts);

            using (var g = SvgEngine.Factory.CreateGraphicsFromImage(result))
            {
                g.DrawImage(inputImage, RectangleF.Create((int)pts[0].X, (int)pts[0].Y, 
                                                      inputImage.Width, inputImage.Height),
                            0, 0, inputImage.Width, inputImage.Height, GraphicsUnit.Pixel);
                g.Flush();
            }
            buffer[this.Result] = result;
		}




		public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgOffset>();
		}

		public override SvgElement DeepCopy<T>()
		{
			var newObj = base.DeepCopy<T>() as SvgOffset;
			newObj.Dx = this.Dx;
			newObj.Dy = this.Dy;
	
			return newObj;
		}

    }
}

using System.Linq;
using Svg.Interfaces;

namespace Svg.FilterEffects
{
	[SvgElement("feMerge")]
    public class SvgMerge : SvgFilterPrimitive
    {
        public override void Process(ImageBuffer buffer)
        {
            var children = this.Children.OfType<SvgMergeNode>().ToList();
            var inputImage = buffer[children.First().Input];
            var result = Bitmap.Create(inputImage.Width, inputImage.Height);
            using (var g = SvgEngine.Factory.CreateGraphicsFromImage(result))
            {
                foreach (var child in children)
                {
                    g.DrawImage(buffer[child.Input], RectangleF.Create(0, 0, inputImage.Width, inputImage.Height),
                                0, 0, inputImage.Width, inputImage.Height, GraphicsUnit.Pixel);
                }
                g.Flush();
            }
            //result.SavePng(@"C:\test.png");
            buffer[this.Result] = result;
        }

		public override SvgElement DeepCopy()
		{
            return DeepCopy<SvgMerge>();
		}

    }
}
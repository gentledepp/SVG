
namespace Svg.Interfaces
{
    public interface IAlternativeSvgTextRenderer
    {
        void Render(SvgTextBase svgTextBase, ISvgRenderer renderer);
        RectangleF GetBounds(SvgTextBase txt, ISvgRenderer renderer);
    }
}

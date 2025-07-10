using Svg.Interfaces;

namespace Svg
{
    public class Region
    {
        private readonly RectangleF _rect;

        public Region(RectangleF rect)
        {
            _rect = rect;
        }

        public Region(GraphicsPath path)
        {
            _rect = path.GetBounds();
        }

        public RectangleF Rect
        {
            get { return _rect; }
        }

        public Region Clone()
        {
            return new Region(Rect);
        }

        public void Exclude(GraphicsPath path)
        {
            //_rect = _rect - path.GetBounds();

            // TODO LX: wtf?
        }

        public RectangleF GetBounds(Graphics graphics)
        {
            //throw new System.NotImplementedException();
            return _rect;
        }
    }
}
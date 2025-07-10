using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Svg.Interfaces;

namespace Svg
{
    internal class GenericBoundable : ISvgBoundable
    {
        private RectangleF _rect;

        public GenericBoundable(RectangleF rect)
        {
            _rect = rect;
        }
        public GenericBoundable(float x, float y, float width, float height)
        {
            _rect = RectangleF.Create(x, y, width, height);
        }

        public PointF Location
        {
            get { return _rect.Location; }
        }

        public SizeF Size
        {
            get { return _rect.Size; }
        }

        public RectangleF Bounds
        {
            get { return _rect; }
        }
    }
}

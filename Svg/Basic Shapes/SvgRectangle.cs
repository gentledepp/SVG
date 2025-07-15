using System;
using System.Collections.Generic;
using Svg.Basic_Shapes;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Represents and SVG rectangle that could also have reounded edges.
    /// </summary>
    [SvgElement("rect")]
    public class SvgRectangle : SvgVisualElement
    {
        private SvgUnit _cornerRadiusX;
        private SvgUnit _cornerRadiusY;
        private SvgUnit _height;
        private GraphicsPath _path;
        private SvgUnit _width;
        private SvgUnit _x;
        private SvgUnit _y;

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgRectangle"/> class.
        /// </summary>
        public SvgRectangle()
        {
            _width = new SvgUnit(0.0f);
            _height = new SvgUnit(0.0f);
            _cornerRadiusX = new SvgUnit(0.0f);
            _cornerRadiusY = new SvgUnit(0.0f);
            _x = new SvgUnit(0.0f);
            _y = new SvgUnit(0.0f);
        }

        /// <summary>
        /// Gets an <see cref="SvgPoint"/> representing the top left point of the rectangle.
        /// </summary>
        public SvgPoint Location
        {
            get { return new SvgPoint(X, Y); }
        }

        /// <summary>
        /// Gets or sets the position where the left point of the rectangle should start.
        /// </summary>
        [SvgAttribute("x")]
        public SvgUnit X
        {
        	get { return _x; }
        	set
        	{
        		if(_x != value)
		        {
		            var oldValue = _x;
        			_x = value;
		            this.Attributes["x"] = value;
        			OnAttributeChanged(new AttributeEventArgs("x", value, oldValue));
        			IsPathDirty = true;
        		}
        	}
        }

        /// <summary>
        /// Gets or sets the position where the top point of the rectangle should start.
        /// </summary>
        [SvgAttribute("y")]
        public SvgUnit Y
        {
        	get { return _y; }
        	set
        	{
        		if(_y != value)
                {
                    var oldValue = _y;
                    _y = value;
                    this.Attributes["y"] = value;
                    OnAttributeChanged(new AttributeEventArgs("y", value, oldValue));
        			IsPathDirty = true;
        		}
        	}
        }

        /// <summary>
        /// Gets or sets the width of the rectangle.
        /// </summary>
        [SvgAttribute("width")]
        public SvgUnit Width
        {
        	get { return _width; }
        	set
        	{
        		if(_width != value)
                {
                    var oldValue = _width;
                    _width = value;
                    this.Attributes["width"] = value;
                    OnAttributeChanged(new AttributeEventArgs("width", value, oldValue));
        			IsPathDirty = true;
        		}
        	}
        }

        /// <summary>
        /// Gets or sets the height of the rectangle.
        /// </summary>
        [SvgAttribute("height")]
        public SvgUnit Height
        {
        	get { return _height; }
        	set
        	{
        		if(_height != value)
                {
                    var oldValue = _height;
                    _height = value;
                    this.Attributes["height"] = value;
                    OnAttributeChanged(new AttributeEventArgs("height", value, oldValue));
        			IsPathDirty = true;
        		}
        	}
        }

        /// <summary>
        /// Gets or sets the X-radius of the rounded edges of this rectangle.
        /// </summary>
        [SvgAttribute("rx")]
        public SvgUnit CornerRadiusX
        {
            get { return _cornerRadiusX; }
            set
            {
                _cornerRadiusX = value;
                this.Attributes["rx"] = value;
                IsPathDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets the Y-radius of the rounded edges of this rectangle.
        /// </summary>
        [SvgAttribute("ry")]
        public SvgUnit CornerRadiusY
        {
            get { return _cornerRadiusY; }
            set
            {
                _cornerRadiusY = value;
                this.Attributes["ry"] = value;
                IsPathDirty = true;
            }
        }

        /// <summary>
        /// Gets or sets a value to determine if anti-aliasing should occur when the element is being rendered.
        /// </summary>
        protected override bool RequiresSmoothRendering
        {
            get { return (CornerRadiusX.Value > 0 || CornerRadiusY.Value > 0); }
        }

        public override RectangleF GetBounds()
        {
            return this.GetBoundsWithClipPaths();
        }

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (_path == null || IsPathDirty)
            {
                // If the corners aren't to be rounded just create a rectangle
                if (CornerRadiusX.Value == 0.0f && CornerRadiusY.Value == 0.0f)
                {
                    var rectangle = RectangleF.Create(Location.ToDeviceValue(renderer, this),
                        SvgUnit.GetDeviceSize(this.Width, this.Height, renderer, this));
                    
                    _path?.Dispose();
                    _path = SvgEngine.Factory.CreateGraphicsPath();
                    _path.StartFigure();
                    _path.AddRectangle(rectangle);
                    _path.CloseFigure();
                }
                else
                {
                    _path?.Dispose();
                    _path = SvgEngine.Factory.CreateGraphicsPath();
                    var arcBounds = RectangleF.Create();
                    var lineStart = PointF.Create(0f,0f);
                    var lineEnd = PointF.Create(0f,0f);
                    var width = Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                    var height = Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
                    // Implement SVG spec: if rx is specified but not ry, ry = rx; if ry is specified but not rx, rx = ry
                    var rxSpecified = this.Attributes.ContainsKey("rx");
                    var rySpecified = this.Attributes.ContainsKey("ry");
                    var rxValue = _cornerRadiusX.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this);
                    var ryValue = _cornerRadiusY.ToDeviceValue(renderer, UnitRenderingType.Vertical, this);
                    
                    if (rxSpecified && !rySpecified)
                        ryValue = rxValue;
                    else if (rySpecified && !rxSpecified)
                        rxValue = ryValue;
                    
                    // Then clamp both values
                    var rx = Math.Min(rxValue * 2, width);
                    var ry = Math.Min(ryValue * 2, height);
                    var location = Location.ToDeviceValue(renderer, this);

                    // Start
                    _path.StartFigure();

                    // Add first arc
                    arcBounds.Location = location;
                    arcBounds.Width = rx;
                    arcBounds.Height = ry;
                    _path.AddArc(arcBounds, 180, 90);

                    // Add first line
                    lineStart.X = Math.Min(location.X + rx, location.X + width * 0.5f);
                    lineStart.Y = location.Y;
                    lineEnd.X = Math.Max(location.X + width - rx, location.X + width * 0.5f);
                    lineEnd.Y = lineStart.Y;
                    _path.AddLine(lineStart, lineEnd);

                    // Add second arc
                    arcBounds.Location = PointF.Create(location.X + width - rx, location.Y);
                    _path.AddArc(arcBounds, 270, 90);

                    // Add second line
                    lineStart.X = location.X + width;
                    lineStart.Y = Math.Min(location.Y + ry, location.Y + height * 0.5f);
                    lineEnd.X = lineStart.X;
                    lineEnd.Y = Math.Max(location.Y + height - ry, location.Y + height * 0.5f);
                    _path.AddLine(lineStart, lineEnd);

                    // Add third arc
                    arcBounds.Location = PointF.Create(location.X + width - rx, location.Y + height - ry);
                    _path.AddArc(arcBounds, 0, 90);

                    // Add third line
                    lineStart.X = Math.Max(location.X + width - rx, location.X + width * 0.5f);
                    lineStart.Y = location.Y + height;
                    lineEnd.X = Math.Min(location.X + rx, location.X + width * 0.5f);
                    lineEnd.Y = lineStart.Y;
                    _path.AddLine(lineStart, lineEnd);

                    // Add third arc
                    arcBounds.Location = PointF.Create(location.X, location.Y + height - ry);
                    _path.AddArc(arcBounds, 90, 90);

                    // Add fourth line
                    lineStart.X = location.X;
                    lineStart.Y = Math.Max(location.Y + height - ry, location.Y + height * 0.5f);
                    lineEnd.X = lineStart.X;
                    lineEnd.Y = Math.Min(location.Y + ry, location.Y + height * 0.5f);
                    _path.AddLine(lineStart, lineEnd);

                    // Close
                    _path.CloseFigure();
                }
                IsPathDirty = false;
            }
            return _path;
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="Graphics"/> object.
        /// </summary>
        protected override void Render(ISvgRenderer renderer)
        {
            if (Width.Value > 0.0f && Height.Value > 0.0f)
            {
                base.Render(renderer);
            }
        }

        protected internal override bool IntersectsWith(RectangleF rectangle, Matrix transform, int maxRecursion)
        {
            if (this.HasFill())
                return true;
            
            var leftTop = PointF.Create(this.X, this.Y);
            var rightTop = PointF.Create(this.X + this.Width, this.Y);
            var rightBottom = PointF.Create(this.X + this.Width, this.Y + this.Height);
            var leftBottom = PointF.Create(this.X, this.Y + this.Height);

            var lineSegments = new List<(PointF from, PointF to)>();
            lineSegments.Add((leftTop.Clone(), rightTop.Clone()));
            lineSegments.Add((rightTop.Clone(),rightBottom.Clone()));
            lineSegments.Add((rightBottom.Clone(),leftBottom.Clone()));
            lineSegments.Add((leftBottom.Clone(), leftTop.Clone()));

            return lineSegments.IsIntersectingWithLine(transform, rectangle);
        }

        public override SvgElement DeepCopy()
		{
			return DeepCopy<SvgRectangle>();
		}

		public override SvgElement DeepCopy<T>()
		{
 			var newObj = base.DeepCopy<T>() as SvgRectangle;
			newObj.CornerRadiusX = this.CornerRadiusX;
			newObj.CornerRadiusY = this.CornerRadiusY;
			newObj.Height = this.Height;
			newObj.Width = this.Width;
			newObj.X = this.X;
			newObj.Y = this.Y;
			return newObj;
		}
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Svg.Interfaces;

namespace Svg
{
    [SvgElement("use")]
    public class SvgUse : SvgVisualElement
    {
        private Uri _referencedElement;
        private SvgAttributeCollection.Attribute<SvgUnit> _x;
        private SvgAttributeCollection.Attribute<SvgUnit> _y;
        private SvgAttributeCollection.Attribute<SvgUnit> _width;
        private SvgAttributeCollection.Attribute<SvgUnit> _height;
        private bool _isRendering = false;

        [SvgAttribute("href", SvgAttributeAttribute.XLinkNamespace)]
        public virtual Uri ReferencedElement
        {
            get { return this._referencedElement; }
            set { this._referencedElement = value; }
        }

        [SvgAttribute("x")]
        public virtual SvgUnit X
        {
            get { return (_x ??= this.Attributes.GetAttribute<SvgUnit>("x")).GetValue(); }
            set { this.Attributes["x"] = value; }
        }

        [SvgAttribute("y")]
        public virtual SvgUnit Y
        {
            get { return (_y ??= this.Attributes.GetAttribute<SvgUnit>("y")).GetValue(); }
            set { this.Attributes["y"] = value; }
        }


        /// <summary>
        /// Gets or sets the width of the fragment.
        /// </summary>
        /// <value>The width.</value>
        [SvgAttribute("width")]
        public SvgUnit Width
        {
            get { return (_width ??= this.Attributes.GetAttribute<SvgUnit>("width")).GetValue(); }
            set { this.Attributes["width"] = value; }
        }

        /// <summary>
        /// Gets or sets the height of the fragment.
        /// </summary>
        /// <value>The height.</value>
        [SvgAttribute("height")]
        public SvgUnit Height
        {
            get { return (_height ??= this.Attributes.GetAttribute<SvgUnit>("height")).GetValue(); }
            set { this.Attributes["height"] = value; }
        }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        protected internal override bool PushTransforms(ISvgRenderer renderer)
        {
            if (!base.PushTransforms(renderer)) return false;
            renderer.TranslateTransform(this.X.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                        this.Y.ToDeviceValue(renderer, UnitRenderingType.Vertical, this));

            if (!_isRendering) // Only calculate scale if we're not already in a render cycle
            {
                var element = this.OwnerDocument?.IdManager?.GetElementById(this.ReferencedElement) as SvgVisualElement;
                if (element != null && element != this)
                {
                    var childBounds = element.Bounds;
                    var scaleX = this.Width != SvgUnit.None ? this.Width / childBounds.Width : 1;
                    var scaleY = this.Height != SvgUnit.None ? this.Height / childBounds.Height : 1;
                    
                    //renderer.ScaleTransform(scaleX, scaleY);
                    var scale = Math.Min(scaleX, scaleY);
                    renderer.ScaleTransform(scale, scale);
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgUse"/> class.
        /// </summary>
        public SvgUse()
        {
            this.X = 0;
            this.Y = 0;
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (_isRendering) return null; // Prevent recursion
            SvgVisualElement element = (SvgVisualElement)this.OwnerDocument?.IdManager?.GetElementById(this.ReferencedElement);
            return (element != null && element != this) ? element.Path(renderer) : null;
        }

        protected internal override bool Renderable { get { return false; } }

        private bool IsCircularReference(SvgVisualElement element)
        {
            var current = this.Parent;
            while (current != null)
            {
                if (current == element)
                    return true;
                if (current is SvgUse use && use.ReferencedElement?.Fragment == this.ReferencedElement?.Fragment)
                    return true;
                current = current.Parent;
            }
            return false;
        }

        protected override void Render(ISvgRenderer renderer)
        {
            if (this.Visible && this.Displayable && !_isRendering && this.PushTransforms(renderer))
            {
                _isRendering = true;
                try
                {
                    this.SetClip(renderer);

                    var element = this.OwnerDocument?.IdManager?.GetElementById(this.ReferencedElement) as SvgVisualElement;
                    if (element != null && element != this && !IsCircularReference(element))
                    {
                        this.ResetClip(renderer);
                        var origParent = element.Parent;
                        element._parent = this;
                        element.RenderElement(renderer);
                        element._parent = origParent;
                        this.PopTransforms(renderer);
                    }
                    else
                    {
                        this.ResetClip(renderer);
                        this.PopTransforms(renderer);
                    }
                }
                finally
                {
                    _isRendering = false;
                }
            }
        }

        public override PointF[] GetTransformedPoints(Matrix transform = null)
        {
            if (_isRendering) return Array.Empty<PointF>(); // Prevent recursion
            
            if (transform == null)
                transform = Matrix.Create();
            else
                transform = transform.Clone();
            
            var element = this.OwnerDocument?.IdManager?.GetElementById(this.ReferencedElement) as SvgVisualElement;
            
             if(element is null || element == this)
                return Array.Empty<PointF>();
            
            return element.GetTransformedElementPoints(transform);
        }

        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgUse>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgUse;
            newObj.ReferencedElement = this.ReferencedElement;
            newObj.X = this.X;
            newObj.Y = this.Y;

            return newObj;
        }

    }
}
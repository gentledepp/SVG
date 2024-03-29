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

            var element = this.OwnerDocument.IdManager.GetElementById(this.ReferencedElement) as SvgVisualElement;
            if (element != null)
            {
                var childBounds = element.Bounds;
                var scaleX = this.Width != SvgUnit.None ? this.Width / childBounds.Width : 1;
                var scaleY = this.Height != SvgUnit.None ? this.Height / childBounds.Height : 1;
                
                //renderer.ScaleTransform(scaleX, scaleY);
                var scale = Math.Min(scaleX, scaleY);
                renderer.ScaleTransform(scale, scale);
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
            SvgVisualElement element = (SvgVisualElement)this.OwnerDocument.IdManager.GetElementById(this.ReferencedElement);
            return (element != null) ? element.Path(renderer) : null;
        }

        protected internal override bool Renderable { get { return false; } }

        protected override void Render(ISvgRenderer renderer)
        {
            if (this.Visible && this.Displayable && this.PushTransforms(renderer))
            {
                this.SetClip(renderer);

                var element = this.OwnerDocument.IdManager.GetElementById(this.ReferencedElement) as SvgVisualElement;
                if (element != null)
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
using System;
using System.IO;
using System.Text.RegularExpressions;
using Svg.Interfaces;

namespace Svg
{
    /// <summary>
    /// Represents and SVG image
    /// </summary>
    [SvgElement("image")]
    public class SvgImage : SvgVisualElement
    {
        private object _img;
        private static readonly Regex _base64detector = new Regex(@"^data:[a-zA-Z/-]+;base64,");
        private SvgAttributeCollection.Attribute<SvgAspectRatio> _preserveAspectRatio;
        private SvgAttributeCollection.Attribute<SvgUnit> _x;
        private SvgAttributeCollection.Attribute<SvgUnit> _y;
        private SvgAttributeCollection.Attribute<SvgUnit> _width;
        private SvgAttributeCollection.Attribute<SvgUnit> _height;
        private SvgAttributeCollection.Attribute<string> _href;

        /// <summary>
		/// Initializes a new instance of the <see cref="SvgImage"/> class.
        /// </summary>
		public SvgImage()
        {
            //Width = SvgUnit.Empty;
            //Height = SvgUnit.Empty;
        }

        /// <summary>
        /// Gets an <see cref="SvgPoint"/> representing the top left point of the rectangle.
        /// </summary>
        public SvgPoint Location
        {
            get { return new SvgPoint(X, Y); }
        }

        /// <summary>
        /// Gets or sets the aspect of the viewport.
        /// </summary>
        /// <value></value>
        [SvgAttribute("preserveAspectRatio")]
        public SvgAspectRatio AspectRatio
        {
            get { return (_preserveAspectRatio ??= Attributes.GetAttribute<SvgAspectRatio>("preserveAspectRatio")).GetValue(); }
            set { Attributes["preserveAspectRatio"] = value; }
        }

        [SvgAttribute("x")]
        public virtual SvgUnit X
        {
            get { return (_x??=Attributes.GetAttribute<SvgUnit>("x")).GetValue(); }
            set { Attributes["x"] = value; }
        }

        [SvgAttribute("y")]
        public virtual SvgUnit Y
        {
            get { return (_y??=Attributes.GetAttribute<SvgUnit>("y")).GetValue(); }
            set { Attributes["y"] = value; }
        }


        [SvgAttribute("width")]
        public virtual SvgUnit Width
        {
            get { return (_width ??= Attributes.GetAttribute<SvgUnit>("width")).GetValue(); }
            set { Attributes["width"] = value; }
        }

        [SvgAttribute("height")]
        public virtual SvgUnit Height
        {
            get { return (_height ??= Attributes.GetAttribute<SvgUnit>("height")).GetValue(); }
            set { Attributes["height"] = value; }
        }

        [SvgAttribute("href", SvgAttributeAttribute.XLinkNamespace)]
        public virtual string Href
        {
            get { return (_href ??= Attributes.GetAttribute<string>("href")).GetValue(); }
            set
            {
                Attributes["href"] = value;
                DisposeImage();
            }
        }

        private void DisposeImage()
        {
            (_img as IDisposable)?.Dispose();
            _img = null;
        }


        public override RectangleF GetBounds()
        {
            if (this.ClipPath != null)
            {
                SvgClipPath clipPath = this.OwnerDocument.GetElementById<SvgClipPath>(this.ClipPath.OriginalString);
                if (clipPath != null)
                {
                    return clipPath.Bounds;
                }
            }

            // if a width/height is set explicitly, use that
            if (!Width.IsNone && !Width.IsEmpty && !Height.IsNone && !Height.IsEmpty)
                return RectangleF.Create(Location.ToDeviceValue(null, this),
                    SizeF.Create(Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this),
                        Height.ToDeviceValue(null, UnitRenderingType.Vertical, this)));

            var bmp = _img as Image;
            var svg = _img as SvgFragment;

            if (bmp != null)
            {
                return RectangleF.Create(Location.ToDeviceValue(null, this), SizeF.Create(bmp.Width, bmp.Height));
            }

            if (svg != null)
            {
                return RectangleF.Create(Location.ToDeviceValue(null, this), svg.Bounds.Size);
            }

            return RectangleF.Create(Location.ToDeviceValue(null, this),
                SizeF.Create(Width.ToDeviceValue(null, UnitRenderingType.Horizontal, this),
                    Height.ToDeviceValue(null, UnitRenderingType.Vertical, this)));
        }

        /// <summary>
        /// Gets the <see cref="GraphicsPath"/> for this element.
        /// </summary>
        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            return null;
        }

        public override void DisposeOverride()
        {
            base.DisposeOverride();

            DisposeImage();
        }
        
        protected internal class ImageCache : RenderCacheEntryBase
        {
            public object Image { get; set; }
            public override void Dispose()
            {
                if(Image is IDisposable i)
                    i.Dispose();
                Image = null;
            }
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="Graphics"/> object.
        /// </summary>
        protected override void Render(ISvgRenderer renderer)
        {
            if (!Visible || !Displayable)
                return;

            var cache = GetOrCreateRenderCacheEntry<ImageCache>(renderer);
            cache.Image ??= GetImage(Href);

            if (Href != null)
            {
                var img = cache.Image;
                if (img != null)
                {
                    RectangleF srcRect;
                    var bmp = img as Image;
                    var svg = img as SvgFragment;
                    if (bmp != null)
                    {
                        srcRect = RectangleF.Create(0, 0, bmp.Width, bmp.Height);
                    }
                    else if (svg != null)
                    {
                        srcRect = RectangleF.Create(PointF.Create(0, 0), svg.GetDimensions());
                    }
                    else
                    {
                        return;
                    }

                    var destClip = (Width.IsEmpty || Width.IsNone) && (Height.IsEmpty || Height.IsNone)
                            ? RectangleF.Create(Location.ToDeviceValue(renderer, this), srcRect.Size)
                            : RectangleF.Create(Location.ToDeviceValue(renderer, this),
                                SizeF.Create(Width.ToDeviceValue(renderer, UnitRenderingType.Horizontal, this),
                                Height.ToDeviceValue(renderer, UnitRenderingType.Vertical, this)));
                    var destRect = destClip;

                    PushTransforms(renderer);
                    renderer.SetClip(SvgEngine.Factory.CreateRegion(destClip), CombineMode.Intersect);
                    if (Parent is not SvgUse)
                        SetClip(renderer);

                    if (AspectRatio != null && AspectRatio.Align != SvgPreserveAspectRatio.none)
                    {
                        var fScaleX = destClip.Width / srcRect.Width;
                        var fScaleY = destClip.Height / srcRect.Height;
                        var xOffset = 0.0f;
                        var yOffset = 0.0f;

                        if (AspectRatio.Slice)
                        {
                            fScaleX = Math.Max(fScaleX, fScaleY);
                            fScaleY = Math.Max(fScaleX, fScaleY);
                        }
                        else
                        {
                            fScaleX = Math.Min(fScaleX, fScaleY);
                            fScaleY = Math.Min(fScaleX, fScaleY);
                        }

                        switch (AspectRatio.Align)
                        {
                            case SvgPreserveAspectRatio.xMinYMin:
                                break;
                            case SvgPreserveAspectRatio.xMidYMin:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                                break;
                            case SvgPreserveAspectRatio.xMaxYMin:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX);
                                break;
                            case SvgPreserveAspectRatio.xMinYMid:
                                yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                                break;
                            case SvgPreserveAspectRatio.xMidYMid:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                                yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                                break;
                            case SvgPreserveAspectRatio.xMaxYMid:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX);
                                yOffset = (destClip.Height - srcRect.Height * fScaleY) / 2;
                                break;
                            case SvgPreserveAspectRatio.xMinYMax:
                                yOffset = (destClip.Height - srcRect.Height * fScaleY);
                                break;
                            case SvgPreserveAspectRatio.xMidYMax:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX) / 2;
                                yOffset = (destClip.Height - srcRect.Height * fScaleY);
                                break;
                            case SvgPreserveAspectRatio.xMaxYMax:
                                xOffset = (destClip.Width - srcRect.Width * fScaleX);
                                yOffset = (destClip.Height - srcRect.Height * fScaleY);
                                break;
                        }

                        destRect = RectangleF.Create(destClip.X + xOffset, destClip.Y + yOffset,
                                                    srcRect.Width * fScaleX, srcRect.Height * fScaleY);
                    }

                    if (bmp != null)
                    {
                        renderer.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
                        //bmp.Dispose();
                    }
                    else if (svg != null)
                    {
                        var currOffset = PointF.Create(renderer.Transform.OffsetX, renderer.Transform.OffsetY);
                        renderer.TranslateTransform(-currOffset.X, -currOffset.Y);
                        renderer.ScaleTransform(destRect.Width / srcRect.Width, destRect.Height / srcRect.Height);
                        renderer.TranslateTransform(currOffset.X + destRect.X, currOffset.Y + destRect.Y);
                        renderer.SetBoundable(new GenericBoundable(srcRect));
                        svg.RenderElement(renderer);
                        renderer.PopBoundable();
                    }

                    if(Parent is not SvgUse)
                        ResetClip(renderer);
                    PopTransforms(renderer);
                }
                // TODO: cache images... will need a shared context for this
                // TODO: support preserveAspectRatio, etc
            }
        }

        public bool IsBase64Image => !string.IsNullOrEmpty(Href) && _base64detector.IsMatch(Href);

        protected object GetImage(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                // handle data/uri embedded images (http://en.wikipedia.org/wiki/Data_URI_scheme)
                // base 65 images start with "data:image/jpeg;base64,"
                if (IsBase64Image)
                {
                    int dataIdx = url.IndexOf(",") + 1;
                    if (dataIdx <= 0 || dataIdx + 1 > url.Length)
                        throw new Exception("Invalid data URI");

                    // we're assuming base64, as ascii encoding would be *highly* unsusual for images
                    // also assuming it's png or jpeg mimetype
                    byte[] imageBytes = Convert.FromBase64String(url.Substring(dataIdx));
                    using (var stream = new MemoryStream(imageBytes))
                    {
                        return SvgEngine.Factory.CreateImageFromStream(stream);
                    }
                }

                var uri = new Uri(url);
                if (!uri.IsAbsoluteUri && OwnerDocument.BaseUri != null)
                {
                    uri = new Uri(OwnerDocument.BaseUri, uri);
                }
                
                using (var stream = SvgEngine.Resolve<IWebRequest>().GetResponse(uri))
                {
                    stream.Position = 0;
                    if (uri.IsAbsoluteUri && uri.LocalPath.ToLowerInvariant().EndsWith(".svg"))
                    {
                        var doc = SvgDocument.Open<SvgDocument>(stream);
                        doc.BaseUri = uri;
                        return doc;
                    }
                    else
                    {
                        return SvgEngine.Factory.CreateBitmapFromStream(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                //Trace.TraceError("Error loading image: '{0}', error: {1} ", uri, ex.Message);
                return null;
            }
        }

        protected static MemoryStream BufferToMemoryStream(Stream input)
        {
            byte[] buffer = new byte[4 * 1024];
            int len;
            MemoryStream ms = new MemoryStream();
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, len);
            }
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }


        public override SvgElement DeepCopy()
        {
            return DeepCopy<SvgImage>();
        }

        public override SvgElement DeepCopy<T>()
        {
            var newObj = base.DeepCopy<T>() as SvgImage;
            newObj.Height = Height;
            newObj.Width = Width;
            newObj.X = X;
            newObj.Y = Y;
            newObj.Href = Href;
            return newObj;
        }
    }
}
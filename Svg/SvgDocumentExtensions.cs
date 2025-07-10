using System;
using System.IO;
using System.Linq;
using Svg.Interfaces;

namespace Svg;

public static class SvgDocumentExtensions
{
    public static SvgImage AddImageInBackground(this SvgDocument doc, string path)
    {
        try
        {
            var children = doc.Children;

            // create image from path
            var image = new SvgImage
            {
                Href = $"file://{path}"
            };
            var size = image.GetImageSize();
            image.X = new SvgUnit(0);
            image.Y = new SvgUnit(0);
            image.Width = new SvgUnit(size.Width);
            image.Height = new SvgUnit(size.Height);
                
            // insert the background before the first visible element
            var index = children.IndexOf(children.FirstOrDefault(x => x is SvgVisualElement));

            // if there are no visual elements, we want to add it to the end of the list
            if (index == -1) index = children.Count;

            children.Insert(index, image);

            return image;
        }
        catch (IOException)
        {
            throw;
        }
    }
    public static SvgImage AddImage(this SvgDocument doc, string path)
    {
        try
        {
            var children = doc.Children;

            // create image from path
            if (!path.StartsWith("/")) path = path.Insert(0, "/");
            var image = new SvgImage
            {
                Href = $"file://{path}"
            };
            var size = image.GetImageSize();
            image.X = new SvgUnit(0);
            image.Y = new SvgUnit(0);
            image.Width = new SvgUnit(size.Width);
            image.Height = new SvgUnit(size.Height);

            children.Add(image);

            return image;
        }
        catch (IOException)
        {
            throw;
        }
    }

    public static Bitmap CaptureDocumentImageBitmap(this SvgDocument document,
        RectangleF constraints = null,
        int maxSize = 4096,
        Color backgroundColor = null)
    {
        return CaptureDocumentBitmapInternal(document, document.CalculateDocumentImageBounds(), constraints, maxSize,
            backgroundColor);
    }

    public static Bitmap CaptureDocumentBitmap(this SvgDocument document,
        RectangleF constraints = null,
        int maxSize = 4096,
        Color backgroundColor = null)
    {
        return CaptureDocumentBitmapInternal(document, document.CalculateDocumentBounds(), constraints, maxSize,
            backgroundColor);
    }

    private static Bitmap CaptureDocumentBitmapInternal(SvgDocument document,RectangleF documentBounds, 
        RectangleF constraints = null,
        int maxSize = 4096, 
        Color backgroundColor = null)
    {
        // determine width and height of the bitmap by the minimum of the whole document's and the constraint's size
        var drawingWidth = (int)Math.Round(Math.Min(documentBounds.Width, constraints?.Width ?? float.MaxValue));
        var drawingHeight =
            (int)Math.Round(Math.Min(documentBounds.Height, constraints?.Height ?? float.MaxValue));

        // adjust width and height of the resulting bitmap to the maxSize parameter
        var bitmapWidth = drawingWidth;
        var bitmapHeight = drawingHeight;

        if (bitmapWidth > maxSize)
        {
            var factor = bitmapWidth / maxSize;
            bitmapWidth /= factor;
            bitmapHeight /= factor;
        }

        if (bitmapHeight > maxSize)
        {
            var factor = bitmapHeight / maxSize;
            bitmapWidth /= factor;
            bitmapHeight /= factor;
        }

        var bitmap = Bitmap.Create(bitmapWidth, bitmapHeight);

        return document.RenderBitmap(bitmap, backgroundColor, documentBounds, constraints);
    }

    public static Bitmap RenderBitmap(this SvgDocument document, Bitmap bitmap, Color backgroundColor,
        RectangleF drawingClip, RectangleF constraints = null)
    {
        if (drawingClip == null || drawingClip == RectangleF.Empty) return bitmap;

        // stash the old values
        var oldWidth = document.Width;
        var oldHeight = document.Height;
        var oldViewBox = document.ViewBox;

        try
        {
            document.SetDocumentViewbox(drawingClip, constraints);
            document.Draw(bitmap, backgroundColor ?? SvgEngine.Factory.Colors.Black);

            return bitmap;
        }
        finally
        {
            // reset to old values
            document.ViewBox = oldViewBox;
            document.Width = oldWidth;
            document.Height = oldHeight;
        }
    }

    public static void SetDocumentViewbox(this SvgDocument document, RectangleF drawingSize, RectangleF constraints = null)
    {
        var x = Math.Max(drawingSize.X, constraints?.X ?? float.MinValue);
        var y = Math.Max(drawingSize.Y, constraints?.Y ?? float.MinValue);
        var width = Math.Min(drawingSize.Width, constraints?.Width ?? float.MaxValue);
        var height = Math.Min(drawingSize.Height, constraints?.Height ?? float.MaxValue);
        document.Width = new SvgUnit(SvgUnitType.Pixel, width);
        document.Height = new SvgUnit(SvgUnitType.Pixel, height);
        document.ViewBox = new SvgViewBox(x, y, width, height);
        document.AspectRatio = new SvgAspectRatio(SvgPreserveAspectRatio.xMidYMid, true);
    }

}
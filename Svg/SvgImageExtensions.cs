using System;
using System.IO;
using Svg.Interfaces;

namespace Svg;

public static class SvgImageExtensions
{

    public static SizeF GetImageSize(this SvgImage image)
    {
        // handle data/uri embedded images (http://en.wikipedia.org/wiki/Data_URI_scheme)
        if (image.Href.IsAbsoluteUri && image.Href.Scheme == "data")
        {
            string uriString = image.Href.OriginalString;
            int dataIdx = uriString.IndexOf(",") + 1;
            if (dataIdx <= 0 || dataIdx + 1 > uriString.Length)
                throw new Exception("Invalid data URI");

            // we're assuming base64, as ascii encoding would be *highly* unsusual for images
            // also assuming it's png or jpeg mimetype
            byte[] imageBytes = Convert.FromBase64String(uriString.Substring(dataIdx));
            using (var stream = new MemoryStream(imageBytes))
            {
                var img = SvgEngine.Factory.CreateImageFromStream(stream);
                return SizeF.Create(img.Width, img.Height);
            }
        }

        if (!image.Href.IsAbsoluteUri && image.OwnerDocument.BaseUri != null)
        {
            image.Href = new Uri(image.OwnerDocument.BaseUri, image.Href);
        }

        //// should work with http: and file: protocol urls
        //var httpRequest = WebRequest.Create(uri);
        //using (WebResponse webResponse = httpRequest.GetResponse())
        using (var stream = SvgEngine.Resolve<IWebRequest>().GetResponse(image.Href))
        {
            stream.Position = 0;
            if (image.Href.LocalPath.ToLowerInvariant().EndsWith(".svg"))
            {
                var doc = SvgDocument.Open<SvgDocument>(stream);
                doc.BaseUri = image.Href;
                return doc.GetDimensions();
            }
            else
            {
                var img = SvgEngine.Factory.CreateBitmapFromStream(stream);
                return SizeF.Create(img.Width, img.Height);
            }
        }
    }
}
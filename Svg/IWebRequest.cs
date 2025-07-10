using System;
using System.IO;

namespace Svg
{
    public interface IWebRequest
    {
        Stream GetResponse(Uri uri);
    }
}

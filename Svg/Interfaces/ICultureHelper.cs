using System;
using System.Globalization;

namespace Svg.Interfaces
{
    public interface ICultureHelper
    {
        IDisposable UsingCulture(CultureInfo culture);
    }
}

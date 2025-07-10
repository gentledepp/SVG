using System;
using System.Globalization;
using System.Threading;
using Svg.Interfaces;

namespace Svg
{
    public class CultureHelper : ICultureHelper
    {
        public IDisposable UsingCulture(CultureInfo culture)
        {
            return new CultureHelperDisposable(culture);
        }

        private class CultureHelperDisposable : IDisposable
        {
            private CultureInfo _old;
#if WINDOWS_UWP
            public CultureHelperDisposable(CultureInfo culture)
            {
                _old = CultureInfo.DefaultThreadCurrentCulture;
                CultureInfo.DefaultThreadCurrentCulture = culture;
            }

            public void Dispose()
            {
                CultureInfo.DefaultThreadCurrentCulture = _old;
            }

#else
            public CultureHelperDisposable(CultureInfo culture)
            {
                _old = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = culture;
            }

            public void Dispose()
            {
                Thread.CurrentThread.CurrentCulture = _old;
            }
#endif
        }
    }
}
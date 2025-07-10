using System;
using System.Runtime.InteropServices;
using Svg.Interfaces;

namespace Svg
{
    public class SvgMarshal : IMarshal
    {
        public void Copy(IntPtr source, byte[] destination, int startIndex, int length)
        {
            Marshal.Copy(source, destination, startIndex, length);
        }

        public void Copy(byte[] source, int startIndex, IntPtr destination, int length)
        {
            Marshal.Copy(source, startIndex, destination, length);
        }
    }
}
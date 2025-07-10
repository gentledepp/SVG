using System;
namespace Svg.Interfaces
{
    public interface IMarshal
    {
        void Copy(IntPtr source, byte[] destination, int startIndex, int length);
        void Copy(byte[] source, int startIndex, IntPtr destination, int length);
    }
}

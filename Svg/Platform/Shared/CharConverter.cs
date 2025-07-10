using System;
using System.Net;
using Svg.Interfaces;

namespace Svg
{
    public class SvgCharConverter : ICharConverter
    {
        public string ConvertFromUtf32(int charCode)
        {
            return char.ConvertFromUtf32(charCode);
        }

        public int ConvertToUtf32(string s, int index)
        {
            return char.ConvertToUtf32(s, index);
        }
    }
}
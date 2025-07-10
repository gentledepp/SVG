using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ICharConverter
    {
        string ConvertFromUtf32(int charCode);

        int ConvertToUtf32(string s, int index);
    }
}

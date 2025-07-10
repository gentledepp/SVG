using System;
using System.Collections.Generic;
using System.Text;

namespace Svg.Editor.Tests
{
    public interface IFileLoader
    {
        SvgDocument Load(string fileName);
    }
}

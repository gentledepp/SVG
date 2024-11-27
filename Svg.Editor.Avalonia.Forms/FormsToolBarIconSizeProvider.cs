using System;
using System.Runtime.InteropServices;
using Svg.Interfaces;

namespace Svg.Editor.Avalonia.Forms;

public class FormsToolBarIconSizeProvider
{
    public SizeF GetSize()
    {
        if (OperatingSystem.IsIOS())
            return SizeF.Create(32, 32);
        if (OperatingSystem.IsAndroid())
            return SizeF.Create(32, 32);

        return SizeF.Create(32, 32);
    }
}
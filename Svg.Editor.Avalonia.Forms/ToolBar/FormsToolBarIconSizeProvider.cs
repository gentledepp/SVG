using System;
using Svg.Interfaces;

namespace Svg.Editor.Avalon.Forms.ToolBar
{
    public static class FormsToolBarIconSizeProvider
    {
        public static SizeF GetSize()
        {
            if (OperatingSystem.IsIOS()) return SizeF.Create(32, 32);
            if (OperatingSystem.IsAndroid()) return SizeF.Create(32, 32);
            return SizeF.Create(32, 32);
        }
    }
}
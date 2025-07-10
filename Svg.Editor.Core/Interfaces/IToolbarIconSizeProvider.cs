using Svg.Interfaces;

namespace Svg.Editor.Interfaces
{
    /// <summary>
    /// Provides the toolbar/appbar/menuitem icon size per platform
    /// see: http://iconhandbook.co.uk/reference/chart/
    /// </summary>
    public interface IToolbarIconSizeProvider
    {
        SizeF GetSize();
    }
}

using System.Collections.Generic;
using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public static class ToolBarGeometryIcons
{
    public static Dictionary<string, StreamGeometry> Icons => new Dictionary<string, StreamGeometry>()
    {
        {
            "ic_arrow_downward",
            StreamGeometry.Parse("M20,12 L18.59,10.59 L13,16.17 V4 H11 V16.17 L5.42,10.59 L4,12 L12,20 L20,12 Z")
        },
        {
            "ic_arrow_upward",
            StreamGeometry.Parse("M4,12 L5.41,13.41 L11,7.83 V20 H13 V7.83 L18.58,13.41 L20,12 L12,4 L4,12 Z")
        },
        {
            "ic_aspect_ratio",
            StreamGeometry.Parse(
                "M19,12 H17 V15 H14 V17 H19 V12 Z M7,9 H10 V7 H5 V12 H7 V9 Z M21,3 H3 C1.9,3 1,3.9 1,5 V19 C1,20.1 1.9,21 3,21 H21 C22.1,21 23,20.1 23,19 V5 C23,3.9 22.1,3 21,3 Z M21,19.01 H3 V4.99 H21 V19.01 Z")
        },
        {
            "bordic_border_style",
            StreamGeometry.Parse(
                "M15,21 H17 V19 H15 V21 Z M19,21 H21 V19 H19 V21 Z M7,21 H9 V19 H7 V21 Z M11,21 H13 V19 H11 V21 Z M19,17 H21 V15 H19 V17 Z M19,13 H21 V11 H19 V13 Z M3,3 V21 H5 V5 H21 V3 H3 Z M19,9 H21 V7 H19 V9 Z")
        },

    };
}
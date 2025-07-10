using System;
using System.Collections.Generic;
using System.Reflection;
using Svg.Editor.Sample.Core.Resources.svg;
using Svg.Editor.Tools;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg.Editor.Sample.Core.Tools
{
    public class AddRandomItemTool : ToolBase
    {
        public AddRandomItemTool() : base("Add random item")
        {
            ToolType = ToolType.Create;
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Add random item", (obj) =>
                {
	                var source = new EmbeddedResourceSource($"{typeof(ResourceMarker).Namespace}.findingmarker.svg", typeof(ResourceMarker).GetTypeInfo().Assembly);
	                SvgDocument otherDoc;
					using (var stream = source.GetStream())
						otherDoc = SvgDocument.Open<SvgDocument>(stream);

					Canvas.AddItemInScreenCenter(otherDoc);

                } , sortFunc:(x) => 1200)
            };
        }

        public Func<string, ISvgSource> SourceProvider { get; set; }
    }
}

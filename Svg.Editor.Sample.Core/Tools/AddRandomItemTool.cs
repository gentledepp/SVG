﻿using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Core;
using Svg.Core.Tools;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Droid.SampleEditor.Core.Tools
{
    public class AddRandomItemTool : ToolBase
    {

        private readonly SvgDrawingCanvas _canvas;

        public AddRandomItemTool(SvgDrawingCanvas canvas, Func<string, ISvgSource> sourceProvider = null) : base("Add random item")
        {
            SourceProvider = sourceProvider;
            _canvas = canvas;
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Add random item", (obj) =>
                {
                    if (SourceProvider == null)
                        return;
                    //var provider = SourceProvider("isolib/Welds/solid/weld1.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve1.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve2.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve3.svg");
                    //var provider = SourceProvider("isolib/Valves/Valves/valve4.svg");
                    //var provider = SourceProvider("isolib/Reducers/solid/reducer1.svg");
                    //var provider = SourceProvider("isolib/Straights/solid and broken/solid1.svg");
                    //var provider = SourceProvider("svg/painting-control-01-f.svg");
                    //var provider = SourceProvider("svg/blind01.svg");
                    //var provider = SourceProvider("svg/Blinds_6.svg");
                    //var provider = SourceProvider("svg/Blinds_6_gezoomtes_minibild.svg");
                    //var provider = SourceProvider("svg/Positions_13_kein_text_im_minibild_und_canvas.svg");
                    var provider = SourceProvider("svg/ic_format_color_fill_white_48px.svg");
                    var otherDoc = SvgDocument.Open<SvgDocument>(provider);
                    var visibleChildren = otherDoc.Children.OfType<SvgVisualElement>().Where(e => e.Displayable && e.Visible).ToList();

                    var child = visibleChildren.First();
                    if (visibleChildren.Count > 1)
                    {
                        var group = new SvgGroup
                        {
                            Fill = otherDoc.Fill,
                            Stroke = otherDoc.Stroke
                        };
                        foreach (var visibleChild in visibleChildren)
                        {
                            group.Children.Add(visibleChild);
                        }
                        child = group;
                    }

                    //var z = canvas.ZoomFactor;
                    //var halfRelWidth = canvas.ScreenWidth/z/2;
                    //var halfRelHeight = canvas.ScreenHeight/z/2;
                    //var childBounds = child.GetBoundingBox();
                    //var halfRelChildWidth = childBounds.Width/2;
                    //var halfRelChildHeight = childBounds.Height/2;
                    //var centerPosX = -canvas.RelativeTranslate.X + halfRelWidth - halfRelChildWidth;
                    //var centerPosY = -canvas.RelativeTranslate.Y + halfRelHeight - halfRelChildHeight;

                    ////var bounds = child.GetBoundingBox();
                    //if (childBounds.X != 0)
                    //    centerPosX -= childBounds.X;
                    //if (childBounds.Y != 0)
                    //    centerPosY -= childBounds.Y;

                    //SvgTranslate tl = new SvgTranslate(centerPosX, centerPosY);
                    //child.Transforms.Add(tl);

                    //child.ID = $"{child.ElementName}_{_canvas.Document.Descendants().Count(d => d.ElementName == child.ElementName)+1}";

                    //_canvas.Document.Children.Add(child);
                    _canvas.AddItemInScreenCenter(child);
                    
                } , sortFunc:(x) => 1200)
            };
        }
        public Func<string, ISvgSource> SourceProvider { get; set; }
    }
}

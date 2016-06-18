﻿using System.Collections.Generic;
using System.Linq;
using Svg.Core.Events;
using Svg.Core.Interfaces;
using Svg.Interfaces;

namespace Svg.Core.Tools
{
    public enum SelectionType
    {
        Intersect,
        Contain
    }

    public class SelectionTool : ToolBase
    {
        private RectangleF _selectionRectangle = null;
        private Brush _brush;
        private Pen _pen;
        private bool _reselectsElements;

        public SelectionTool() : base("Select")
        {
        }

        private Brush Brush => _brush ?? (_brush = Svg.Engine.Factory.CreateSolidBrush(Svg.Engine.Factory.CreateColorFromArgb(255, 80, 210, 210)));
        private Pen Pen => _pen ?? (_pen = Svg.Engine.Factory.CreatePen(Brush, 5));

        public override void Initialize(SvgDrawingCanvas ws)
        {
            Commands = new List<IToolCommand>
            {
                new ToggleSelectionToolCommand(this, ws)
            };

            // make sure selection is inactive in case that panning is active at start
            this.IsActive = !ws.Tools.OfType<PanTool>().FirstOrDefault()?.IsActive ?? true;
        }

        public override void OnUserInput(UserInputEvent @event, SvgDrawingCanvas ws)
        {
            // skip if movetool is active
            var moveTool = ws.Tools.OfType<MoveTool>().SingleOrDefault();
            if (moveTool.IsActive)
                return;
            

            if (!IsActive)
                return;

            var e = @event as MoveEvent;
            if (e != null)
            {
                float startX = e.Pointer1Down.X;
                float startY = e.Pointer1Down.Y;
                float endX = e.Pointer1Position.X;
                float endY = e.Pointer1Position.Y;

                if (startX > endX)
                {
                    var t = startX;
                    startX = endX;
                    endX = t;
                }
                if (startY > endY)
                {
                    var t = startY;
                    startY = endY;
                    endY = t;
                }
                _selectionRectangle = Engine.Factory.CreateRectangleF(startX, startY, endX - startX, endY - startY);
                
                ws.FireInvalidateCanvas();
            }

            var p = @event as PointerEvent;
            if (p != null)
            {
                // if the user points down on an selected item, we do not draw the selection rectangle, so check that as well
                if (p.EventType == EventType.PointerDown)
                {
                    if (!ws.SelectedElements.Any())
                        _reselectsElements = false;

                    var elements = ws.GetElementsUnder(ws.GetPointerRectangle(p.Pointer1Position), SelectionType.Intersect);
                    if (elements.Any(element => ws.SelectedElements.Contains(element)))
                    {
                        _reselectsElements = true;
                    }
                }
                // if the user never moved, but clicked on an item, we try to select that spot
                else if (p.EventType == EventType.PointerUp && _selectionRectangle == null)
                {
                    // select elements under pointer
                    SelectElementsUnder(ws.GetPointerRectangle(p.Pointer1Position), ws, SelectionType.Intersect);
                    _selectionRectangle = null;

                    ws.FireInvalidateCanvas();
                }
                // on pointer up or cancel, we remove the selection rectangle
                else if (p.EventType == EventType.PointerUp || p.EventType == EventType.Cancel)
                {
                    // select elements under rectangle
                    SelectElementsUnder(_selectionRectangle, ws, SelectionType.Contain);

                    _selectionRectangle = null;
                    ws.FireInvalidateCanvas();
                }
            }
        }

        private void SelectElementsUnder(RectangleF selectionRectangle, SvgDrawingCanvas ws, SelectionType selectionType)
        {
            ws.SelectedElements.Clear();

            // the canvas has not been scaled and translated yet
            // we need to compare our rectangle to the translated boundingboxes of the svg elements
            var selected = ws.GetElementsUnder(selectionRectangle, selectionType);

            foreach (var element in selected)
                ws.SelectedElements.Add(element);
        }

        public override void OnDraw(IRenderer renderer, SvgDrawingCanvas ws)
        {
            // we draw the selection rectangle
            if (_selectionRectangle != null)
            {
                renderer.Graphics.Save();

                // as we are in "OnDraw", an panning as well as zoomin tools have already translated 
                // the canvas in order to properly render the svg elements zoomed and panned
                // we need to undo that translation, as our selection rectangle must be drawn
                // in absolute screen coordinates (below the finger of the user - not translated and scaled)
                var m = renderer.Matrix.Clone();
                m.Invert();
                renderer.Graphics.Concat(m);
                renderer.DrawRectangle(_selectionRectangle, Pen);
                
                renderer.Graphics.Save();
            }

            // we draw the selection boundingboxes of all selected elements
            foreach (var element in ws.SelectedElements)
            {
                renderer.Graphics.Save();
                var m = renderer.Matrix.Clone();
                m.Invert();
                renderer.Graphics.Concat(m);
                
                // we draw a selection adorner around all elements
                // as the canvas is already translated, we do not need to use the renderbounds, but the bounds themselves

                renderer.DrawRectangle(element.RenderBounds, Pen);

                renderer.Graphics.Save();
            }
        }

        private class ToggleSelectionToolCommand : ToolCommand
        {
            private readonly SvgDrawingCanvas _canvas;

            public ToggleSelectionToolCommand(SelectionTool tool, SvgDrawingCanvas canvas) : base(tool, "Select", (obj)=> {})
            {
                _canvas = canvas;
            }

            public override void Execute(object parameter)
            {
                var selectionTool = (SelectionTool)this.Tool;
                selectionTool.IsActive = !selectionTool.IsActive;

                var panTool = _canvas.Tools.OfType<PanTool>().FirstOrDefault();
                if (panTool != null)
                    panTool.IsActive = !selectionTool.IsActive;

                Name = selectionTool.IsActive ? "Pan" : "Select";

                // also reset selection triangle
                if (!selectionTool.IsActive)
                    selectionTool._selectionRectangle = null;

                _canvas.FireToolCommandsChanged();
            }

            public override bool CanExecute(object parameter)
            {
                return true;
            }
        }
    }
}

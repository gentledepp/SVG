using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;

namespace Svg.Editor.Tools
{
    public class ArrangeTool : UndoableToolBase
    {
        public ArrangeTool(IUndoRedoService undoRedoService) : base("Arrange", undoRedoService)
        {
            IconName = "ic_swap_vert.svg";
        }

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Bring to front", o =>
                {
                    var children = Canvas.Document.Children;
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Bring to front operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    foreach (var element in Canvas.SelectedElements.OrderByDescending(x => children.IndexOf(x)))
                    {
                        var index = children.IndexOf(element);
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Bring to front", o1 =>
                        {
                            for (var i = index; i < children.Count - 1; i++)
                            {
                                children[i] = children[i + 1];
                                children[i + 1] = element;
                            }
                        }, o1 =>
                        {
                            for (var i = children.IndexOf(element); i > index; i--)
                            {
                                children[i] = children[i - 1];
                                children[i - 1] = element;
                            }
                        }), hasOwnUndoRedoScope: false);
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_flip_to_front.svg", description: LocalizationService.GetString("Svg.Editor.ArrangeTool.BringToFront.Description")),
                new ToolCommand(this, "Bring forward", o =>
                {
                    var children = Canvas.Document.Children;
                    var selected = Canvas.SelectedElements.OrderByDescending(x => children.IndexOf(x));
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move forward operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    foreach (var element in selected)
                    {
                        var cIndex = children.IndexOf(element);
                        var successor = children.ElementAtOrDefault(cIndex + 1);
                        if (successor != null && !selected.Contains(successor) &&
                            !successor.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey))
                        {
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Move forward", o1 =>
                            {
                                children[cIndex] = successor;
                                children[cIndex + 1] = element;
                            }, o1 =>
                            {
                                children[cIndex] = element;
                                children[cIndex + 1] = successor;
                            }), hasOwnUndoRedoScope: false);
                        }
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_arrow_upward.svg", description: LocalizationService.GetString("Svg.Editor.ArrangeTool.MoveForward.Description")),
                new ToolCommand(this, "Send backward", o =>
                {
                    var children = Canvas.Document.Children;
                    var selected = Canvas.SelectedElements.OrderBy(x => children.IndexOf(x));
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Send backward operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    foreach (var element in selected)
                    {
                        var cIndex = children.IndexOf(element);
                        var precursor = children.ElementAtOrDefault(cIndex - 1);
                        if (precursor != null && !selected.Contains(precursor) && !precursor.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey))
                        {
                            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Send backward", o1 =>
                            {
                                children[cIndex] = precursor;
                                children[cIndex - 1] = element;
                            }, o1 =>
                            {
                                children[cIndex] = element;
                                children[cIndex - 1] = precursor;
                            }), hasOwnUndoRedoScope: false);
                        }
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_arrow_downward.svg", description: LocalizationService.GetString("Svg.Editor.ArrangeTool.SendBackward.Description")),
                new ToolCommand(this, "Send to back", o =>
                {
                    var children = Canvas.Document.Children;
                    var selected = Canvas.SelectedElements.OrderBy(x => children.IndexOf(x));
                    UndoRedoService.ExecuteCommand(new UndoableActionCommand("Send to back operation", o1 => Canvas.FireInvalidateCanvas(), o1 => Canvas.FireInvalidateCanvas()));
                    foreach (var element in selected)
                    {
                        var index = children.IndexOf(element);
                        UndoRedoService.ExecuteCommand(new UndoableActionCommand("Send to back", o1 =>
                        {
                            var firstArrangeableIndex =
                                children.IndexOf(
                                    children.First(
                                        x => x is SvgVisualElement && !x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)));
                            for (var i = index; i > firstArrangeableIndex; i--)
                            {
                                children[i] = children[i - 1];
                                children[i - 1] = element;
                            }
                        }, o1 =>
                        {
                            for (var i = children.IndexOf(element); i < index; i++)
                            {
                                children[i] = children[i + 1];
                                children[i + 1] = element;
                            }
                        }), hasOwnUndoRedoScope: false);
                    }
                }, o => Canvas.SelectedElements.Any(), iconName: "ic_flip_to_back.svg", description: LocalizationService.GetString("Svg.Editor.ArrangeTool.FlipToBack.Description"))
            };
        }
    }
}

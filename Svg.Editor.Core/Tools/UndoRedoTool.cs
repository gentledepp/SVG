using System.Collections.Generic;
using System.Threading.Tasks;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public class UndoRedoTool : UndoableToolBase
    {
        public UndoRedoTool(IUndoRedoService undoRedoService) : base("Undo/Redo", undoRedoService)
        {
            IconName = "ic_undo.svg";
        }

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);
            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Undo", o => UndoRedoService.Undo(), o => UndoRedoService.CanUndo(), iconName: "ic_undo.svg", description: LocalizationService.GetString("Svg.Editor.Strokes.Undo")),
                new ToolCommand(this, "Redo", o => UndoRedoService.Redo(), o => UndoRedoService.CanRedo(), iconName: "ic_redo.svg", description: LocalizationService.GetString("Svg.Editor.Strokes.Redo")),
            };
        }

        public override void OnDocumentChanged(SvgDocument oldDocument, SvgDocument newDocument)
        {
            UndoRedoService.Clear();
        }
    }
}
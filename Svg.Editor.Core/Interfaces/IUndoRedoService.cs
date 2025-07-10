using System;
using Svg.Editor.UndoRedo;

namespace Svg.Editor.Interfaces
{
    public interface IUndoRedoService
    {
        bool CanRedo();
        bool CanUndo();
        //void ExecuteCommand(IUndoableCommand command, object state = null);
        void ExecuteCommand(IUndoableCommand command, object state = null, bool hasOwnUndoRedoScope = true);
        void Redo();
        void Undo();
        void Clear();
        //void RemoveCommand(IUndoableCommand command);
        //void AddCommand(IUndoableCommand command);
        //IEnumerable<IUndoableCommand> UndoStack { get; }
        //IEnumerable<IUndoableCommand> RedoStack { get; }
        //int RedoBufferSize { get; set; }
        int UndoStackCapacity { get; set; }
        event EventHandler CanRedoChanged;
        event EventHandler CanUndoChanged;

        /// <summary>
        /// Occurs when either a do or an undo command is executed.
        /// </summary>
        event EventHandler<CommandEventArgs> ActionExecuted;
    }
}
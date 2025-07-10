using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Svg;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;

[assembly: SvgService(typeof(IUndoRedoService), typeof(UndoRedoService))]
namespace Svg.Editor.UndoRedo
{
    public class UndoRedoService : IUndoRedoService
    {
        private Stack<ExecutionStackEntry> ExecutionStack { get; } = new Stack<ExecutionStackEntry>();
        private List<IUndoableCommand> UndoStack { get; } = new List<IUndoableCommand>();
        private Stack<IUndoableCommand> RedoStack { get; } = new Stack<IUndoableCommand>();

        public int UndoStackCapacity { get; set; } = 10;

        /// <summary>
        /// Occurs when either a do or an undo command is executed.
        /// </summary>
        public event EventHandler<CommandEventArgs> ActionExecuted;

        /// <inheritdoc />	
        public bool IsActive
        {
            get;
            private set;
        }

        public bool IsUndoing { get; private set; }

        /// <summary>
        /// Determines whether this instance can redo.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can redo; otherwise, <c>false</c>.
        /// </returns>
        public bool CanRedo() => RedoStack.Count > 0;

        /// <summary>
        /// Determines whether this instance can undo.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can undo; otherwise, <c>false</c>.
        /// </returns>
        public bool CanUndo() => UndoStack.Count > 0;

        private class ExecutionStackEntry
        {
            public ExecutionStackEntry(ICommand command)
            {
                Command = command;
            }
            public ICommand Command { get; }
            public bool Cancel { get; set; }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="state">The state.</param>
        /// <param name="hasOwnUndoRedoScope"></param>
        public void ExecuteCommand(IUndoableCommand command, object state = null, bool hasOwnUndoRedoScope = true)
        {
            if (command == null || IsActive)
            {
                return;
            }
            // we process first, so another command that might be triggered by this one
            // can be added to this commands undo-redo scope
            ProcessCommand(command, hasOwnUndoRedoScope);

            ExecutionStack.Push(new ExecutionStackEntry(command));

            command.Execute(state);

            //Debug.WriteLine($"Command executed: {command.Name}");

            var entry = ExecutionStack.Pop();

            if (entry.Command != command)
                throw new InvalidOperationException("executionstack is out of sync!!");

            if (entry.Cancel)
            {
                //entry.Command.Undo(state);
                Undo(state);
                RedoStack.Pop();
                if (!RedoStack.Any()) CanRedoChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ProcessCommand(IUndoableCommand command, bool hasOwnUndoRedoScope)
        {
            var args = new CommandEventArgs(command, ExecuteAction.Execute);
            if (hasOwnUndoRedoScope || UndoStack.Count == 0)
            {
                UndoStack.Add(command);
                EnsureUndoCapacity();
                if (UndoStack.Count == 1) CanUndoChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // add to previous undostack entry 
                var cmd = UndoStack.Last();
                UndoStack.RemoveAt(UndoStack.Count - 1);
                if (!UndoStack.Any()) CanUndoChanged?.Invoke(this, EventArgs.Empty);

                var compositeCommand = cmd as CompositeCommand ?? new CompositeCommand(cmd);
                compositeCommand.AddCommand(command);

                UndoStack.Add(compositeCommand);
                EnsureUndoCapacity();
                if (UndoStack.Count == 1) CanUndoChanged?.Invoke(this, EventArgs.Empty);
            }

            var redoStackAny = RedoStack.Any();
            RedoStack.Clear();
            if (redoStackAny) CanRedoChanged?.Invoke(this, EventArgs.Empty);
            OnActionExecuted(args);
        }

        private void EnsureUndoCapacity()
        {
            while (UndoStack.Count > UndoStackCapacity)
                UndoStack.RemoveAt(0);
        }

        /// <summary>
        /// Redoes this instance.
        /// </summary>
        public virtual void Redo()
        {
            if (!CanRedo()) return;

            RedoInternal();
        }

        public void Undo()
        {
            Undo(null);
        }

        /// <summary>
        /// Undoes this instance.
        /// </summary>
        public virtual void Undo(object state)
        {
            if (!CanUndo()) return;

            IsUndoing = true;
            UndoInternal(state);
            IsUndoing = false;
        }

        /// <summary>
        /// Clears the undo and redo stacks.
        /// </summary>
        public virtual void Clear()
        {
            var undoStackAny = RedoStack.Any();
            UndoStack.Clear();
            if (undoStackAny) CanUndoChanged?.Invoke(this, EventArgs.Empty);
            var redoStackAny = RedoStack.Any();
            RedoStack.Clear();
            if (redoStackAny) CanRedoChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds the given command without executing it.
        /// </summary>
        /// <param name="command">The command to add.</param>
        public void AddCommand(IUndoableCommand command)
        {
            UndoStack.Add(command);
            EnsureUndoCapacity();
            if (UndoStack.Count == 1) CanUndoChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Removes the command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void RemoveCommand(IUndoableCommand command)
        {
            throw new NotImplementedException();
            //RedoStack.Remove(command);
            //UndoStack.Remove(command);
        }

        /// <summary>
        /// Raises the <see cref="ActionExecuted" /> event.
        /// </summary>
        /// <param name="args">The <see cref="CommandEventArgs" /> instance containing the event data.</param>
        protected virtual void OnActionExecuted(CommandEventArgs args)
        {
            ActionExecuted?.Invoke(this, args);
        }

        private void RedoInternal()
        {
            IsActive = true;
            var command = RedoStack.Pop();
            if (!RedoStack.Any()) CanRedoChanged?.Invoke(this, EventArgs.Empty);
            var args = new CommandEventArgs(command, ExecuteAction.Redo);
            command.Redo();
            UndoStack.Add(command);
            if (UndoStack.Count == 1) CanUndoChanged?.Invoke(this, EventArgs.Empty);
            OnActionExecuted(args);
            IsActive = false;
        }

        private void UndoInternal(object state)
        {
            IsActive = true;
            var command = UndoStack.Last();
            UndoStack.RemoveAt(UndoStack.Count - 1);
            if (!UndoStack.Any()) CanUndoChanged?.Invoke(this, EventArgs.Empty);
            var args = new CommandEventArgs(command, ExecuteAction.Undo);
            command.Undo(state);
            RedoStack.Push(command);
            if (RedoStack.Count == 1) CanRedoChanged?.Invoke(this, EventArgs.Empty);
            OnActionExecuted(args);
            IsActive = false;
        }

        public event EventHandler CanRedoChanged;
        public event EventHandler CanUndoChanged;

        public void CancelCurrentOperation()
        {
            if (ExecutionStack.Count == 0)
                return;

            ExecutionStack.Peek().Cancel = true;
        }

        public bool IsCanceling => ExecutionStack.Count != 0 && ExecutionStack.Peek().Cancel;
    }

    public sealed class CommandEventArgs : GenericEventArgs<ICommand>
    {
        public CommandEventArgs(ICommand entity, ExecuteAction action) : base(entity)
        {
            ExecuteAction = action;
        }

        public ExecuteAction ExecuteAction { get; }
    }

    public class GenericEventArgs<T> : EventArgs
    {
        public GenericEventArgs()
        {

        }

        public GenericEventArgs(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; set; }
    }

    public enum ExecuteAction
    {
        Execute = 0,
        Undo = 1,
        Redo = 2
    }
}

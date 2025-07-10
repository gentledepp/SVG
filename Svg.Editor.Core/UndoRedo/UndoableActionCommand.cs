using System;
using System.Diagnostics;
using Svg.Editor.Interfaces;

namespace Svg.Editor.UndoRedo
{
    /// <summary>
    /// Base class for undoable actions based on delegates.
    /// </summary>
    [DebuggerDisplay("Name:{Name}")]
    public class UndoableActionCommand : IUndoableCommand
    {
        private readonly Action<object> _undo;
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;
        private object _commandState;

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoableActionCommand" /> class.
        /// </summary>
        /// <param name="name">The name or title of the command.</param>
        /// <param name="execute">The redo or execute method.</param>
        /// <param name="undo">The undo or rollback method.</param>
        /// <param name="canExecute">The method returning whether the command can be executed.</param>
        public UndoableActionCommand(string name, Action<object> execute, Action<object> undo = null, Predicate<object> canExecute = null)
        {
            Name = name;
            _execute = execute;
            _canExecute = canExecute;
            _undo = undo;
        }

        /// <summary>
        /// Unwinds an undoable action.
        /// </summary>
        public virtual void Undo(object state = null)
        {
            _undo?.Invoke(state);
        }

        /// <summary>
        /// Executes an undoable action.
        /// </summary>
        public virtual void Redo()
        {
            Execute(_commandState);
        }

        /// <summary>
        /// Executes an undoable action.
        /// </summary>
        /// <param name="state"></param>
        public virtual void Execute(object state = null)
        {
            if (!CanExecute(state))
            {
                return;
            }
            if (_execute != null)
            {
                _execute(state);
                _commandState = state;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this command can be executed.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public virtual bool CanExecute(object state = null)
        {
            return _canExecute == null || _canExecute(state);
        }
    }
}

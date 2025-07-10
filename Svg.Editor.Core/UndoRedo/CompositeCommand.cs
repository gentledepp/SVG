using System;
using System.Collections.Generic;
using System.Linq;
using Svg.Editor.Interfaces;

namespace Svg.Editor.UndoRedo
{
    public class CompositeCommand : UndoableActionCommand
    {
        private readonly List<IUndoableCommand> _commands = new List<IUndoableCommand>();

        public event EventHandler CommandStarted;
        public event EventHandler CommandEnded;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Telerik.Windows.Diagrams.Core.CompositeCommand" /> class.
        /// </summary>
        /// <param name="name">The name or title of the composite action.</param>
        /// <param name="execute">The execute action.</param>
        /// <param name="undo">The undo action.</param>
        /// <param name="canExecute">The CanExecute action.</param>
        public CompositeCommand(string name, Action<object> execute = null, Action<object> undo = null, Predicate<object> canExecute = null)
            : base(name, execute, undo, canExecute)
        {
        }

        public CompositeCommand(IUndoableCommand command) : this(command.Name)
        {
            _commands.Add(command);
        }

        /// <summary>
        /// Executes and undoable action.
        /// </summary>
        public override void Redo()
        {
            CommandStarted?.Invoke(this, EventArgs.Empty);

            foreach (var t in _commands)
                t.Redo();

            CommandEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Executes the specified state.
        /// </summary>
        public override void Execute(object state = null)
        {
            CommandStarted?.Invoke(this, EventArgs.Empty);

            if (CanExecute(state))
            {
                foreach (var t in _commands)
                {
                    t.Execute(state);
                }
            }
            base.Execute(state);

            CommandEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Unwinds an undoable action.
        /// </summary>
        public override void Undo(object state = null)
        {
            CommandStarted?.Invoke(this, EventArgs.Empty);

            for (var i = _commands.Count - 1; i >= 0; i--)
                _commands[i].Undo(state);

            base.Undo(state);

            CommandEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns boolean value indicating whether this command can be executed.
        /// </summary>
        public override bool CanExecute(object state = null)
        {
            return _commands.All(c => c.CanExecute(state));
        }

        public CompositeCommand AddCommand(IUndoableCommand command)
        {
            if (command != null)
                _commands.Add(command);
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Tools
{
    public abstract class UndoableToolBase : ToolBase
    {
        protected UndoableToolBase(string name, IUndoRedoService undoRedoService) : this(name, null, undoRedoService) { }

        protected UndoableToolBase(string name, IDictionary<string,object> properties, IUndoRedoService undoRedoService) : base(name, properties)
        {
            UndoRedoService = undoRedoService ?? throw new ArgumentNullException(nameof(undoRedoService));
        }

        protected IUndoRedoService UndoRedoService { get; }
    }
}

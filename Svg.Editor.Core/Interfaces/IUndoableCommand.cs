using System.Windows.Input;

namespace Svg.Editor.Interfaces
{
    public interface IUndoableCommand : ICommand
    {
        void Undo(object state);
        void Redo();
        string Name { get; set; }
    }
}

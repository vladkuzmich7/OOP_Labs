
namespace DocMaster.Command
{
    public class CommandHistory
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public void Push(ICommand command) => _undoStack.Push(command);

        public (int newPosition, bool success) Undo(int currentPosition)
        {
            if (_undoStack.Count == 0) return (currentPosition, false);

            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);

            return (cmd.CursorPositionBefore, true);
        }

        public (int newPosition, bool success) Redo(int currentPosition)
        {
            if (_redoStack.Count == 0) return (currentPosition, false);

            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);

            return (cmd.CursorPositionAfter, true);
        }
    }
}

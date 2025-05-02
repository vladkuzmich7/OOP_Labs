using System.Collections.Generic;

namespace OOPsl.DocumentFunctions.Commands
{
    public class CommandManager
    {
        private Stack<ICommand> executedCommands = new Stack<ICommand>();
        private Stack<ICommand> undoneCommands = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            executedCommands.Push(command);
            undoneCommands.Clear();
        }

        public string Undo()
        {
            if (executedCommands.Count > 0)
            {
                ICommand command = executedCommands.Pop();
                command.UnExecute();
                undoneCommands.Push(command);
                return command.UpdatedText;
            }
            return null;
        }

        public string Redo()
        {
            if (undoneCommands.Count > 0)
            {
                ICommand command = undoneCommands.Pop();
                command.Execute();
                executedCommands.Push(command);
                return command.UpdatedText;
            }
            return null;
        }

        public void ClearHistory()
        {
            executedCommands.Clear();
            undoneCommands.Clear();
        }
    }
}
namespace OOPsl.DocumentFunctions.Commands
{
    public class DeleteCommand(int index, int length, string currentText) 
        : ICommand
    {
        public string UpdatedText { get; private set; }

        public void Execute()
        {
            UpdatedText = currentText.Remove(index, length);
        }
        public void UnExecute()
        {
            UpdatedText = currentText;
        }
    }
}

namespace OOPsl.DocumentFunctions.Commands
{
    public class InsertCommand : ICommand
    {
        private readonly int index;
        private readonly string insertedText;
        private string currentText;

        public string UpdatedText { get; private set; }

        public InsertCommand(int index, string insertedText, string currentText)
        {
            this.index = index;
            this.insertedText = insertedText;
            this.currentText = currentText;
        }

        public void Execute()
        {
            UpdatedText = currentText.Insert(index, insertedText);
            currentText = UpdatedText; 
        }

        public void UnExecute()
        {
            UpdatedText = currentText.Remove(index, insertedText.Length);
            currentText = UpdatedText; 
        }
    }
}
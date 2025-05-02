namespace OOPsl.DocumentFunctions.Commands
{
    public class InsertCommand : ICommand
    {
        private int index;
        private string insertedText;
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
        }
        public void UnExecute()
        {
            UpdatedText = currentText;
        }
    }
}

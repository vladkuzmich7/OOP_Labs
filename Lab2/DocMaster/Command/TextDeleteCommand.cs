using DocMaster.Services;
using DocMaster.Models;

namespace DocMaster.Command
{
    //////////////////////////// Deleting text from a doc ////////////////////////////
    public class TextDeleteCommand : ICommand
    {
        private readonly Document _document;
        private readonly int _position;
        private readonly string _deletedText;
        private readonly string _editedBy;

        public int CursorPositionBefore { get; }
        public int CursorPositionAfter { get; }

        public TextDeleteCommand(Document doc, int pos, int length, int cursorBefore, string editedBy)
        {
            pos = Math.Clamp(pos, 0, doc.Content.Length);
            length = Math.Clamp(length, 0, doc.Content.Length - pos);

            _document = doc;
            _position = pos;
            _deletedText = doc.Content.Substring(pos, length);
            CursorPositionBefore = cursorBefore;
            CursorPositionAfter = pos;
            _editedBy = editedBy;
        }

        public void Execute()
        {
            if (_position + _deletedText.Length <= _document.Content.Length)
            {
                _document.Content = _document.Content.Remove(_position, _deletedText.Length);
            }
            _document.NotifyChange(_editedBy);
            DocumentHistoryService.AddRecord(_document, _editedBy, "deleted the text");
        }

        public void Undo()
        {
            _document.Content = _document.Content.Insert(_position, _deletedText);
        }
    }
}

using DocMaster.Services;
using DocMaster.Models;

namespace DocMaster.Command
{
    //////////////////////////// Adding text to a doc ////////////////////////////
    public class TextInsertCommand : ICommand
    {
        private readonly Document _document;
        private readonly int _position;
        private readonly string _text;
        private readonly string _editedBy;

        public int CursorPositionBefore { get; }
        public int CursorPositionAfter => _position + _text.Length;
        public TextInsertCommand(Document doc, int pos, string text, int cursorBefore, string editedBy)
        {
            _document = doc;
            _position = pos;
            _text = text;
            CursorPositionBefore = cursorBefore;
            _editedBy = editedBy;
        }

        public void Execute()
        {
            _document.Content = _document.Content.Insert(_position, _text);
            _document.NotifyChange(_editedBy);
            DocumentHistoryService.AddRecord(_document, _editedBy, "changed the text");
        }

        public void Undo()
        {
            _document.Content = _document.Content.Remove(_position, _text.Length);
        }
    }
}

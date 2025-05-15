using DocMaster.Services;
using DocMaster.Models;

namespace DocMaster.Command
{
    public class TextReplaceCommand : ICommand
    {
        private readonly Document _document;
        private readonly int _start;
        private readonly int _end;
        private readonly string _newText;
        private readonly string _oldText;
        private readonly string _editedBy;


        public int CursorPositionBefore { get; }
        public int CursorPositionAfter { get; }

        public TextReplaceCommand(Document doc, int start, int end, string newText, int cursorBefore, string editedBy)
        {
            _document = doc;
            _start = start;
            _end = end;
            _newText = newText;
            _oldText = doc.Content.Substring(start, end - start);
            CursorPositionBefore = cursorBefore;
            CursorPositionAfter = start + newText.Length;
            _editedBy = editedBy;
        }

        public void Execute()
        {
            _document.Content = _document.Content
                .Remove(_start, _end - _start)
                .Insert(_start, _newText);
            //_document.NotifyChange(_editedBy);
            DocumentHistoryService.AddRecord(_document, _editedBy, "replaced the text");
        }

        public void Undo()
        {
            _document.Content = _document.Content
                .Remove(_start, _newText.Length)
                .Insert(_start, _oldText);
        }
    }
}

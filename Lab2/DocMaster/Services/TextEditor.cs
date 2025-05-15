using DocMaster.Models;
using DocMaster.Command;
using DocMaster.Roles.Observers;
using DocMaster.Services;

namespace DocMaster.Services
{
    public class TextEditor
    {
        private Document _document;
        private int _cursorPosition;
        private int _selectionStart = -1;
        private readonly CommandHistory _history = new();

        private bool _isPreviewMode = false;
        private DocumentFormat _format;

        private readonly User _currentUser;

        private ITextComponent _formattedDocument;

        public TextEditor(Document doc, User currentUser)
        {
            _document = doc;
            _formattedDocument = doc;
            _format = doc.Format;
            _cursorPosition = doc.Content.Length;
            _currentUser = currentUser;
        }

        public void StartEditing()
        {
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += (s, e) => e.Cancel = true;
            Console.CursorVisible = false;
            ConsoleKeyInfo key;
            do
            {
                RenderText();
                key = Console.ReadKey(true);
                HandleKeyPress(key);
            } while (key.Key != ConsoleKey.Escape);

            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress -= (s, e) => e.Cancel = true;
            Console.CursorVisible = true;
        }
        private void UpdateSelection(bool isSelecting)
        {
            if (isSelecting)
            {
                if (_selectionStart == -1)
                    _selectionStart = _cursorPosition;
            }
            else
            {
                _selectionStart = -1;
            }
        }
        private void CopyToSystemClipboard()
        {
            if (_selectionStart == -1 || _selectionStart == _cursorPosition) return;

            int start = Math.Min(_selectionStart, _cursorPosition);
            int end = Math.Max(_selectionStart, _cursorPosition);
            string selectedText = _document.Content.Substring(start, end - start);

            try
            {
                TextCopy.ClipboardService.SetText(selectedText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка буфера: {ex.Message}");
            }
        }
        private void HandleKeyPress(ConsoleKeyInfo key)
        {
            if (_format == DocumentFormat.Markdown && !_isPreviewMode)
            {
                HandleMarkdownShortcuts(key);
            }
            if (key.Key == ConsoleKey.F12)
            {
                TogglePreviewMode();
                return;
            }

            if (_isPreviewMode) return;
            bool isShiftPressed = (key.Modifiers & ConsoleModifiers.Shift) != 0;

            // Control + combinations
            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
            {
                HandleControlCombination(key.Key);
                UpdateSelection(isShiftPressed);
                return;
            }


            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    InsertText(Environment.NewLine);
                    break;
                case ConsoleKey.Tab:
                    InsertText("    ");
                    break;
                case ConsoleKey.LeftArrow:
                    _cursorPosition = Math.Max(0, _cursorPosition - 1);
                    break;
                case ConsoleKey.RightArrow:
                    _cursorPosition = Math.Min(_document.Content.Length, _cursorPosition + 1);
                    break;
                case ConsoleKey.Backspace when _cursorPosition > 0:
                    DeleteChar(_cursorPosition - 1);
                    break;
                case ConsoleKey.Delete when _cursorPosition < _document.Content.Length:
                    DeleteChar(_cursorPosition);
                    break;
                case ConsoleKey.Home:
                    _cursorPosition = 0;
                    break;
                case ConsoleKey.End:
                    _cursorPosition = _document.Content.Length;
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                        InsertChar(key.KeyChar);
                    break;
            }

            UpdateSelection(isShiftPressed);
        }

        private void HandleControlCombination(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Z:
                    var (undoPos, undoSuccess) = _history.Undo(_cursorPosition);
                    if (undoSuccess)
                    {
                        _cursorPosition = undoPos;
                        _cursorPosition = Math.Clamp(_cursorPosition, 0, _document.Content.Length);
                    }
                    break;

                case ConsoleKey.Y:
                    var (redoPos, redoSuccess) = _history.Redo(_cursorPosition);
                    if (redoSuccess)
                    {
                        _cursorPosition = redoPos;
                        _cursorPosition = Math.Clamp(_cursorPosition, 0, _document.Content.Length);
                    }
                    break;
                case ConsoleKey.C:
                    CopySelection();
                    break;

                case ConsoleKey.V:
                    PasteText();
                    break;

                case ConsoleKey.X:
                    CutSelection();
                    break;
            }
        }
        private void CutSelection()
        {
            if (_selectionStart == -1 || _selectionStart == _cursorPosition)
                return;

            int start = Math.Min(_selectionStart, _cursorPosition);
            int end = Math.Max(_selectionStart, _cursorPosition);
            int length = end - start;

            CopySelection();

            var cmd = new TextDeleteCommand(
                doc: _document,
                pos: start,
                length: length,
                cursorBefore: _cursorPosition,
                editedBy: _currentUser.Username
            );

            cmd.Execute();
            _history.Push(cmd);

            _cursorPosition = start;
            _selectionStart = -1;

            RenderText();
        }
        private void InsertChar(char c)
        {
            var before = _cursorPosition;
            var cmd = new TextInsertCommand(_document, _cursorPosition, c.ToString(), before, _currentUser.Username);
            cmd.Execute();
            _history.Push(cmd);
            _cursorPosition = cmd.CursorPositionAfter;
        }

        private void DeleteChar(int pos)
        {
            var before = _cursorPosition;
            var cmd = new TextDeleteCommand(_document, pos, 1, before, _currentUser.Username);
            cmd.Execute();
            _history.Push(cmd);
            _cursorPosition = cmd.CursorPositionAfter;
        }
        private void InsertText(string text)
        {
            text = text.Replace(Environment.NewLine, "\n");

            int originalPosition = _cursorPosition;

            var cmd = new TextInsertCommand(_document, _cursorPosition, text, originalPosition, _currentUser.Username);
            cmd.Execute();
            _history.Push(cmd);

            _cursorPosition += text.Length;

            if (text.Contains('\n'))
            {
                int newLineIndex = _document.Content.IndexOf('\n', originalPosition);

                if (newLineIndex != -1)
                {
                    _cursorPosition = newLineIndex + 1;
                }
                else
                {
                    _cursorPosition = _document.Content.Length;
                }
            }

            _cursorPosition = Math.Clamp(_cursorPosition, 0, _document.Content.Length);
        }
        private void CopySelection()
        {
            if (_selectionStart == -1 || _selectionStart == _cursorPosition)
                return;

            int start = Math.Min(_selectionStart, _cursorPosition);
            int end = Math.Max(_selectionStart, _cursorPosition);
            string selectedText = _document.Content.Substring(start, end - start);

            try
            {
                TextCopy.ClipboardService.SetText(selectedText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка буфера: {ex.Message}");
            }
            RenderText();
        }

        private void PasteText()
        {
            var text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text)) return;

            var cmd = new TextInsertCommand(
                doc: _document,
                pos: _cursorPosition,
                text: text,
                cursorBefore: _cursorPosition,
                editedBy: _currentUser.Username
            );

            cmd.Execute();
            _history.Push(cmd);

            _cursorPosition = cmd.CursorPositionAfter;

            _cursorPosition = Math.Clamp(_cursorPosition, 0, _document.Content.Length);
        }
        private void HandleMarkdownShortcuts(ConsoleKeyInfo key)
        {
            if ((key.Modifiers & ConsoleModifiers.Control) != 0)
            {
                int start = Math.Min(_selectionStart, _cursorPosition);
                int end = Math.Max(_selectionStart, _cursorPosition);

                if (key.Key == ConsoleKey.B && start != end)
                {
                    ApplyMarkdownFormatting(start, end, "**");
                }
                else if (key.Key == ConsoleKey.I && start != end)
                {
                    ApplyMarkdownFormatting(start, end, "*");
                }
                else if (key.Key == ConsoleKey.U && start != end)
                {
                    ApplyMarkdownFormatting(start, end, "~~");
                }
            }
            else if (key.Key == ConsoleKey.F12)
            {
                TogglePreviewMode();
            }
        }

        private void ApplyMarkdownFormatting(int start, int end, string wrapper)
        {
            string selectedText = _document.Content.Substring(start + 1, end - start - 1);
            string newText = $"{wrapper}{selectedText}{wrapper}";

            var cmd = new TextReplaceCommand(
                doc: _document,
                start: start,
                end: end,
                newText: newText,
                cursorBefore: _cursorPosition,
                editedBy: _currentUser.Username
            );

            cmd.Execute();
            _history.Push(cmd);
            _cursorPosition = end + wrapper.Length * 2;
            _selectionStart = -1;
        }

        private void TogglePreviewMode()
        {
            _isPreviewMode = !_isPreviewMode;
            RenderText();
        }
        private void RenderText()
        {
            if (_isPreviewMode && _format == DocumentFormat.Markdown)
            {
                RenderMarkdownPreview();
            }
            else
            {
                RenderNormalText();
            }
        }
        private void RenderMarkdownPreview()
        {
            Console.Clear();
            Console.WriteLine("=== Preview Mode (F12 to exit) ===");

            string content = _document.Content;
            int pos = 0;

            while (pos < content.Length)
            {
                if (pos < content.Length - 1 && content[pos] == '*' && content[pos + 1] == '*')
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    pos = ProcessBold(content, pos);
                }
                else if (content[pos] == '*')
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    pos = ProcessItalic(content, pos);
                }
                else if (content[pos] == '~' && content[pos + 1] == '~')
                {

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    pos = ProcessUnderline(content, pos);
                }
                else
                {
                    Console.ResetColor();
                    Console.Write(content[pos]);
                    pos++;
                }
            }
        }

        private int ProcessBold(string content, int pos)
        {
            pos += 2;
            while (pos < content.Length + 1 && !(content[pos] == '*' && content[pos + 1] == '*'))
            {
                Console.Write(content[pos]);
                pos++;
            }
            pos += 2;
            //Console.ResetColor();
            return pos;
        }

        private int ProcessUnderline(string content, int pos)
        {
            pos += 2;
            while (pos < content.Length + 1 && !(content[pos] == '~' && content[pos + 1] == '~'))
            {
                Console.Write(content[pos]);
                pos++;
            }
            pos += 2;
            //Console.ResetColor();
            return pos;
        }

        private int ProcessItalic(string content, int pos)
        {
            pos++;
            while (pos < content.Length && content[pos] != '*')
            {
                Console.Write(content[pos]);
                pos++;
            }
            pos++;
            //Console.ResetColor();
            return pos;
        }

        private int ProcessToEndOfLine(string content, int pos)
        {
            // Пропускаем пробелы после #
            while (pos < content.Length && char.IsWhiteSpace(content[pos]))
                pos++;

            // Запоминаем начало строки
            int lineStart = pos;

            // Ищем конец строки
            while (pos < content.Length && content[pos] != '\n')
                pos++;

            // Выводим всю строку
            if (lineStart < pos)
            {
                Console.Write(content.Substring(lineStart, pos - lineStart));
            }

            // Добавляем перенос строки
            Console.WriteLine();

            // Пропускаем символ \n если он есть
            if (pos < content.Length && content[pos] == '\n')
                pos++;

            return pos;
        }
        private void RenderNormalText()
        {
            Console.Clear();
            Console.WriteLine("=== Editing Mode (ESC to exit) ===");
            Console.Write(_document.Content);

            int consoleLine = 2;
            int consoleColumn = 0;
            int contentPos = 0;

            foreach (char c in _document.Content)
            {
                if (contentPos >= _cursorPosition) break;

                if (c == '\n')
                {
                    consoleLine++;
                    consoleColumn = 0;
                }
                else
                {
                    consoleColumn++;
                    if (consoleColumn >= Console.WindowWidth)
                    {
                        consoleLine++;
                        consoleColumn = 0;
                    }
                }
                contentPos++;
            }

            Console.SetCursorPosition(consoleColumn, consoleLine);

            Console.Write(_cursorPosition < _document.Content.Length
                ? _document.Content[_cursorPosition]
                : ' ');

            Console.SetCursorPosition(consoleColumn, consoleLine);
        }
    }

}
public static class Clipboard
{
    public static string Content { get; private set; } = string.Empty;
    public static void SetText(string text)
    {
        try
        {
            // Use system buffer from TextCopy
            TextCopy.ClipboardService.SetText(text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clipboard error: {ex.Message}");
        }
    }

    public static string GetText()
    {
        try
        {
            return TextCopy.ClipboardService.GetText();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clipboard error: {ex.Message}");
            return string.Empty;
        }
    }
}
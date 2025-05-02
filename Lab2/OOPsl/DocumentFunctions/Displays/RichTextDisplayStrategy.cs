using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OOPsl.DocumentFunctions.Displays
{
    public class RichTextDisplayStrategy : IDisplayStrategy
    {
        private const string ResetAnsi = "\x1b[0m";
        private readonly Stack<string> _styleStack = new Stack<string>();
        private readonly Stack<string> _tagStack = new Stack<string>();
        private readonly List<string> _errors = new List<string>();

        public void Display(Document document)
        {
            string content = document.Content;
            var output = new StringBuilder();
            int i = 0;

            while (i < content.Length)
            {
                if (content[i] == '<')
                {
                    int tagEnd = content.IndexOf('>', i);
                    if (tagEnd == -1)
                    {
                        _errors.Add($"Незакрытый тег в позиции {i}");
                        break;
                    }

                    string tag = content.Substring(i, tagEnd - i + 1);
                    ProcessTag(tag, i, output);
                    i = tagEnd + 1;
                }
                else
                {
                    output.Append(GetCurrentStyle());
                    output.Append(content[i]);
                    i++;
                }
            }

            while (_tagStack.Count > 0)
            {
                _errors.Add($"Незакрытый тег <{_tagStack.Pop()}>");
            }

            if (_errors.Count == 0)
            {
                output.Append(ResetAnsi);
                Console.WriteLine(output.ToString());
            }
            else
            {
                Console.WriteLine("Обнаружены ошибки форматирования:");
                foreach (var error in _errors)
                {
                    Console.WriteLine($"• {error}");
                }
                Console.WriteLine("\nТекст не был отображен из-за ошибок.");
            }
        }

        private void ProcessTag(string tag, int position, StringBuilder output)
        {
            bool isClosing = tag.StartsWith("</");
            string tagName = Regex.Replace(tag, @"[^a-zA-Z]", "").ToLower();

            output.Append(ResetAnsi);

            if (isClosing)
            {
                HandleClosingTag(tagName, position);
            }
            else
            {
                HandleOpeningTag(tagName, position);
            }

            output.Append(GetCurrentStyle());
        }

        private void HandleOpeningTag(string tagName, int position)
        {
            if (IsValidTag(tagName))
            {
                _tagStack.Push(tagName);
                ApplyStyle(tagName);
            }
            else
            {
                _errors.Add($"Недопустимый тег <{tagName}> в позиции {position}");
            }
        }

        private void HandleClosingTag(string tagName, int position)
        {
            if (_tagStack.Count == 0)
            {
                _errors.Add($"Лишний закрывающий тег </{tagName}> в позиции {position}");
                return;
            }

            string expectedTag = _tagStack.Pop();
            if (expectedTag != tagName)
            {
                _errors.Add($"Несоответствие тегов: </{tagName}> вместо </{expectedTag}> в позиции {position}");
                _tagStack.Push(expectedTag);
            }
            else
            {
                RemoveStyle(tagName);
            }
        }

        private bool IsValidTag(string tag) => tag == "b" || tag == "i" || tag == "u";

        private void ApplyStyle(string tag)
        {
            switch (tag)
            {
                case "b": _styleStack.Push("\x1b[1m"); break;
                case "i": _styleStack.Push("\x1b[3m"); break;
                case "u": _styleStack.Push("\x1b[4m"); break;
            }
        }

        private void RemoveStyle(string tag)
        {
            var temp = new Stack<string>();
            bool removed = false;

            while (_styleStack.Count > 0)
            {
                string style = _styleStack.Pop();
                if (!removed && style == GetAnsiCode(tag))
                {
                    removed = true;
                    continue;
                }
                temp.Push(style);
            }

            while (temp.Count > 0) _styleStack.Push(temp.Pop());
        }

        private string GetAnsiCode(string tag) => tag switch
        {
            "b" => "\x1b[1m",
            "i" => "\x1b[3m",
            "u" => "\x1b[4m",
            _ => ""
        };

        private string GetCurrentStyle() =>
            _styleStack.Count > 0 ? string.Join("", _styleStack) : ResetAnsi;
    }
}
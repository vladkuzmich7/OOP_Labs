using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace OOPsl.DocumentFunctions.Displays
{
    public class XmlDisplayStrategy : IDisplayStrategy
    {
        private const string Reset = "\x1b[0m";
        private const string OtherTagColor = "\x1b[38;5;205m";
        private readonly Stack<string> _activeStyles = new Stack<string>();
        private readonly Stack<string> _tagStack = new Stack<string>();
        private readonly List<string> _errors = new List<string>();

        public void Display(Document document)
        {
            var content = document.Content;
            var output = new StringBuilder();
            int position = 0;
            bool inTag = false;
            var currentTag = new StringBuilder();

            while (position < content.Length)
            {
                if (content[position] == '<')
                {
                    inTag = true;
                    currentTag.Clear();
                }

                if (inTag)
                {
                    currentTag.Append(content[position]);

                    if (content[position] == '>')
                    {
                        inTag = false;
                        var tag = currentTag.ToString();

                        if (!ValidateTag(tag, position))
                        {
                            _errors.Add($"Неверный тег в позиции {position}");
                        }

                        ProcessTag(tag);

                        output.Append(IsStylableTag(tag) ? Reset : OtherTagColor);
                        output.Append(tag);
                        output.Append(GetCurrentStyle());
                    }
                }
                else
                {
                    output.Append(GetCurrentStyle());
                    output.Append(content[position]);
                }

                position++;
            }
            if (_tagStack.Count > 0)
            {
                _errors.Add($"Незакрытые теги: {string.Join(", ", _tagStack)}");
            }

            if (_errors.Any())
            {
                Console.WriteLine("Обнаружены ошибки формата:");
                foreach (var error in _errors)
                {
                    Console.WriteLine($"• {error}");
                }
            }
            else
            {
                Console.WriteLine(output.ToString());
            }

            Console.Write(Reset); 
        }

        private bool ValidateTag(string tag, int position)
        {
            if (tag.Length < 2) return false;
            if (tag[0] != '<') return false;
            if (tag[^1] != '>') return false;

            if (tag.StartsWith("</"))
            {
                var tagName = ParseTagName(tag.Substring(2));
                if (!_tagStack.Any() || _tagStack.Peek() != tagName)
                {
                    _errors.Add($"Несоответствующий закрывающий тег </{tagName}> в позиции {position}");
                    return false;
                }
                _tagStack.Pop();
                return true;
            }

            var name = ParseTagName(tag.Substring(1));
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            _tagStack.Push(name);
            return true;
        }

        private string ParseTagName(string tagPart)
        {
            tagPart = tagPart.Trim();
            int end = tagPart.IndexOfAny(new[] { '>', ' ', '\t', '/' });
            return end > 0
                ? tagPart.Substring(0, end).ToLower()
                : tagPart.Replace(">", "").ToLower();
        }

        private bool IsStylableTag(string tag)
        {
            var tagName = tag.ToLower();
            return tagName.StartsWith("<b") ||
                   tagName.StartsWith("<i") ||
                   tagName.StartsWith("<u") ||
                   tagName.StartsWith("</b") ||
                   tagName.StartsWith("</i") ||
                   tagName.StartsWith("</u");
        }

        private void ProcessTag(string tag)
        {
            if (tag.StartsWith("</"))
            {
                string tagName = ParseTagName(tag.Substring(2));
                RemoveStyle(tagName);
            }
            else if (tag.StartsWith("<"))
            {
                string tagName = ParseTagName(tag.Substring(1));
                ApplyStyle(tagName);
            }
        }

        private string GetCurrentStyle() =>
            _activeStyles.Count > 0 ? string.Join("", _activeStyles) : "";

        private void ApplyStyle(string tag)
        {
            string ansiCode = tag switch
            {
                "b" => "\x1b[1m",
                "i" => "\x1b[3m",
                "u" => "\x1b[4m",
                _ => ""
            };

            if (!string.IsNullOrEmpty(ansiCode))
                _activeStyles.Push(ansiCode);
        }

        private void RemoveStyle(string tag)
        {
            string target = tag switch
            {
                "b" => "\x1b[1m",
                "i" => "\x1b[3m",
                "u" => "\x1b[4m",
                _ => ""
            };

            var temp = new Stack<string>();
            while (_activeStyles.Count > 0)
            {
                var style = _activeStyles.Pop();
                if (style == target) break;
                temp.Push(style);
            }
            while (temp.Count > 0)
                _activeStyles.Push(temp.Pop());
        }
    }
}
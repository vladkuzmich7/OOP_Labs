using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OOPsl.DocumentFunctions.Displays
{
    public class StyledChar
    {
        public char Character { get; }
        public bool Bold { get; }
        public bool Italic { get; }
        public bool Underline { get; }

        public StyledChar(char ch, bool bold, bool italic, bool underline)
        {
            Character = ch;
            Bold = bold;
            Italic = italic;
            Underline = underline;
        }
    }

    public class MarkdownDisplayStrategy : IDisplayStrategy
    {
        private const string ResetAnsi = "\x1b[0m";

        public void Display(Document document)
        {
            string content = document.Content;
            var errors = ValidateMarkdown(content);
            if (errors.Count > 0)
            {
                Console.WriteLine("Ошибки форматирования:");
                errors.ForEach(e => Console.WriteLine($"• {e}"));
                Console.WriteLine("Контент не отображён.");
                return;
            }

            string formatted = ApplyFormatting(content);
            Console.WriteLine(formatted + ResetAnsi);
        }

        private List<string> ValidateMarkdown(string content)
        {
            var errors = new List<string>();
            var stack = new Stack<string>();
            var matches = Regex.Matches(content, @"(\*{3}|\*{2}|\*|<u>|</u>)");
            foreach (Match match in matches)
            {
                string token = match.Value;
                int position = match.Index;
                switch (token)
                {
                    case "<u>":
                        stack.Push("u");
                        break;
                    case "</u>":
                        if (stack.Count == 0 || stack.Pop() != "u")
                            errors.Add($"Непарный тег </u> на позиции {position}");
                        break;
                    default:
                        break;
                }
            }
            while (stack.Count > 0)
                errors.Add($"Незакрытый тег: {stack.Pop()}");

            bool currentBold = false;
            bool currentItalic = false;
            int i = 0;
            while (i < content.Length)
            {
                if (content[i] == '\\' && i + 1 < content.Length)
                {
                    i += 2;
                    continue;
                }
                if (i <= content.Length - 3 && content.Substring(i, 3) == "***")
                {
                    currentBold = !currentBold;
                    currentItalic = !currentItalic;
                    i += 3;
                    continue;
                }
                if (i <= content.Length - 2 && content.Substring(i, 2) == "**")
                {
                    currentBold = !currentBold;
                    i += 2;
                    continue;
                }
                if (content[i] == '*')
                {
                    currentItalic = !currentItalic;
                    i++;
                    continue;
                }
                if (i <= content.Length - 3 && content.Substring(i, 3) == "<u>")
                {
                    i += "<u>".Length;
                    continue;
                }
                if (i <= content.Length - 4 && content.Substring(i, 4) == "</u>")
                {
                    i += "</u>".Length;
                    continue;
                }
                i++;
            }

            if (currentBold)
                errors.Add("Некорректное использование символов '**' или '***': незакрытый жирный текст.");
            if (currentItalic)
                errors.Add("Некорректное использование символа '*': незакрытый курсивный текст.");

            return errors;
        }

        private string ApplyFormatting(string content)
        {
            var styledChars = new List<StyledChar>();

            bool currentBold = false;
            bool currentItalic = false;
            bool currentUnderline = false;
            int i = 0;
            while (i < content.Length)
            {
                if (content[i] == '\\' && i + 1 < content.Length)
                {
                    styledChars.Add(new StyledChar(content[i + 1], currentBold, currentItalic, currentUnderline));
                    i += 2;
                    continue;
                }

                if (i <= content.Length - 3 && content.Substring(i, 3) == "***")
                {
                    currentBold = !currentBold;
                    currentItalic = !currentItalic;
                    i += 3;
                    continue;
                }
                if (i <= content.Length - 2 && content.Substring(i, 2) == "**")
                {
                    currentBold = !currentBold;
                    i += 2;
                    continue;
                }
                if (content[i] == '*')
                {
                    currentItalic = !currentItalic;
                    i++;
                    continue;
                }
                if (i <= content.Length - 3 && content.Substring(i, 3) == "<u>")
                {
                    currentUnderline = true;
                    i += "<u>".Length;
                    continue;
                }
                if (i <= content.Length - 4 && content.Substring(i, 4) == "</u>")
                {
                    currentUnderline = false;
                    i += "</u>".Length;
                    continue;
                }

                styledChars.Add(new StyledChar(content[i], currentBold, currentItalic, currentUnderline));
                i++;
            }

            var sb = new StringBuilder();
            foreach (var sc in styledChars)
            {
                string ansiSequence = GetAnsiCode(sc);
                sb.Append(ansiSequence);
                sb.Append(sc.Character);
                sb.Append(ResetAnsi);
            }

            return sb.ToString();
        }

        private string GetAnsiCode(StyledChar sc)
        {
            var codes = new List<string>();
            if (sc.Bold) codes.Add("1");
            if (sc.Italic) codes.Add("3");
            if (sc.Underline) codes.Add("4");
            if (codes.Count == 0)
                return ResetAnsi;
            return $"\x1b[{string.Join(";", codes)}m";
        }
    }
}
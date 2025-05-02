using System;
using System.Collections.Generic;
using System.Text;

namespace OOPsl.DocumentFunctions.Converters
{
    public class StyledChar
    {
        public char Character { get; }
        public bool Bold { get; }
        public bool Italic { get; }
        public bool Underline { get; }

        public StyledChar(char character, bool bold, bool italic, bool underline)
        {
            Character = character;
            Bold = bold;
            Italic = italic;
            Underline = underline;
        }

        public bool HasSameFormatting(StyledChar other)
        {
            return Bold == other.Bold && Italic == other.Italic && Underline == other.Underline;
        }
    }

    public class RtfToMarkdownConverter : IFormatConverter
    {
        public string Convert(string input)
        {
            List<StyledChar> styledChars = ParseRtf(input);
            return BuildMarkdown(styledChars);
        }

        private List<StyledChar> ParseRtf(string input)
        {
            var result = new List<StyledChar>();
            bool currentBold = false, currentItalic = false, currentUnderline = false;
            int i = 0;
            while (i < input.Length)
            {
                if (input[i] == '\\')
                {
                    if (i + 1 < input.Length)
                    {
                        result.Add(new StyledChar(input[i + 1], currentBold, currentItalic, currentUnderline));
                        i += 2;
                        continue;
                    }
                }

                if (input[i] == '<')
                {
                    if (i <= input.Length - 3 && input.Substring(i, 3) == "<b>")
                    {
                        currentBold = true;
                        i += 3;
                        continue;
                    }
                    if (i <= input.Length - 3 && input.Substring(i, 3) == "<i>")
                    {
                        currentItalic = true;
                        i += 3;
                        continue;
                    }
                    if (i <= input.Length - 3 && input.Substring(i, 3) == "<u>")
                    {
                        currentUnderline = true;
                        i += 3;
                        continue;
                    }
                    if (i <= input.Length - 4 && input.Substring(i, 4) == "</b>")
                    {
                        currentBold = false;
                        i += 4;
                        continue;
                    }
                    if (i <= input.Length - 4 && input.Substring(i, 4) == "</i>")
                    {
                        currentItalic = false;
                        i += 4;
                        continue;
                    }
                    if (i <= input.Length - 4 && input.Substring(i, 4) == "</u>")
                    {
                        currentUnderline = false;
                        i += 4;
                        continue;
                    }
                }

                result.Add(new StyledChar(input[i], currentBold, currentItalic, currentUnderline));
                i++;
            }
            return result;
        }

        private string BuildMarkdown(List<StyledChar> styledChars)
        {
            if (styledChars.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            var groupBuilder = new StringBuilder();
            StyledChar currentStyle = styledChars[0];

            foreach (var sch in styledChars)
            {
                if (!sch.HasSameFormatting(currentStyle))
                {
                    sb.Append(WrapWithMarkdown(groupBuilder.ToString(), currentStyle));
                    groupBuilder.Clear();
                    currentStyle = sch;
                }
                groupBuilder.Append(sch.Character);
            }
            sb.Append(WrapWithMarkdown(groupBuilder.ToString(), currentStyle));
            return sb.ToString();
        }

        private string WrapWithMarkdown(string text, StyledChar style)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (style.Bold && style.Italic)
                text = $"***{text}***";
            else if (style.Bold)
                text = $"**{text}**";
            else if (style.Italic)
                text = $"*{text}*";

            if (style.Underline)
                text = $"<u>{text}</u>";

            return text;
        }
    }
}
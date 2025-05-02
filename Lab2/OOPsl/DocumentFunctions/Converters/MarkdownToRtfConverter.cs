using System.Text.RegularExpressions;

namespace OOPsl.DocumentFunctions.Converters
{
    public class MarkdownToRtfConverter : IFormatConverter
    {
        public string Convert(string input)
        {
            string rtf = input;

            rtf = Regex.Replace(rtf, @"<u>\*\*\*(.+?)\*\*\*</u>", @"<u><b><i>$1</i></b></u>", RegexOptions.Singleline);
            rtf = Regex.Replace(rtf, @"\*\*\*(.+?)\*\*\*", @"<b><i>$1</i></b>", RegexOptions.Singleline);

            rtf = Regex.Replace(rtf, @"<u>\*\*(.+?)\*\*</u>", @"<u><b>$1</b></u>", RegexOptions.Singleline);
            rtf = Regex.Replace(rtf, @"\*\*(.+?)\*\*", @"<b>$1</b>", RegexOptions.Singleline);

            rtf = Regex.Replace(rtf, @"<u>\*(?!\*)(.+?)\*</u>", @"<u><i>$1</i></u>", RegexOptions.Singleline);
            rtf = Regex.Replace(rtf, @"\*(?!\*)(.+?)\*", @"<i>$1</i>", RegexOptions.Singleline);

            rtf = Regex.Replace(rtf, @"<u>(.+?)</u>", @"<u>$1</u>", RegexOptions.Singleline);

            return rtf;
        }
    }
}

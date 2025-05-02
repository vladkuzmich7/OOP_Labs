using OOPsl.DocumentFunctions.Displays;

namespace OOPsl.DocumentFunctions.Displayers
{
    public static class DisplayStrategyFactory
    {
        public static IDisplayStrategy GetStrategyForDocument(Document document)
        {
            string extension = Path.GetExtension(document.FileName).ToLower();
            switch (extension)
            {
                case ".md":
                    return new MarkdownDisplayStrategy();
                case ".rtf":
                    return new RichTextDisplayStrategy();
                case ".txt":
                    return new PlainTextDisplayStrategy();
                case ".json":
                    return new JsonDisplayStrategy();
                case ".xml":
                    return new XmlDisplayStrategy();
                default:
                    return new PlainTextDisplayStrategy();
            }
        }
    }
}

using OOPsl.DocumentFunctions.Displays;

namespace OOPsl.DocumentFunctions.Displayers
{
    public static class DocumentViewer
    {
        public static void Display(Document document, IDisplayStrategy displayStrategy)
        {
            displayStrategy.Display(document);
        }
    }
}

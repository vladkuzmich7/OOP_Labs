using DocMaster.Models;

namespace DocMaster.Data.Adapters
{
    public class TxtAdapter : IDocumentAdapter
    {
        public string Convert(Document doc)
        {
            // Просто возвращаем исходный текст
            return doc.Content;
        }
    }
}

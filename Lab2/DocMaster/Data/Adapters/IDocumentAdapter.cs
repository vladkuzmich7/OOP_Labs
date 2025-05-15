using DocMaster.Models;

namespace DocMaster.Data.Adapters
{
    public interface IDocumentAdapter
    {
        public string Convert(Document doc);
    }
}

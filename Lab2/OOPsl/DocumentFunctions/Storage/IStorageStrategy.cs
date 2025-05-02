using OOPsl.DocumentFunctions;

namespace OOPsl.DocumentFunctions.Storage
{
    public interface IStorageStrategy
    {
        void Save(Document document);
        Document Load(string fileName);
        void Delete(Document document);

        void SaveHistory(Document document);
        List<string> LoadHistory(Document document);
        void DeleteHistory(Document document);
    }
}

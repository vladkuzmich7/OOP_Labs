using DocMaster.Models;

namespace DocMaster.Services.FileService
{
    public interface IFileService
    {
        void Save(Document document, string path, DocumentFormat? targetFormat = null);
        Document Load(string fullPath);
        List<string> GetAvailableDocuments(string directory);
        void RemoveFromManifest(string manifestPath, string filePath);
        void AddToManifest(string manifestPath, string filePath);
        List<string> ReadManifest(string manifestPath);
        string GetExtension(DocumentFormat format);
        Document LoadDocument(string fullPath);
    }
}

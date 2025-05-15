
using DocMaster.Services.FileService;
using DocMaster.Models;

namespace DocMaster.Data.StorageStrategies
{
    public class LocalStorageStrategy : IStorageStrategy
    {
        private readonly string _storagePath;
        public LocalStorageStrategy(string storagePath)
        {
            _storagePath = storagePath;
        }
        public void Save(Document document, DocumentFormat targetFormat)
        {
            string content = document.ConvertTo(targetFormat);
            string extension = GetExtension(targetFormat);
            string fileName = $"{document.Name}{extension}";
            string fullPath = Path.Combine(_storagePath, fileName);
            File.WriteAllText(fullPath, content);
        }
        private string GetExtension(DocumentFormat format) => format switch
        {
            DocumentFormat.TXT => ".txt",
            DocumentFormat.Markdown => ".md",
            DocumentFormat.JSON => ".json",
            DocumentFormat.XML => ".xml",
            _ => ".txt"
        };
    }
}

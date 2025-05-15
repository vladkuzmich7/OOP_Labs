using DocMaster.Services.FileService;
using DocMaster.Models;
using DocMaster.Roles.Observers;
using DocMaster.Data.StorageStrategies;

namespace DocMaster.Services
{
    public class DocumentManager
    {
        private readonly IFileService _fileService;
        private readonly string _storagePath = Directory.GetCurrentDirectory();
        private const string ManifestFileName = ".docmaster_manifest";
        private readonly List<string> _createdDocuments = new();
        private readonly string _manifestPath;
        private readonly BlockedDocumentManager _blockManager;

        public DocumentManager(IFileService fileService, string storagePath)
        {
            _blockManager = new BlockedDocumentManager();
            _fileService = fileService;
            _storagePath = storagePath;
            _manifestPath = Path.Combine(storagePath, ManifestFileName);

            // Upload the manifest
            var existingEntries = _fileService.ReadManifest(_manifestPath);
            _createdDocuments.AddRange(existingEntries);

            // Clean non-existen files
            _createdDocuments.RemoveAll(path => !File.Exists(path));

            // Save updated manifest
            SaveManifest();
        }
        private void SaveManifest()
        {
            var manifestPath = Path.Combine(_storagePath, ManifestFileName);
            File.WriteAllLines(manifestPath, _createdDocuments);
        }

        public void SaveDocumentAs(Document document, DocumentFormat targetFormat)
        {
            // Сохраняем в новом формате без добавления в манифест
            var extension = new LocalFileService().GetExtension(targetFormat);
            var fullPath = Path.Combine(_storagePath, $"{document.Name}{extension}");
            File.WriteAllText(fullPath, document.ConvertTo(targetFormat));
        }
        public List<string> GetAvailableDocuments()
        {
            _createdDocuments.RemoveAll(path => !File.Exists(path));
            return new List<string>(_createdDocuments);
        }

        public Document CreateDocument(string name, DocumentFormat format)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("-----> Document name cannot be empty.");

            var doc = new Document(name, format);
            var fullPath = GetFullPath(doc);
            if (!_createdDocuments.Contains(fullPath))
            {
                _createdDocuments.Add(fullPath);
                SaveManifest();
            }
            else
            {
                throw new InvalidOperationException("-----> Document with this name and format already exists.");
            }

            return doc;
        }

        private string GetFullPath(Document document)
        {
            var extensions = new Dictionary<DocumentFormat, string>
            {
                { DocumentFormat.TXT, ".txt" },
                { DocumentFormat.Markdown, ".md" }
            };
            return Path.Combine(_storagePath, $"{document.Name}{extensions[document.Format]}");
        }
        public void SaveDocument(Document document)
        {
            var fullPath = GetFullPath(document);
            _fileService.Save(document, _storagePath);
        }

        public List<string> GetDocumentList()
        {
            return _fileService.ReadManifest(_manifestPath)
            .Where(File.Exists)
            .ToList();
        }

        public Document OpenDocument(string filePath)
        {
            return _fileService.Load(filePath);
        }
        public void DeleteDocument(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _fileService.RemoveFromManifest(_manifestPath, filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error deleting document: {ex.Message}");
            }
        }

        public void ExportDocument(Document doc, DocumentFormat targetFormat)
        {
            try
            {
                string content = doc.ConvertTo(targetFormat);
                string extension = targetFormat switch
                {
                    DocumentFormat.TXT => ".txt",
                    DocumentFormat.JSON => ".json",
                    DocumentFormat.XML => ".xml",
                    _ => throw new NotSupportedException()
                };

                string fileName = $"{Path.GetFileNameWithoutExtension(doc.Name)}{extension}";
                string path = Path.Combine(_storagePath, fileName);

                File.WriteAllText(path, content);
                _createdDocuments.Add(path);
                SaveManifest();

                Console.WriteLine($"Document exported to {targetFormat} successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Export failed: {ex.Message}");
            }
        }
        public void SaveUsingStrategy(Document document, DocumentFormat format, IStorageStrategy strategy)
        {
            strategy.Save(document, format);
        }
    }
}

using DocMaster.Models;

namespace DocMaster.Services.FileService
{
    public class LocalFileService : IFileService
    {
        private readonly Dictionary<DocumentFormat, string> _visibleExtensions = new()
        {
            {DocumentFormat.TXT, ".txt"},
            {DocumentFormat.Markdown, ".md"}
        };
        private readonly Dictionary<DocumentFormat, string> _allExtensions = new()
        {
            {DocumentFormat.TXT, ".txt"},
            {DocumentFormat.Markdown, ".md"},
            {DocumentFormat.JSON, ".json"},
            {DocumentFormat.XML, ".xml"}
        };
        public Document LoadDocument(string fullPath)
        {
            var content = File.ReadAllText(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var format = GetFormatFromExtension(Path.GetExtension(fullPath));

            return new Document(fileName, format) { Content = content };
        }
        private DocumentFormat GetFormatFromExtension(string extension) => extension.ToLower() switch
        {
            ".txt" => DocumentFormat.TXT,
            ".md" => DocumentFormat.Markdown,
            _ => DocumentFormat.TXT
        };
        public void Save(Document document, string path, DocumentFormat? targetFormat = null)
        {
            var format = targetFormat ?? document.Format;
            var extension = _allExtensions[format];
            var fullPath = Path.Combine(path, $"{document.Name}{extension}");
            var content = format == document.Format
                ? document.Content
                : document.ConvertTo(format);

            File.WriteAllText(fullPath, content);
        }

            public Document Load(string fullPath)
        {
            var content = File.ReadAllText(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var format = Path.GetExtension(fullPath).ToLower() switch
            {
                ".txt" => DocumentFormat.TXT,
                ".md" => DocumentFormat.Markdown,
                _ => throw new NotSupportedException("-----> Unsupported file format")
            };
            return new Document(fileName, format) { Content = content };
        }
        public List<string> GetAvailableDocuments(string directory)
        {
            var files = new List<string>();
            foreach (var format in _visibleExtensions.Values)
            {
                files.AddRange(Directory.GetFiles(directory, $"*{format}"));
            }
            return files;
        }
        public void RemoveFromManifest(string manifestPath, string filePath)
        {
            if (!File.Exists(manifestPath)) return;

            var entries = File.ReadAllLines(manifestPath)
                .Where(entry => entry != filePath)
                .ToList();

            File.WriteAllLines(manifestPath, entries);
        }

        public void AddToManifest(string manifestPath, string filePath)
        {
            var entries = new List<string>();
            if (File.Exists(manifestPath))
            {
                entries = File.ReadAllLines(manifestPath).ToList();
            }

            if (!entries.Contains(filePath))
            {
                entries.Add(filePath);
                File.WriteAllLines(manifestPath, entries);
            }
        }

        public List<string> ReadManifest(string manifestPath)
        {
            return File.Exists(manifestPath)
                ? File.ReadAllLines(manifestPath).ToList()
                : new List<string>();
        }
        public string GetExtension(DocumentFormat format) => _allExtensions.TryGetValue(format, out var ext) ? ext : ".txt";
    }
}

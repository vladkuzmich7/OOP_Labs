using DocMaster.Models;
using Microsoft.Win32;

namespace DocMaster.Data.StorageStrategies
{
    public class CloudStorageStrategy : IStorageStrategy
    {
        private readonly string _oneDrivePath;

        public CloudStorageStrategy()
        {
            _oneDrivePath = GetOneDrivePath();

            if (!Directory.Exists(_oneDrivePath))
                throw new DirectoryNotFoundException("OneDrive folder not found");
        }

        public void Save(Document document, DocumentFormat targetFormat)
        {
            string content = document.ConvertTo(targetFormat);
            string fileName = $"{document.Name}{GetExtension(targetFormat)}";
            string fullPath = Path.Combine(_oneDrivePath, "DocMaster", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
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
        private string GetOneDrivePath()
        {
            // Для личного аккаунта
            var userPath = Environment.GetEnvironmentVariable("OneDrive");
            if (!string.IsNullOrEmpty(userPath)) return userPath;

            // Резервный вариант через реестр
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\OneDrive");
            return key?.GetValue("UserFolder") as string ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "OneDrive"
            );
        }
    }
}

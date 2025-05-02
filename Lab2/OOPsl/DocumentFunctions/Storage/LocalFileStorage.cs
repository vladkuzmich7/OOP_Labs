using Newtonsoft.Json;
using OOPsl.DocumentFunctions;

namespace OOPsl.DocumentFunctions.Storage
{
    public class LocalFileStorage : IStorageStrategy
    {
        private readonly string documentsFolder = @"C:\oop";
        private readonly string historyFolder = @"C:\oop";

        public LocalFileStorage()
        {
            if (!Directory.Exists(documentsFolder))
                Directory.CreateDirectory(documentsFolder);
            if (!Directory.Exists(historyFolder))
                Directory.CreateDirectory(historyFolder);
        }

        public void Save(Document document)
        {
            try
            {
                string fileName = Path.GetFileName(document.FileName);
                string fullPath = Path.Combine(documentsFolder, fileName);
                File.WriteAllText(fullPath, document.Content);
                document.FileName = fullPath;
                SaveHistory(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при сохранении файла локально: " + ex.Message);
            }
        }

        public Document Load(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(documentsFolder, fileName);
                if (File.Exists(fullPath))
                {
                    var doc = new Document();
                    doc.Content = File.ReadAllText(fullPath);
                    doc.VersionHistory = LoadHistory(doc);
                    return doc;
                }
                else
                {
                    Console.WriteLine("Локальный файл не найден: " + fullPath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке локального файла: " + ex.Message);
                return null;
            }
        }

        public void Delete(Document document)
        {
            try
            {
                string fileName = Path.GetFileName(document.FileName);
                string fullPath = Path.Combine(documentsFolder, fileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                //DeleteHistory(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении локального файла: " + ex.Message);
            }
        }

        public void SaveHistory(Document document)
        {
            try
            {
                string historyPath = GetHistoryFilePath(document);
                //document.VersionHistory.Add(document.Content);
                string json = JsonConvert.SerializeObject(document.VersionHistory, Formatting.Indented);
                File.WriteAllText(historyPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при сохранении истории: " + ex.Message);
            }
        }

        public List<string> LoadHistory(Document document)
        {
            try
            {
                string historyPath = GetHistoryFilePath(document);
                if (File.Exists(historyPath))
                {
                    string json = File.ReadAllText(historyPath);
                    return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке истории: " + ex.Message);
                return new List<string>();
            }
        }

        public void DeleteHistory(Document document)
        {
            try
            {
                string historyPath = GetHistoryFilePath(document);
                if (File.Exists(historyPath))
                {
                    File.Delete(historyPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении истории: " + ex.Message);
            }
        }

        private string GetHistoryFilePath(Document document)
        {
            string baseName = Path.GetFileNameWithoutExtension(document.FileName);
            string historyFileName = $"{baseName}_history.json";
            return Path.Combine(historyFolder, historyFileName);
        }
    }
}

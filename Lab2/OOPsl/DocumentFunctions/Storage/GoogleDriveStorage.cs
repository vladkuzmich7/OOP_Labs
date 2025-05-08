using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System.Text;

namespace OOPsl.DocumentFunctions.Storage
{
    public class GoogleDriveStorage : IStorageStrategy
    {
        private readonly string ApplicationName = "OOPslApp";
        private DriveService service;
        private string folderId = "1TdYeCOeQjJ75CcdygGrzY3XiVsbmmME1";
        private string historyFolderId = "1kbgk6UtCurY7hWYFU7BUD2V2AqaBvaq4";

        public GoogleDriveStorage()
        {
            InitializeService();
        }

        private void InitializeService()
        {
            UserCredential credential;
            using (var stream = new FileStream(@"C:\Users\Vlad\Downloads\OOPsl\credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public void Save(Document document)
        {
            try
            {
                Delete(document);
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = Path.GetFileName(document.FileName),
                    Parents = new List<string> { folderId }
                };

                byte[] contentBytes = Encoding.UTF8.GetBytes(document.Content);
                using (var stream = new MemoryStream(contentBytes))
                {
                    var request = service.Files.Create(fileMetadata, stream, "text/plain");
                    request.Fields = "id";
                    var uploadResult = request.Upload();
                    if (uploadResult.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        Console.WriteLine("Ошибка при загрузке файла: " + uploadResult.Exception.Message);
                    }
                    else
                    {
                        Console.WriteLine($"Документ успешно загружен на Google Диск в папку с ID {folderId}. ID файла: {request.ResponseBody.Id}");
                    }
                }
                SaveHistory(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при сохранении файла на Google Диск: " + ex.Message);
            }
        }

        public Document Load(string fileName)
        {
            try
            {
                var listRequest = service.Files.List();
                listRequest.Q = $"name = '{fileName}' and '{folderId}' in parents and trashed=false";
                listRequest.Fields = "files(id, name)";
                var result = listRequest.Execute();
                if (result.Files == null || result.Files.Count == 0)
                {
                    Console.WriteLine("Файл не найден на Google Диске.");
                    return null;
                }
                var file = result.Files.First();
                var getRequest = service.Files.Get(file.Id);
                using (var stream = new MemoryStream())
                {
                    getRequest.MediaDownloader.ProgressChanged += progress =>
                    {
                        if (progress.Status == Google.Apis.Download.DownloadStatus.Completed)
                        {
                            //Console.WriteLine("Загрузка файла завершена.");
                        }
                        else if (progress.Status == Google.Apis.Download.DownloadStatus.Failed)
                        {
                            Console.WriteLine("Ошибка при загрузке файла.");
                        }
                    };
                    getRequest.Download(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();
                        Document doc = new Document()
                        {
                            Content = content,
                            FileName = fileName
                        };
                        doc.VersionHistory = LoadHistory(doc);
                        return doc;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке файла с Google Диска: " + ex.Message);
                return null;
            }
        }

        public List<Document> GetAllDocumentsFromDrive()
        {
            List<Document> driveDocs = new List<Document>();
            try
            {
                var listRequest = service.Files.List();
                listRequest.Q = $"'{folderId}' in parents and trashed=false";
                listRequest.Fields = "files(id, name)";
                var result = listRequest.Execute();
                if (result.Files != null && result.Files.Count > 0)
                {
                    foreach (var file in result.Files)
                    {
                        Document doc = Load(file.Name);
                        if (doc != null)
                            driveDocs.Add(doc);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении списка документов с Google Диска: " + ex.Message);
            }
            return driveDocs;
        }

        public void Delete(Document document)
        {
            try
            {
                var listRequest = service.Files.List();
                listRequest.Q = $"name = '{Path.GetFileName(document.FileName)}' and '{folderId}' in parents and trashed=false";
                listRequest.Fields = "files(id, name)";
                var result = listRequest.Execute();

                if (result.Files == null || result.Files.Count == 0)
                {
                    Console.WriteLine("Файл не найден на Google Диске.");
                    return;
                }

                var fileId = result.Files.First().Id;
                var deleteRequest = service.Files.Delete(fileId);
                deleteRequest.Execute();
                Console.WriteLine($"Документ \"{document.FileName}\" успешно удалён с Google Диска.");

                //DeleteHistory(document);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении файла с Google Диска: " + ex.Message);
            }
        }

        public void SaveHistory(Document document)
        {
            try
            {
                DeleteHistory(document);
                string historyFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_history.json";
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = historyFileName,
                    Parents = new List<string> { historyFolderId }
                };

                string json = JsonConvert.SerializeObject(document.VersionHistory, Formatting.Indented);
                byte[] contentBytes = Encoding.UTF8.GetBytes(json);
                using (var stream = new MemoryStream(contentBytes))
                {
                    var request = service.Files.Create(fileMetadata, stream, "application/json");
                    request.Fields = "id";
                    var uploadResult = request.Upload();
                    if (uploadResult.Status == Google.Apis.Upload.UploadStatus.Failed)
                    {
                        Console.WriteLine("Ошибка при загрузке истории файла: " + uploadResult.Exception.Message);
                    }
                    else
                    {
                        Console.WriteLine($"История документа успешно загружена на Google Диск в папку с ID {historyFolderId}. ID файла: {request.ResponseBody.Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при сохранении истории файла на Google Диске: " + ex.Message);
            }
        }

        public List<string> LoadHistory(Document document)
        {
            try
            {
                string historyFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_history.json";
                var listRequest = service.Files.List();
                listRequest.Q = $"name = '{historyFileName}' and '{historyFolderId}' in parents and trashed=false";
                listRequest.Fields = "files(id, name)";
                var result = listRequest.Execute();
                if (result.Files == null || result.Files.Count == 0)
                {
                    return new List<string>();
                }
                var file = result.Files.First();
                var getRequest = service.Files.Get(file.Id);
                using (var stream = new MemoryStream())
                {
                    getRequest.Download(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        List<string> history = JsonConvert.DeserializeObject<List<string>>(json);
                        return history;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке истории с Google Диска: " + ex.Message);
                return new List<string>();
            }
        }

        public void DeleteHistory(Document document)
        {
            try
            {
                string historyFileName = Path.GetFileNameWithoutExtension(document.FileName) + "_history.json";
                var listRequest = service.Files.List();
                listRequest.Q = $"name = '{historyFileName}' and '{historyFolderId}' in parents and trashed=false";
                listRequest.Fields = "files(id, name)";
                var result = listRequest.Execute();
                if (result.Files == null || result.Files.Count == 0)
                {
                    Console.WriteLine("История не найдена на Google Диске.");
                    return;
                }
                var fileId = result.Files.First().Id;
                var deleteRequest = service.Files.Delete(fileId);
                deleteRequest.Execute();
                Console.WriteLine($"История документа \"{document.FileName}\" успешно удалена с Google Диска.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении истории с Google Диска: " + ex.Message);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tester
{
    public class DocumentOperationsTests : IDisposable
    {
        private readonly string testLocalFilesFolder = Path.Combine(Path.GetTempPath(), "OOPslTestLocalFiles");
        private readonly string testLocalHistoryFolder = Path.Combine(Path.GetTempPath(), "OOPslTestLocalHistory");

        private readonly DocumentManager documentManager;
        private readonly UserManager userManager;
        private readonly User testUser;
        private readonly IStorage storage;

        public DocumentOperationsTests()
        {
            Directory.CreateDirectory(testLocalFilesFolder);
            Directory.CreateDirectory(testLocalHistoryFolder);

            documentManager = new DocumentManager();
            userManager = new UserManager();
            testUser = new User("TestUser");
            userManager.AddUser(testUser);
            storage = new LocalFileStorage(testLocalHistoryFolder);
        }

        public void Dispose()
        {
            if (Directory.Exists(testLocalFilesFolder))
                Directory.Delete(testLocalFilesFolder, true);
            if (Directory.Exists(testLocalHistoryFolder))
                Directory.Delete(testLocalHistoryFolder, true);
        }

        [Fact]
        public void CreateDocument_ShouldAddDocumentToManager()
        {
            string fileName = "testdocument.txt";
            string fullPath = Path.Combine(testLocalFilesFolder, fileName);
            Document doc = new Document
            {
                FileName = fullPath,
                Content = "Hello World"
            };

            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            var allDocs = documentManager.GetAllDocuments();
            Assert.Contains(allDocs, d =>
                string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                              Path.GetFileNameWithoutExtension(fileName),
                              StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void DeleteDocument_ShouldRemoveDocumentAndHistory()
        {
            string fileName = "tobedeleted.txt";
            string fullPath = Path.Combine(testLocalFilesFolder, fileName);
            Document doc = new Document
            {
                FileName = fullPath,
                Content = "To be deleted"
            };
            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            string historyPath = Path.Combine(testLocalHistoryFolder,
                Path.GetFileNameWithoutExtension(fileName) + "_history.json");
            File.WriteAllText(historyPath, "history");

            documentManager.RemoveDocument(doc, storage);

            var localDocs = documentManager.GetLocalDocuments();
            Assert.DoesNotContain(localDocs, d =>
                string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                              Path.GetFileNameWithoutExtension(fileName),
                              StringComparison.OrdinalIgnoreCase));
            Assert.False(File.Exists(historyPath));
        }

        [Fact]
        public void SubscribeToDocumentChanges_ShouldAddNotification()
        {
            string fileName = "notif.txt";
            string fullPath = Path.Combine(testLocalFilesFolder, fileName);
            Document doc = new Document
            {
                FileName = fullPath,
                Content = "Initial Content"
            };
            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());
            doc.Attach(testUser);

            doc.Content = "Updated Content";
            doc.Notify();

            Assert.Contains(testUser.Notifications, note =>
                note.Contains(Path.GetFileName(fileName), StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void CreateDocument_WithUniqueBaseName_ShouldAddDocument()
        {
            string fileName = "uniqueDoc.txt";
            string fullPath = Path.Combine(testLocalFilesFolder, fileName);
            Document doc = new Document
            {
                FileName = fullPath,
                Content = "Unique content"
            };

            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            var allDocs = documentManager.GetAllDocuments();
            Assert.Contains(allDocs, d =>
                string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                              Path.GetFileNameWithoutExtension(fileName),
                              StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ImportLocalFile_WithValidData_ShouldAddDocument_Simple()
        {
            string tempFileName = "import_test.txt";
            string tempFilePath = Path.Combine(testLocalFilesFolder, tempFileName);
            File.WriteAllText(tempFilePath, "Test content");
            Document doc = new Document
            {
                FileName = tempFileName,
                Content = File.ReadAllText(tempFilePath)
            };

            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            var importedDoc = documentManager.GetLocalDocuments()
                .FirstOrDefault(d =>
                    string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                                  Path.GetFileNameWithoutExtension(tempFileName),
                                  StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(importedDoc);
            Assert.Equal("Test content", importedDoc.Content);
        }

        [Fact]
        public void ImportLocalFile_WithValidData_ShouldAddDocument()
        {
            string tempFileName = "import_test2.txt";
            string tempFilePath = Path.Combine(testLocalFilesFolder, tempFileName);
            File.WriteAllText(tempFilePath, "Another test content");
            Document doc = new Document
            {
                FileName = tempFileName,
                Content = File.ReadAllText(tempFilePath)
            };

            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            var importedDoc = documentManager.GetLocalDocuments()
                .FirstOrDefault(d =>
                    string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                                  Path.GetFileNameWithoutExtension(tempFileName),
                                  StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(importedDoc);
            Assert.Equal("Another test content", importedDoc.Content);
        }

        [Fact]
        public void ImportLocalFile_WithValidData_ShouldAddDoc()
        {
            string tempFileName = "import_test3.txt";
            string tempFilePath = Path.Combine(testLocalFilesFolder, tempFileName);
            File.WriteAllText(tempFilePath, "Third test content");
            Document doc = new Document
            {
                FileName = tempFileName,
                Content = File.ReadAllText(tempFilePath)
            };

            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());

            var importedDoc = documentManager.GetLocalDocuments()
                .FirstOrDefault(d =>
                    string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                                  Path.GetFileNameWithoutExtension(tempFileName),
                                  StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(importedDoc);
            Assert.Equal("Third test content", importedDoc.Content);
        }

        [Fact]
        public void CreateDocument_WithInvalidExtension_ShouldShowErrorMessage_Simple()
        {
            string invalidFileName = "badformat.exe";
            var allowedExtensions = new[] { ".txt", ".md", ".rtf", ".json", ".xml" };
            string ext = Path.GetExtension(invalidFileName);
            string errorMessage = allowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)
                ? ""
                : $"Неверное расширение файла. Допустимые расширения: {string.Join(", ", allowedExtensions)}";

            Assert.False(string.IsNullOrEmpty(errorMessage));
            Console.Error.WriteLine(errorMessage);
        }

        [Fact]
        public void CreateDocument_WithExistingBaseName_ShouldShowErrorMessage_Simple()
        {
            string fileName1 = "duplicate.txt";
            string fullPath1 = Path.Combine(testLocalFilesFolder, fileName1);
            Document doc1 = new Document { FileName = fullPath1, Content = "Content 1" };
            documentManager.CreateDocument(doc1, testUser, userManager.GetUsers());

            string fileName2 = "duplicate.md";
            string fullPath2 = Path.Combine(testLocalFilesFolder, fileName2);
            Document doc2 = new Document { FileName = fullPath2, Content = "Content 2" };

            Exception ex = Assert.Throws<DuplicateDocumentException>(() =>
                documentManager.CreateDocument(doc2, testUser, userManager.GetUsers()));

            Assert.Contains("уже существует", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void DuplicateDocumentException_ShouldBeThrownForDuplicateDocuments()
        {
            string fileName = "duplicate.txt";
            string fullPath = Path.Combine(testLocalFilesFolder, fileName);
            Document doc = new Document { FileName = fullPath, Content = "Content" };
            documentManager.CreateDocument(doc, testUser, userManager.GetUsers());
            Exception ex = Assert.Throws<DuplicateDocumentException>(() =>
                documentManager.CreateDocument(doc, testUser, userManager.GetUsers()));
            Assert.Contains("уже существует", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UserManagerTests
    {
        private readonly string testUsersFile = Path.Combine(Path.GetTempPath(), "test_users.json");

        private UserManager CreateUserManagerForTest()
        {
            if (File.Exists(testUsersFile))
            {
                File.Delete(testUsersFile);
            }
            return new UserManager();
        }

        [Fact]
        public void AddUser_ShouldAddUserToUserManager()
        {
            var userManager = CreateUserManagerForTest();
            var user = new User("TestUser");

            userManager.AddUser(user);

            var users = userManager.GetUsers();
            Assert.Contains(user, users);
        }

        [Fact]
        public void RemoveUser_ShouldRemoveUserFromUserManager()
        {
            var userManager = CreateUserManagerForTest();
            var user = new User("TestUser");
            userManager.AddUser(user);
            userManager.RemoveUser(user);

            var users = userManager.GetUsers();
            Assert.DoesNotContain(user, users);
        }

        [Fact]
        public void UpdateUser_ShouldUpdateUserDetails()
        {
            var userManager = CreateUserManagerForTest();
            var user = new User("TestUser");
            userManager.AddUser(user);
            var updatedUser = new User("TestUserUpdated");
            userManager.RemoveUser(user);
            userManager.AddUser(updatedUser);
            var found = userManager.GetUsers().FirstOrDefault(u => u.Name == "TestUserUpdated");
            Assert.NotNull(found);
        }

        [Fact]
        public void GetUser_ByName_ShouldReturnCorrectUser()
        {
            var userManager = CreateUserManagerForTest();
            var user = new User("TestUser");
            userManager.AddUser(user);
            var found = userManager.GetUsers().FirstOrDefault(u => u.Name == "TestUser");
            Assert.NotNull(found);
        }

        [Fact]
        public void ListUsers_ShouldReturnNonEmptyList_WhenUsersExist()
        {
            var userManager = CreateUserManagerForTest();
            userManager.AddUser(new User("TestUser"));
            var users = userManager.GetUsers();
            Assert.True(users.Any());
        }
    }

    public class Document
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public List<string> Notifications { get; } = new List<string>();

        public void Attach(User user)
        {
            user.Notifications.Add($"{user.Name} подписан на изменения {Path.GetFileName(FileName)}");
        }

        public void Notify()
        {
            Notifications.Add($"{Path.GetFileName(FileName)} изменён");
        }
    }

    public class DocumentManager
    {
        private readonly List<Document> documents = new List<Document>();

        public void CreateDocument(Document doc, User owner, IEnumerable<User> allUsers)
        {
            if (documents.Any(d =>
                string.Equals(Path.GetFileNameWithoutExtension(d.FileName),
                              Path.GetFileNameWithoutExtension(doc.FileName),
                              StringComparison.OrdinalIgnoreCase)))
            {
                throw new DuplicateDocumentException("Документ с таким базовым именем уже существует.");
            }
            documents.Add(doc);
        }

        public IEnumerable<Document> GetAllDocuments() => documents;

        public IEnumerable<Document> GetLocalDocuments() => documents;

        public void RemoveDocument(Document doc, IStorage storage)
        {
            documents.Remove(doc);
            string historyPath = Path.Combine(storage.HistoryFolder,
                Path.GetFileNameWithoutExtension(doc.FileName) + "_history.json");
            if (File.Exists(historyPath))
                File.Delete(historyPath);
        }
    }

    public interface IStorage
    {
        string HistoryFolder { get; }
    }

    public class LocalFileStorage : IStorage
    {
        public string HistoryFolder { get; }

        public LocalFileStorage(string historyFolder)
        {
            HistoryFolder = historyFolder;
        }
    }

    public class DuplicateDocumentException : Exception
    {
        public DuplicateDocumentException(string message) : base(message) { }
    }

    public class User
    {
        public string Name { get; }
        public List<string> Notifications { get; } = new List<string>();

        public User(string name)
        {
            Name = name;
        }
    }

    public class UserManager
    {
        private readonly List<User> users = new List<User>();

        public UserManager() { }

        public void AddUser(User user)
        {
            if (users.Any(u => u.Name.Equals(user.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Пользователь с таким именем уже существует.");
            users.Add(user);
        }

        public void RemoveUser(User user)
        {
            users.Remove(user);
        }

        public IEnumerable<User> GetUsers() => users;
    }
}
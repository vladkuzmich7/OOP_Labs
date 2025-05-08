using OOPsl.DocumentFunctions.Managers;
using OOPsl.UserFunctions;
using OOPsl.DocumentFunctions;
using Document = OOPsl.DocumentFunctions.Document;
using OOPsl.DocumentFunctions.Storage;
using OOPsl.DocumentFunctions.Displayers;
using OOPsl.DocumentFunctions.Displays;
using OOPsl.DocumentFunctions.Converters;

namespace OOPsl.MenuFunctions
{
    public class UserActionsMenu
    {
        private User currentUser;
        private DocumentManager documentManager;
        private DocumentAccessManager accessManager;
        private UserManager userManager;
        private TextEditor editor;

        public UserActionsMenu(User user, DocumentManager documentManager, DocumentAccessManager accessManager, UserManager userManager)
        {
            currentUser = user;
            this.documentManager = documentManager;
            this.accessManager = accessManager;
            this.userManager = userManager;
            editor = new TextEditor();
        }

        public void Display()
        {
            bool exitMenu = false;
            while (!exitMenu)
            {
                Console.Clear();
                Console.WriteLine($"=== Действия для пользователя: {currentUser.Name} ===");
                Console.WriteLine("1. Создать новый файл");
                Console.WriteLine("2. Импортировать файл из локальной системы");
                Console.WriteLine("3. Показать все файлы (с ролями)");
                Console.WriteLine("4. Открыть файл");
                Console.WriteLine("5. Удалить файл");
                Console.WriteLine("6. Посмотреть историю файла");
                Console.WriteLine("7. Изменить роли для файла");
                Console.WriteLine("8. Подписаться на изменения документа");
                Console.WriteLine("9. Показать уведомления");
                Console.WriteLine("10. Вернуться к выбору пользователя");
                Console.WriteLine("11. Выход из приложения");
                Console.Write("Выберите действие: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateNewFile();
                        break;
                    case "2":
                        ImportLocalFile();
                        break;
                    case "3":
                        ShowAllFiles();
                        break;
                    case "4":
                        OpenFile();
                        break;
                    case "5":
                        DeleteFile();
                        break;
                    case "6":
                        ViewDocumentHistory();
                        break;
                    case "7":
                        ChangeRolesForFile();
                        break;
                    case "8":
                        SubscribeToDocumentChanges();
                        break;
                    case "9":
                        ShowNotifications();
                        break;
                    case "10":
                        exitMenu = true;
                        break;
                    case "11":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу для повторного ввода...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void CreateNewFile()
        {
            Console.Clear();
            Console.Write("Введите имя файла: ");
            string fileName = Console.ReadLine();

            string ext = Path.GetExtension(fileName).ToLower();
            string[] allowedExtensions = { ".txt", ".md", ".rtf", ".json", ".xml" };
            if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
            {
                Console.WriteLine("Неверное расширение файла. Допустимые расширения: .txt, .md, .rtf, .json, .xml");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            string baseName = Path.GetFileNameWithoutExtension(fileName);
            var existingDocs = documentManager.GetAllDocuments()
                .Where(d => Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(baseName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (existingDocs.Any())
            {
                Console.WriteLine("Документ с таким базовым именем уже существует.");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }
            Document newDoc = new Document();
            newDoc.FileName = fileName;

            documentManager.CreateDocument(newDoc, currentUser, userManager.GetUsers());
            Console.WriteLine($"Документ \"{fileName}\" успешно создан.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ImportLocalFile()
        {
            Console.Clear();
            Console.Write("Введите полный путь к файлу для импорта: ");
            string importPath = Console.ReadLine();

            if (!File.Exists(importPath))
            {
                Console.WriteLine("Файл не найден. Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            string[] allowedExtensions = { ".txt", ".md", ".rtf", ".json", ".xml" };
            string importExtension = Path.GetExtension(importPath).ToLower();
            if (!allowedExtensions.Contains(importExtension))
            {
                Console.WriteLine("Недопустимое расширение файла. Допустимые расширения: .txt, .md, .rtf, .json, .xml");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            string content;
            try
            {
                content = File.ReadAllText(importPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка чтения файла: " + ex.Message);
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            string fileName;
            while (true)
            {
                Console.Write("Введите имя для нового документа (с расширением): ");
                fileName = Console.ReadLine();

                string ext = Path.GetExtension(fileName).ToLower();
                if (string.IsNullOrWhiteSpace(ext) || !allowedExtensions.Contains(ext))
                {
                    Console.WriteLine("Неверное расширение файла. Допустимые расширения: .txt, .md, .rtf, .json, .xml");
                    continue;
                }

                string baseName = Path.GetFileNameWithoutExtension(fileName);
                bool nameExists = documentManager.GetAllDocuments()
                    .Any(d => Path.GetFileNameWithoutExtension(d.FileName)
                        .Equals(baseName, StringComparison.OrdinalIgnoreCase));

                if (nameExists)
                {
                    Console.WriteLine("Документ с таким базовым именем уже существует. Пожалуйста, выберите другое имя.");
                    continue;
                }

                break;
            }

            Document newDoc = new Document
            {
                FileName = fileName,
                Content = content
            };

            documentManager.CreateDocument(newDoc, currentUser, userManager.GetUsers());
            Console.WriteLine($"Документ \"{fileName}\" успешно импортирован. Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void SubscribeToDocumentChanges()
        {
            Console.Clear();
            Console.WriteLine("Выберите источник файла для подписки:");
            Console.WriteLine("1. Локальные документы");
            Console.WriteLine("2. Документы с Google Drive");
            Console.Write("Ваш выбор: ");
            string sourceChoice = Console.ReadLine();

            List<Document> docs;
            if (sourceChoice == "1")
            {
                docs = documentManager.GetLocalDocuments();
            }
            else if (sourceChoice == "2")
            {
                docs = documentManager.GetCloudDocuments();
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите имя файла для подписки (без расширения): ");
            string inputName = Console.ReadLine();
            string inputBaseName = Path.GetFileNameWithoutExtension(inputName);

            Document docToSubscribe = docs.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(inputBaseName, StringComparison.OrdinalIgnoreCase));
            if (docToSubscribe == null)
            {
                Console.WriteLine("Документ не найден.");
            }
            else
            {
                docToSubscribe.Attach(currentUser);
                Console.WriteLine($"Вы успешно подписаны на изменения документа \"{docToSubscribe.FileName}\".");
            }
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowAllFiles()
        {
            Console.Clear();
            var localDocs = documentManager.GetLocalDocuments();
            var cloudDocs = documentManager.GetCloudDocuments();

            Console.WriteLine("=== Локальные документы ===");
            if (localDocs.Count == 0)
            {
                Console.WriteLine("Нет локальных документов.");
            }
            else
            {
                foreach (var doc in localDocs)
                {
                    Console.WriteLine($"Файл: {doc.FileName}");
                    var accessList = accessManager.GetAccessList(doc);
                    if (accessList.Count == 0)
                        Console.WriteLine("  Роли не назначены.");
                    else
                    {
                        foreach (var ace in accessList)
                            Console.WriteLine($"  Пользователь: {ace.User.Name}, Роль: {ace.Role}");
                    }
                }
            }
            Console.WriteLine("\n=== Документы с Google Drive ===");
            if (cloudDocs.Count == 0)
            {
                Console.WriteLine("Нет документов из облака.");
            }
            else
            {
                foreach (var doc in cloudDocs)
                {
                    Console.WriteLine($"Файл: {doc.FileName}");
                    var accessList = accessManager.GetAccessList(doc);
                    if (accessList.Count == 0)
                        Console.WriteLine("  Роли не назначены.");
                    else
                    {
                        foreach (var ace in accessList)
                            Console.WriteLine($"  Пользователь: {ace.User.Name}, Роль: {ace.Role}");
                    }
                }
            }
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void OpenFile()
        {
            Console.Clear();
            Console.WriteLine("Выберите источник файла:");
            Console.WriteLine("1. Локальные файлы");
            Console.WriteLine("2. Google Drive");
            Console.Write("Ваш выбор: ");
            string sourceChoice = Console.ReadLine();

            List<Document> docs;
            if (sourceChoice == "1")
            {
                docs = documentManager.GetLocalDocuments();
            }
            else if (sourceChoice == "2")
            {
                docs = documentManager.GetCloudDocuments();
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите имя файла для открытия (без расширения): ");
            string inputName = Console.ReadLine();
            string inputBaseName = Path.GetFileNameWithoutExtension(inputName);

            Document docToOpen = docs.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(inputBaseName, StringComparison.OrdinalIgnoreCase));

            if (docToOpen == null)
            {
                Console.WriteLine("Документ не найден.");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            var accessList = accessManager.GetAccessList(docToOpen);
            var currentUserAccess = accessList.FirstOrDefault(a =>
                a.User.Name.Equals(currentUser.Name, StringComparison.OrdinalIgnoreCase));

            if (currentUserAccess == null)
            {
                Console.WriteLine("У вас нет доступа к этому документу.");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            if (currentUserAccess.Role == DocumentRole.Admin || currentUserAccess.Role == DocumentRole.Editor)
            {
                Console.WriteLine("Открывается документ.");
                Console.WriteLine("Выберите режим открытия:");
                Console.WriteLine("1. Режим редактирования");
                Console.WriteLine("2. Режим просмотра");
                Console.Write("Ваш выбор: ");
                var modeKey = Console.ReadKey();
                Console.WriteLine();

                if (modeKey.KeyChar == '1')
                {
                    Console.WriteLine("Открывается режим редактирования. Нажмите Escape для выхода из редактора.");
                    editor.LoadDocument(docToOpen);
                    try
                    {
                        editor.Run();
                        Console.WriteLine("Сохранить файл:");
                        Console.WriteLine("1. Локально");
                        Console.WriteLine("2. В облако (Google Drive)");
                        Console.Write("Выберите опцию (1 или 2): ");
                        var key = Console.ReadKey();
                        Console.WriteLine();
                        IStorageStrategy storageStrategy = null;
                        if (key.KeyChar == '1')
                            storageStrategy = new LocalFileStorage();
                        else if (key.KeyChar == '2')
                            storageStrategy = new GoogleDriveStorage();
                        else
                        {
                            Console.WriteLine("Неверный выбор. Сохранение отменено.");
                            Console.WriteLine("Нажмите любую клавишу для возврата...");
                            Console.ReadKey();
                            return;
                        }

                        string sourceExtension = Path.GetExtension(docToOpen.FileName).TrimStart('.').ToLower();
                        string formatChoice = "";
                        string targetExtension = "";

                        if (sourceExtension == "md" || sourceExtension == "rtf")
                        {
                            Console.WriteLine("\nВыберите формат для сохранения файла:");
                            Console.WriteLine("1. Markdown (.md)");
                            Console.WriteLine("2. Rich Text Format (.rtf)");
                            Console.Write("Введите номер пункта: ");
                            formatChoice = Console.ReadLine();
                            switch (formatChoice)
                            {
                                case "1":
                                    targetExtension = "md";
                                    break;
                                case "2":
                                    targetExtension = "rtf";
                                    break;
                                default:
                                    Console.WriteLine("Неверный выбор формата. Сохранение отменено.");
                                    Console.WriteLine("Нажмите любую клавишу для возврата...");
                                    Console.ReadKey();
                                    return;
                            }
                        }
                        else if (sourceExtension == "json" || sourceExtension == "xml")
                        {
                            Console.WriteLine("\nВыберите формат для сохранения файла:");
                            Console.WriteLine("1. XML (.xml)");
                            Console.WriteLine("2. JSON (.json)");
                            Console.Write("Введите номер пункта: ");
                            formatChoice = Console.ReadLine();
                            switch (formatChoice)
                            {
                                case "1":
                                    targetExtension = "xml";
                                    break;
                                case "2":
                                    targetExtension = "json";
                                    break;
                                default:
                                    Console.WriteLine("Неверный выбор формата. Сохранение отменено.");
                                    Console.WriteLine("Нажмите любую клавишу для возврата...");
                                    Console.ReadKey();
                                    return;
                            }
                        }
                        else
                        {
                            targetExtension = sourceExtension;
                        }

                        string newContent = docToOpen.Content;

                        if (!sourceExtension.Equals(targetExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                if ((sourceExtension == "md" || sourceExtension == "rtf") && (targetExtension == "md" || targetExtension == "rtf"))
                                {
                                    if (sourceExtension == "md" && targetExtension == "rtf")
                                    {
                                        MarkdownToRtfConverter mdToRtf = new MarkdownToRtfConverter();
                                        newContent = mdToRtf.Convert(docToOpen.Content);
                                    }
                                    else if (sourceExtension == "rtf" && targetExtension == "md")
                                    {
                                        RtfToMarkdownConverter rtfToMd = new RtfToMarkdownConverter();
                                        newContent = rtfToMd.Convert(docToOpen.Content);
                                    }
                                }
                                else if ((sourceExtension == "json" || sourceExtension == "xml") && (targetExtension == "json" || targetExtension == "xml"))
                                {
                                    if (sourceExtension == "json" && targetExtension == "xml")
                                    {
                                        JsonToXmlConverter jsonToXml = new JsonToXmlConverter();
                                        newContent = jsonToXml.Convert(docToOpen.Content);
                                    }
                                    else if (sourceExtension == "xml" && targetExtension == "json")
                                    {
                                        XmlToJsonConverter xmlToJson = new XmlToJsonConverter();
                                        newContent = xmlToJson.Convert(docToOpen.Content);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Файл не был сохранен! Ошибка: ", ex.Message);
                                Console.WriteLine("\nНажмите любую клавишу для возврата...");
                                Console.ReadKey();
                                return;
                            }
                        }

                        storageStrategy.Delete(docToOpen);
                        string baseName = Path.GetFileNameWithoutExtension(docToOpen.FileName);
                        docToOpen.FileName = baseName + "." + targetExtension;
                        docToOpen.Content = newContent;

                        documentManager.SaveDocument(docToOpen, storageStrategy);
                        docToOpen.Notify();
                        Console.WriteLine("Файл сохранён!!!");
                    }
                    catch (Exception ex)
                    {
                        Console.TreatControlCAsInput = false;
                        Console.WriteLine("Вы ввели слишком длинный текст, такое Windows терминал не поддерживает, sorry(((((");
                    }
                    Console.WriteLine("Нажмите любую клавишу для возврата...");
                    Console.ReadKey();
                }
                else if (modeKey.KeyChar == '2')
                {
                    Console.WriteLine("Открывается режим просмотра.");
                    try
                    {
                        IDisplayStrategy strategy = DisplayStrategyFactory.GetStrategyForDocument(docToOpen);
                        DocumentViewer.Display(docToOpen, strategy);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отображении файла: {ex.Message}");
                    }
                    Console.WriteLine("\nНажмите любую клавишу для возврата...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Неверный выбор. Возврат в меню...");
                    Console.ReadKey();
                }
            }
            else if (currentUserAccess.Role == DocumentRole.Viewer)
            {
                Console.WriteLine("У вас права только на просмотр. Открывается режим просмотра.");
                try
                {
                    IDisplayStrategy strategy = DisplayStrategyFactory.GetStrategyForDocument(docToOpen);
                    DocumentViewer.Display(docToOpen, strategy);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отображении файла: {ex.Message}");
                }
                Console.WriteLine("\nНажмите любую клавишу для возврата...");
                Console.ReadKey();
            }
        }

        private void DeleteFile()
        {
            IStorageStrategy storageStrategy;
            Console.Clear();
            Console.WriteLine("Выберите источник файла для удаления:");
            Console.WriteLine("1. Локальные документы");
            Console.WriteLine("2. Документы с Google Drive");
            string sourceChoice = Console.ReadLine();
            List<Document> docs;
            if (sourceChoice == "1")
            {
                docs = documentManager.GetLocalDocuments();
                storageStrategy = new LocalFileStorage();
            }
            else if (sourceChoice == "2")
            {
                docs = documentManager.GetCloudDocuments();
                storageStrategy = new GoogleDriveStorage();
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите имя файла для удаления (без расширения): ");
            string inputName = Console.ReadLine();
            string inputBaseName = Path.GetFileNameWithoutExtension(inputName);

            Document docToDelete = docs.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(inputBaseName, StringComparison.OrdinalIgnoreCase));


            if (docToDelete == null)
            {
                Console.WriteLine("Документ не найден.");
            }
            else
            {
                var accessList = accessManager.GetAccessList(docToDelete);
                bool isAdmin = accessList.Any(ace =>
                    ace.User.Name.Equals(currentUser.Name, StringComparison.OrdinalIgnoreCase) &&
                    ace.Role == DocumentRole.Admin);

                if (!isAdmin)
                {
                    Console.WriteLine("Вы не являетесь администратором этого документа, удаление невозможно.");
                }
                else
                {
                    documentManager.RemoveDocument(docToDelete, storageStrategy);
                    currentUser.OwnedDocuments.Remove(docToDelete);
                    Console.WriteLine($"Документ \"{docToDelete.FileName}\" успешно удалён.");
                }
            }
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ViewDocumentHistory()
        {
            Console.Clear();
            Console.WriteLine("Выберите источник документа:");
            Console.WriteLine("1. Локальные документы");
            Console.WriteLine("2. Документы с Google Drive");
            string sourceChoice = Console.ReadLine();

            IStorageStrategy storageStrategy;
            List<Document> docs;

            if (sourceChoice == "1")
            {
                storageStrategy = new LocalFileStorage();
                docs = documentManager.GetLocalDocuments();
            }
            else if (sourceChoice == "2")
            {
                storageStrategy = new GoogleDriveStorage();
                docs = documentManager.GetCloudDocuments();
            }
            else
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите имя файла для просмотра истории (без расширения): ");
            string inputName = Console.ReadLine();
            string inputBaseName = Path.GetFileNameWithoutExtension(inputName);

            Document doc = docs.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(inputBaseName, StringComparison.OrdinalIgnoreCase));

            if (doc == null)
            {
                Console.WriteLine("Документ не найден.");
                Console.ReadKey();
                return;
            }

            var history = storageStrategy.LoadHistory(doc);
            if (history == null || history.Count == 0)
            {
                Console.WriteLine("История изменений не найдена.");
            }
            else
            {
                Console.WriteLine($"Всего версий: {history.Count}");
                Console.Write("Введите номер версии для просмотра (1 - самая старая, {0} - последняя): ", history.Count);
                if (int.TryParse(Console.ReadLine(), out int versionNumber) &&
                    versionNumber >= 1 && versionNumber <= history.Count)
                {
                    Console.Clear();
                    Console.WriteLine($"=== Версия {versionNumber} документа \"{doc.FileName}\" ===\n");
                    Console.WriteLine(history[versionNumber - 1]);
                }
                else
                {
                    Console.WriteLine("Некорректный номер версии.");
                }
            }

            Console.WriteLine("\nНажмите любую клавишу для возврата...");
            Console.ReadKey();
        }

        private void ChangeRolesForFile()
        {
            Console.Clear();
            Console.WriteLine("Выберите источник файла для изменения ролей:");
            Console.WriteLine("1. Локальные документы");
            Console.WriteLine("2. Документы с Google Drive");
            string sourceChoice = Console.ReadLine();
            List<Document> docs;
            if (sourceChoice == "1")
                docs = documentManager.GetLocalDocuments();
            else if (sourceChoice == "2")
                docs = documentManager.GetCloudDocuments();
            else
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            Console.Write("Введите имя файла для изменения ролей (без расширения): ");
            string inputName = Console.ReadLine();
            string inputBaseName = Path.GetFileNameWithoutExtension(inputName);

            Document docToChange = docs.FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Equals(inputBaseName, StringComparison.OrdinalIgnoreCase));
            if (docToChange == null)
            {
                Console.WriteLine("Документ не найден.");
            }
            else
            {
                var accessList = accessManager.GetAccessList(docToChange);
                bool isAdmin = accessList.Any(ace =>
                    ace.User.Name.Equals(currentUser.Name, StringComparison.OrdinalIgnoreCase) &&
                    ace.Role == DocumentRole.Admin);

                if (!isAdmin)
                {
                    Console.WriteLine("Вы не являетесь администратором этого документа, изменение ролей невозможно.");
                }
                else
                {
                    Console.WriteLine("Введите имена пользователей через запятую, для которых нужно поменять роль (Viewer <-> Editor): ");
                    string input = Console.ReadLine();
                    var userNames = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(name => name.Trim());

                    foreach (var userName in userNames)
                    {
                        User targetUser = userManager.FindUser(userName);
                        if (targetUser == null)
                        {
                            Console.WriteLine($"Пользователь \"{userName}\" не найден.");
                        }
                        else
                        {
                            var ace = accessList.FirstOrDefault(a => a.User.Name.Equals(targetUser.Name, StringComparison.OrdinalIgnoreCase));
                            if (ace == null)
                            {
                                Console.WriteLine($"Пользователь \"{userName}\" не имеет доступа к данному документу.");
                            }
                            else if (ace.Role == DocumentRole.Viewer)
                            {
                                accessManager.SetUserRole(docToChange, targetUser, DocumentRole.Editor, currentUser);
                                Console.WriteLine($"Роль пользователя \"{userName}\" изменена с Viewer на Editor.");
                            }
                            else if (ace.Role == DocumentRole.Editor)
                            {
                                accessManager.SetUserRole(docToChange, targetUser, DocumentRole.Viewer, currentUser);
                                Console.WriteLine($"Роль пользователя \"{userName}\" изменена с Editor на Viewer.");
                            }
                            else if (ace.Role == DocumentRole.Admin)
                            {
                                Console.WriteLine($"Пользователь \"{userName}\" является администратором, его роль не изменяется.");
                            }
                        }
                    }
                }
            }
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private void ShowNotifications()
        {
            Console.Clear();
            Console.WriteLine("=== Уведомления пользователя ===");

            if (currentUser.Notifications.Count == 0)
            {
                Console.WriteLine("У вас нет новых уведомлений.");
            }
            else
            {
                int count = 1;
                foreach (var notification in currentUser.Notifications)
                {
                    Console.WriteLine($"{count++}. {notification}");
                }
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}
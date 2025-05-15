using DocMaster.Roles;
using DocMaster.Services;
using DocMaster.Utilities;
using DocMaster.Models;
using DocMaster.Services.FileService;
using DocMaster.Roles.Observers;
using DocMaster.Data.StorageStrategies;

namespace DocMaster.UI
{
    public class App
    {
        private readonly Menu _menu = new();
        private readonly RoleContext _roleContext = new();
        private readonly UserManager _userManager = new();

        private User _currentUser;
        private readonly DocumentManager _documentManager = new DocumentManager(new LocalFileService(), Directory.GetCurrentDirectory());
        private Document _currentDocument;

        private const string DatabaseConnectionString =
        "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

        public void Run()
        {
            InitializeUsers();
            while (true)
            {
                var selectedRole = ChooseRole();
                if (selectedRole == UserRole.Admin)
                {
                    if (!HandleAdminLogin()) continue;
                }
                else
                {
                    HandleUserLogin(selectedRole);
                }
                MainLoop();
            }
        }
        private void InitializeUsers()
        {
            _userManager.AddUser(new User("admin", UserRole.Admin));
        }

        private UserRole ChooseRole()
        {
            Console.Clear();
            Console.WriteLine("Choose a role:");
            Console.WriteLine("1. Viewer");
            Console.WriteLine("2. Editor");
            Console.WriteLine("3. Admin");
            Console.WriteLine("4. Exit");
            Console.Write("Choice: ");

            int choice = InputValidator.GetIntInput(1, 4);
            if (choice == 4) Environment.Exit(0);
            return (UserRole)(choice - 1);
        }
        private bool HandleAdminLogin()
        {
            Console.Write("\nEnter admin password: ");
            string password = Console.ReadLine();
            if (password != "parol")
            {
                Console.WriteLine("Incorrect password! Press any key...");
                Console.ReadKey();
                return false;
            }
            _currentUser = _userManager.GetUser("admin");
            _roleContext.SetRole(UserRole.Admin);
            Console.WriteLine($"Welcome, Admin!");
            Console.Write("Press any key...");
            Console.ReadKey();
            return true;
        }
        private void HandleUserLogin(UserRole role)
        {
            Console.Write("\nEnter your username: ");
            string username = Console.ReadLine();
            _currentUser = _userManager.GetUser(username);

            if (_currentUser == null)
            {
                _currentUser = new User(username, role);
                _userManager.AddUser(_currentUser);
            }

            _roleContext.SetRole(role);
            Console.WriteLine($"Welcome, {username}! Your role: {role}.");
            Console.Write("Press any key...");
            Console.ReadKey();
        }
        private void MainLoop()
        {
            while (true)
            {
                //Console.Clear();
                _menu.ShowMainMenu(_roleContext.CurrentRole);
                int maxOption = GetMaxMenuOption();
                int choice = InputValidator.GetIntInput(1, maxOption);

                if ((maxOption == 10 && choice == 10) || (maxOption == 9 && choice == 9) || (maxOption == 3 && choice == 3))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    _currentUser = null;
                    return;
                }
                if (_roleContext.CurrentRole == UserRole.Admin)
                {
                    switch (choice)
                    {
                        case 1 when _roleContext.CanEditDocument:
                            CreateDocument();
                            break;
                        case 2:
                            OpenDocument();
                            break;
                        case 3:
                            DeleteDocument();
                            break;
                        case 4:
                            EditDocument();
                            break;
                        case 5:
                            SearchDocument();
                            break;
                        case 6:
                            SaveDocument();
                            break;
                        case 7:
                            ChooseColor();
                            break;
                        case 8 when _roleContext.CanManageUsers:
                            ManageUsers();
                            break;
                        case 9:
                            ShowChangeHistory();
                            break;
                    }
                }
                else if (_roleContext.CurrentRole == UserRole.Editor)
                {
                    switch (choice)
                    {
                        case 1 when _roleContext.CanEditDocument:
                            CreateDocument();
                            break;
                        case 2:
                            OpenDocument();
                            break;
                        case 3:
                            DeleteDocument();
                            break;
                        case 4:
                            EditDocument();
                            break;
                        case 5:
                            SearchDocument();
                            break;
                        case 6:
                            SaveDocument();
                            break;
                        case 7:
                            ChooseColor();
                            break;
                        case 8:
                            ShowChangeHistory();
                            break;
                    }
                }
                else
                {
                    switch (choice)
                    {
                        case 1:
                            OpenDocument();
                            break;
                        case 2:
                            ChooseColor();
                            break;
                    }
                }
            }
        }
        private int GetMaxMenuOption() => _roleContext.CurrentRole switch
        {
            UserRole.Admin => 10,
            UserRole.Editor => 9,
            _ => 3
        };

        private void ManageUsers()
        {
            Console.Clear();
            _menu.ShowAdminMenu();

            int choice = InputValidator.GetIntInput(1, 5);
            if (choice == 1)
            {
                Console.Write("\nEnter username: ");
                string username = Console.ReadLine();

                var targetUser = _userManager.GetUser(username);
                if (targetUser == null)
                {
                    Console.WriteLine($"\n-----> User {username} not found!");
                    return;
                }
                if (targetUser.Username == _currentUser.Username)
                {
                    Console.WriteLine("\n-----> You cannot change your own role!");
                    return;
                }

                Console.WriteLine("Select new role:");
                Console.WriteLine("1. Viewer\n2. Editor");
                Console.Write("Choice: ");
                int roleChoice = InputValidator.GetIntInput(1, 2);

                _userManager.ChangeUserRole(username, (UserRole)(roleChoice - 1));
            }
            else if (choice == 2)
            {
                Console.WriteLine("\nAll users: ");
                Console.WriteLine("-------------");
                foreach (var user in _userManager.GetAllUsers())
                {
                    if (user.CurrentRole == UserRole.Admin)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"|* {user.Username} - {user.CurrentRole}");
                        Console.ResetColor();
                        continue;
                    }
                    Console.WriteLine($"| {user.Username} - {user.CurrentRole}");
                }
            }
            switch (choice)
            {
                case 3:
                    BlockDocumentForUser();
                    break;
                case 4:
                    UnblockDocumentForUser();
                    break;
                case 5:
                    ShowBlockedDocuments();
                    break;
            }
        }

        private void CreateDocument()
        {
            Console.Write("Enter document name: ");
            string name = Console.ReadLine();

            Console.WriteLine("Select document format: ");
            Console.WriteLine("1. TXT");
            Console.WriteLine("2. Markdown");
            //Console.WriteLine("3. RichText");
            Console.Write("Choice: ");

            int formatChoice = InputValidator.GetIntInput(1, 3);
            DocumentFormat format = (DocumentFormat)(formatChoice - 1);

            _currentDocument = _documentManager.CreateDocument(name, format);
            _documentManager.SaveDocument(_currentDocument);

            Console.WriteLine($"-----> Document {name} created successfully!");
            Console.Write("Press any key...");
            Console.ReadKey();
        }

        private void OpenDocument()
        {
            var documents = _documentManager.GetDocumentList()
                .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
                .ToList();

            if (documents.Count == 0)
            {
                Console.WriteLine("-----> No documents found!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nAvailable documents :");
            Console.WriteLine("--------------------");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"{i + 1}.* {Path.GetFileName(documents[i])}");
            }

            Console.Write("\nEnter file number or full path: ");
            string input = Console.ReadLine();

            try
            {
                if (int.TryParse(input, out int choice) && choice > 0 && choice <= documents.Count)
                {
                    _currentDocument = _documentManager.OpenDocument(documents[choice - 1]);
                    if (_currentUser.CurrentRole == UserRole.Editor || _currentUser.CurrentRole == UserRole.Admin)
                    {
                        _currentDocument.Subscribe(_currentUser);
                    }
                }
                else
                {
                    _currentDocument = _documentManager.OpenDocument(input);
                    if (_currentUser.CurrentRole == UserRole.Editor || _currentUser.CurrentRole == UserRole.Admin)
                    {
                        _currentDocument.Subscribe(_currentUser);
                    }
                }

                Console.WriteLine("\nDocument content:");
                Console.WriteLine("--------------------\n");
                Console.WriteLine(_currentDocument.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error opening document: {ex.Message}");
            }
            Console.WriteLine("\n--------------------");
            Console.Write("Press any key...");
            Console.ReadKey();
        }
        private void DeleteDocument()
        {
            var documents = _documentManager.GetDocumentList();

            if (documents.Count == 0)
            {
                Console.WriteLine("-----> No documents found!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nAvailable documents: ");
            Console.WriteLine("--------------------");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"{i + 1}.* {Path.GetFileName(documents[i])}");
            }

            Console.Write("\nEnter file number: ");
            string input = Console.ReadLine();


            if (int.TryParse(input, out int choice) && choice > 0 && choice <= documents.Count)
            {
                _documentManager.DeleteDocument(documents[choice - 1]);
            }

            Console.WriteLine("\n-----> Document was deleted.");
            Console.Write("Press any key...");
            Console.ReadKey();
        }
        private void EditDocument()
        {
            if (_currentDocument == null)
            {
                Console.WriteLine("-----> No document opened!");
                Console.ReadKey();
                return;
            }
            var editor = new TextEditor(_currentDocument, _currentUser);
            editor.StartEditing();

            _documentManager.SaveDocument(_currentDocument);
        }
        private void ChooseColor()
        {
            Console.WriteLine("\nSelect background color:");
            Console.WriteLine("1. Black");
            Console.WriteLine("2. DarkBlue");
            Console.WriteLine("3. Blue");
            Console.WriteLine("4. DarkYellow");
            Console.WriteLine("5. Yellow");
            Console.WriteLine("6. DarkCyan");
            Console.WriteLine("7. Cyan");
            Console.WriteLine("8. DarkMangenta");
            Console.WriteLine("9. Mangenta");
            Console.Write("Choice: ");

            int bgChoice = InputValidator.GetIntInput(1, 9);
            ConsoleColor bgColor = bgChoice switch
            {
                1 => ConsoleColor.Black,
                2 => ConsoleColor.DarkBlue,
                3 => ConsoleColor.Blue,
                4 => ConsoleColor.DarkYellow,
                5 => ConsoleColor.Yellow,
                6 => ConsoleColor.DarkCyan,
                7 => ConsoleColor.Cyan,
                8 => ConsoleColor.DarkMagenta,
                9 => ConsoleColor.Magenta,
                _ => ConsoleColor.Black
            };

            AppStyleSettings.Instance.ChangeColors(bgColor);

        }
        private void SaveDocument()
        {
            var documents = _documentManager.GetDocumentList();
            if (documents.Count == 0)
            {
                Console.WriteLine("\n-----> No documents available!");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nAvailable documents:");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(documents[i])}");
            }
            Console.Write("Choice: ");
            int choice = InputValidator.GetIntInput(1, documents.Count);
            var selectedDoc = _documentManager.OpenDocument(documents[choice - 1]);

            var allowedFormats = GetAllowedFormats(selectedDoc.Format);
            Console.WriteLine("\nSelect format:");
            for (int i = 0; i < allowedFormats.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {allowedFormats[i]}");
            }
            Console.Write("Choice: ");
            int formatChoice = InputValidator.GetIntInput(1, allowedFormats.Count);
            DocumentFormat targetFormat = allowedFormats[formatChoice - 1];

            Console.WriteLine("\nSelect storage location:");
            Console.WriteLine("1. Local");
            Console.WriteLine("2. Database");
            Console.WriteLine("3. Cloud");
            Console.Write("Choice: ");
            int storageChoice = InputValidator.GetIntInput(1, 3);

            var strategy = StorageStrategyFactory.CreateStrategy(
                type: (StorageType)(storageChoice - 1),
                storagePath: Directory.GetCurrentDirectory(),
                DatabaseConnectionString);

            _documentManager.SaveUsingStrategy(selectedDoc, targetFormat, strategy);
            Console.WriteLine("\n-----> Document saved successfully!");
            Console.ReadKey();
        }
        private List<DocumentFormat> GetAllowedFormats(DocumentFormat original)
        {
            var formats = new List<DocumentFormat> { original, DocumentFormat.JSON, DocumentFormat.XML };
            if (original == DocumentFormat.Markdown) formats.Add(DocumentFormat.TXT);
            return formats.Distinct().ToList();
        }

        private void BlockDocumentForUser()
        {
            // Проверка наличия пользователей
            var availableUsers = _userManager.GetAllUsers()
                .Where(u => u.CurrentRole != UserRole.Admin)
                .ToList();

            if (availableUsers.Count == 0)
            {
                Console.WriteLine("\n-----> No available users (Editors/Viewers) to block!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            // Проверка наличия документов
            var documents = _documentManager.GetDocumentList();
            if (documents.Count == 0)
            {
                Console.WriteLine("\n-----> No documents available to block!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect user to block:");
            availableUsers.ForEach(u => Console.WriteLine($"{availableUsers.IndexOf(u) + 1}. {u.Username}"));
            int userChoice = InputValidator.GetIntInput(1, availableUsers.Count);

            Console.WriteLine("\nSelect document to block:");
            documents.ForEach(d => Console.WriteLine($"{documents.IndexOf(d) + 1}. {Path.GetFileName(d)}"));
            int docChoice = InputValidator.GetIntInput(1, documents.Count);

            _userManager.BlockDocumentForUser(documents[docChoice - 1], availableUsers[userChoice - 1].Username);
            Console.WriteLine("\n-----> Document blocked successfully!");
            Console.Write("Press any key...");
            Console.ReadKey();
        }

        private void UnblockDocumentForUser()
        {
            var blocked = _userManager.GetBlockedDocuments();
            if (blocked.Count == 0)
            {
                Console.WriteLine("\n-----> No blocked documents found!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nSelect blocked entry to remove:");
            blocked.ForEach(b =>
                Console.WriteLine($"{blocked.IndexOf(b) + 1}. {Path.GetFileName(b.FilePath)} " +
                                  $"(users: {string.Join(", ", b.BlockedUsers)})"));

            int choice = InputValidator.GetIntInput(1, blocked.Count);
            var entry = blocked[choice - 1];

            _userManager.UnblockDocumentForUser(entry.FilePath, entry.BlockedUsers.First());
            Console.WriteLine("\n-----> Document unblocked successfully!");
            Console.Write("Press any key...");
            Console.ReadKey();
        }

        private void ShowBlockedDocuments()
        {
            var blocked = _userManager.GetBlockedDocuments();
            Console.WriteLine("\nBlocked Documents:");
            foreach (var block in blocked)
            {
                Console.WriteLine($"File: {Path.GetFileName(block.FilePath)}");
                Console.WriteLine($"Blocked users: {string.Join(", ", block.BlockedUsers)}");
                Console.WriteLine("-------------------");
            }
            Console.Write("Press any key...");
            Console.ReadKey();
        }
        private void ShowChangeHistory()
        {
            Console.Clear();
            Console.WriteLine("=== History of changes ===");

            IEnumerable<DocumentChangeRecord> records = _roleContext.CurrentRole == UserRole.Admin
                ? DocumentHistoryService.GetFullHistory()
                : DocumentHistoryService.GetUserHistory(_currentUser.Username);

            if (!records.Any())
            {
                Console.WriteLine("-----> No changes were found.");
            }
            else
            {
                foreach (var record in records.OrderByDescending(r => r.ChangeTime))
                {
                    Console.WriteLine(record);
                }
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
        private void SearchDocument()
        {
            var documents = _documentManager.GetDocumentList()
                .Where(f => f.EndsWith(".txt") || f.EndsWith(".md"))
                .ToList();

            if (documents.Count == 0)
            {
                Console.WriteLine("-----> No documents found!");
                Console.Write("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\nAvailable documents:");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(documents[i])}");
            }

            Console.Write("Enter document number: ");
            int docChoice = InputValidator.GetIntInput(1, documents.Count);
            string filePath = documents[docChoice - 1];

            Document doc;
            try
            {
                doc = _documentManager.OpenDocument(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error opening document: {ex.Message}");
                Console.ReadKey();
                return;
            }

            Console.Write("Enter word to search: ");
            string searchWord = Console.ReadLine().Trim();

            if (string.IsNullOrEmpty(searchWord))
            {
                Console.WriteLine("Search word cannot be empty.");
                Console.ReadKey();
                return;
            }

            var matches = FindAllMatches(doc.Content, searchWord);

            Console.WriteLine("\nSearch results:");
            HighlightMatches(doc.Content, matches);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private List<(int Start, int End)> FindAllMatches(string content, string searchWord)
        {
            var matches = new List<(int, int)>();
            int currentIndex = 0;

            while (currentIndex <= content.Length - searchWord.Length)
            {
                int foundIndex = content.IndexOf(searchWord, currentIndex, StringComparison.OrdinalIgnoreCase);
                if (foundIndex == -1)
                    break;

                matches.Add((foundIndex, foundIndex + searchWord.Length - 1));
                currentIndex = foundIndex + 1;
            }

            return matches;
        }

        private void HighlightMatches(string content, List<(int Start, int End)> matches)
        {
            int currentMatch = 0;
            ConsoleColor originalColor = Console.ForegroundColor;

            for (int i = 0; i < content.Length; i++)
            {
                if (currentMatch < matches.Count && i == matches[currentMatch].Start)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }

                Console.Write(content[i]);

                if (currentMatch < matches.Count && i == matches[currentMatch].End)
                {
                    Console.ForegroundColor = originalColor;
                    currentMatch++;
                }
            }
            Console.ForegroundColor = originalColor;
        }
    }
}

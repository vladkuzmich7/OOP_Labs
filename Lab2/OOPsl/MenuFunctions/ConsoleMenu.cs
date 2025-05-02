using OOPsl.DocumentFunctions.Managers;
using OOPsl.UserFunctions;

namespace OOPsl.MenuFunctions
{
    public class ConsoleMenu(UserManager userManager, DocumentManager documentManager, DocumentAccessManager accessManager) 
        : IMenu
    {
        public int Display()
        {
            Console.Clear();
            Console.WriteLine("=== Главное меню пользователей ===");
            Console.WriteLine("1. Показать текущих пользователей");
            Console.WriteLine("2. Создать нового пользователя");
            Console.WriteLine("3. Выбрать пользователя");
            Console.WriteLine("4. Изменить настройки редактора");
            Console.WriteLine("5. Выход");
            Console.Write("Выберите пункт: ");
            if (int.TryParse(Console.ReadLine(), out int option))
            {
                return option;
            }
            return -1;
        }

        public void DisplayUsers()
        {
            Console.Clear();
            Console.WriteLine("=== Список пользователей ===");
            var users = userManager.GetUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("Пользователей не найдено.");
            }
            else
            {
                foreach (var user in users)
                {
                    Console.WriteLine(user.Name);
                }
            }
            Console.WriteLine("Нажмите любую клавишу для возврата...");
            Console.ReadKey();
        }

        public void CreateUser()
        {
            Console.Clear();
            Console.Write("Введите имя нового пользователя: ");
            string newUserName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(newUserName))
            {
                Console.WriteLine("Имя не может быть пустым.");
            }
            else
            {
                User newUser = new RegularUser(newUserName);
                userManager.AddUser(newUser);
                accessManager.AddUserToAllDocuments(newUser, documentManager.GetAllDocuments());
                Console.WriteLine($"Пользователь \"{newUserName}\" успешно создан.");
            }
        }

        public void SelectUser()
        {
            Console.Clear();
            Console.WriteLine("=== Выбор пользователя ===");
            var users = userManager.GetUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("Пользователей не найдено. Сначала создайте нового пользователя.");
                Console.WriteLine("Нажмите любую клавишу для возврата...");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < users.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {users[i].Name}");
            }
            Console.Write("Выберите пользователя по номеру: ");
            if (int.TryParse(Console.ReadLine(), out int userIndex) && userIndex > 0 && userIndex <= users.Count)
            {
                User selectedUser = users[userIndex - 1];
                UserActionsMenu userActionsMenu = new UserActionsMenu(selectedUser, documentManager, accessManager, userManager);
                userActionsMenu.Display();
            }
            else
            {
                Console.WriteLine("Неверный выбор. Нажмите любую клавишу для возврата...");
                Console.ReadKey();
            }
        }
        public void ChangeSettings()
        {
            Console.Clear();
            Console.WriteLine("=== Изменение настроек редактора ===");
            Console.WriteLine($"Текущая тема: {EditorSettings.Instance.Theme}");
            //Console.WriteLine($"Текущий размер шрифта: {EditorSettings.Instance.FontSize}");
            Console.WriteLine();

            Console.Write("Введите новую тему (Dark/Light): ");
            string newTheme = Console.ReadLine();
            if (!string.IsNullOrEmpty(newTheme))
            {
                EditorSettings.Instance.Theme = newTheme;
                if (newTheme.Equals("Light", StringComparison.OrdinalIgnoreCase))
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Clear();
            }

            //Console.Write("Введите новый размер шрифта (целое число): ");
            //string fontSizeInput = Console.ReadLine();
            //if (int.TryParse(fontSizeInput, out int newFontSize))
            //{
            //    EditorSettings.Instance.FontSize = newFontSize;
            //    EditorSettings.Instance.ApplyFontSettings();
            //}
            //else
            //{
            //    Console.WriteLine("Некорректное значение размера шрифта.");
            //}

            //Console.WriteLine("Настройки сохранены. Нажмите любую клавишу для возврата...");
            //Console.ReadKey();
        }

    }
}
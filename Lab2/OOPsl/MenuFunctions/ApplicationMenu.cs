using OOPsl.DocumentFunctions.Managers;
using OOPsl.UserFunctions;

namespace OOPsl.MenuFunctions
{
    public class ApplicationMenu
    {
        private IMenu mainMenu;

        public ApplicationMenu(IMenu mainMenu)
        {
            this.mainMenu = mainMenu;
        }

        public void Run()
        {
            bool exit = false;
            while (!exit)
            {
                int option = mainMenu.Display();
                switch (option)
                {
                    case 1:
                        mainMenu.DisplayUsers();
                        break;
                    case 2:
                        mainMenu.CreateUser();
                        break;
                    case 3:
                        mainMenu.SelectUser();
                        break;
                    case 4:
                        mainMenu.ChangeSettings();
                        break;
                    case 5:
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Нажмите любую клавишу для повторного ввода...");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
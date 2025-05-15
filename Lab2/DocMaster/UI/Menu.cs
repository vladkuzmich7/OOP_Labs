
namespace DocMaster.UI
{
    public class Menu
    {
        public void ShowMainMenu(UserRole role)
        {
            //Console.Clear();
            Console.WriteLine("\n\n=== DocMaster ===");

            if(role == UserRole.Viewer)
            {
                Console.WriteLine("1. View document");
                Console.WriteLine("2. System settings");
                Console.WriteLine("3. Exit");
            }

            if (role == UserRole.Editor)
            {
                Console.WriteLine("1. Create document");
                Console.WriteLine("2. Open document");
                Console.WriteLine("3. Delete document");
                Console.WriteLine("4. Edit document");
                Console.WriteLine("5. Word search");
                Console.WriteLine("6. Save document");
                Console.WriteLine("7. System settings");
                Console.WriteLine("8. History of changes");
                Console.WriteLine("9. Exit");
            }

            if (role == UserRole.Admin)
            {
                Console.WriteLine("1. Create document");
                Console.WriteLine("2. Open document");
                Console.WriteLine("3. Delete document");
                Console.WriteLine("4. Edit document");
                Console.WriteLine("5. Word search");
                Console.WriteLine("6. Save document");
                Console.WriteLine("7. System settings");
                Console.WriteLine("8. User management");
                Console.WriteLine("9. History of changes");
                Console.WriteLine("10. Exit");
            }

            Console.Write("Choice: ");
        }
        public void ShowAdminMenu()
        {
            Console.WriteLine("\n=== Admin Dashboard ===");
            Console.WriteLine("1. Change user role");
            Console.WriteLine("2. View all users");
            Console.WriteLine("3. Block document for user");
            Console.WriteLine("4. Unblock document for user");
            Console.WriteLine("5. View blocked documents");
            Console.Write("Choice: ");
        }
        public void ShowSaveMenu()
        {
            Console.WriteLine("\n=== Save Options ===");
            Console.WriteLine("1. Save in current format");
            Console.WriteLine("2. Save in another format");
            Console.WriteLine("3. Select storage location");
            Console.Write("Choice: ");


        }
    }
}

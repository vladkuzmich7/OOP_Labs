using Newtonsoft.Json;
using System.Text.Json;

namespace OOPsl.UserFunctions
{
    public class UserManager
    {
        private List<User> users = new List<User>();
        private readonly string usersFile;

        private const string DefaultUsersFile = @"D:\OOP\LR2\OOPsl\OOPsl\Files\users.json";

        public UserManager() : this(DefaultUsersFile) { }

        public UserManager(string filePath)
        {
            usersFile = filePath;
            LoadUsers();
        }

        public void AddUser(User user)
        {
            users.Add(user);
            SaveUsers();
        }

        public void RemoveUser(User user)
        {
            users.Remove(user);
            SaveUsers();
        }

        public User FindUser(string userName)
        {
            return users.FirstOrDefault(u => u.Name.Equals(userName, StringComparison.OrdinalIgnoreCase));
        }

        public List<User> GetUsers()
        {
            return users;
        }

        private void SaveUsers()
        {
            try
            {
                string dir = Path.GetDirectoryName(usersFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string json = JsonConvert.SerializeObject(users, Newtonsoft.Json.Formatting.Indented,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                File.WriteAllText(usersFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сохранения пользователей: " + ex.Message);
            }
        }

        private void LoadUsers()
        {
            if (File.Exists(usersFile))
            {
                try
                {
                    string json = File.ReadAllText(usersFile);
                    users = JsonConvert.DeserializeObject<List<User>>(json,
                        new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }) ?? new List<User>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка загрузки пользователей: " + ex.Message);
                    users = new List<User>();
                }
            }
        }
    }
}
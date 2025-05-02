using OOPsl.DocumentFunctions;
using OOPsl.DocumentFunctions.Managers;
using System.Text.Json.Serialization;

namespace OOPsl.UserFunctions
{
    public class RegularUser : User
    {
        public RegularUser(string name) : base(name) { }

        public void ShowNotifications()
        {
            Console.WriteLine($"Уведомления для {Name}:");
            foreach (var note in Notifications)
            {
                Console.WriteLine($"- {note}");
            }
        }
    }
}
using DocMaster.Roles.Observers;

namespace DocMaster.Models
{
    public class User : IRoleChangeObserver, IDocumentChangeObserver
    {
        public string Username { get; }
        public UserRole CurrentRole { get; private set; }
        public List<string> Notifications { get; } = new List<string>();

        public User(string username, UserRole initialRole)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            CurrentRole = initialRole;
        }
        public void SetRole(UserRole newRole, RoleChangeNotifier notifier)
        {
            var oldRole = CurrentRole;
            CurrentRole = newRole;
            notifier.Notify(this, newRole);
        }
        public void OnRoleChanged(User changedUser, UserRole newRole)
        {
            //if (changedUser.Username == this.Username)
            //{
            //    Console.WriteLine($"[SYSTEM] Your role was changed to {newRole}");
            //}
            if (this.CurrentRole == UserRole.Admin)
            {
                Console.WriteLine($"[ADMIN NOTIFICATION] User {changedUser.Username} role changed to {newRole}");
            }
        }
        //public void Update(Document document, string action, string userName)
        //{
        //    if (CurrentRole == UserRole.Admin || CurrentRole == UserRole.Editor)
        //    {
        //        Console.WriteLine($"[{DateTime.Now:T}] User {userName} performed: {action}");
        //    }
        //}
        public void OnDocumentChanged(Document document, string editedBy)
        {
            if (CurrentRole == UserRole.Editor || CurrentRole == UserRole.Admin)
            {
                string notification = $"[{DateTime.Now}] Document '{document.Name}' edited by {editedBy}.";
                Notifications.Add(notification);
                Console.WriteLine(notification);
            }
        }
    }
}

using DocMaster.Models;
using DocMaster.Roles.Observers;

namespace DocMaster.Services
{
    public class UserManager
    {
        private readonly List<User> _users = new();
        private readonly RoleChangeNotifier _roleNotifier = new();
        private readonly BlockedDocumentManager _blockManager = new();
        private readonly DocumentChangeNotifier _documentNotifier = new();
        //private readonly DocumentManager _documentManager;

        public void AddUser(User user)
        {
            _users.Add(user);
            _roleNotifier.Subscribe(user);
            _documentNotifier.Subscribe(user);
        }

        public void ChangeUserRole(string username, UserRole newRole)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if(user != null)
            {
                user?.SetRole(newRole, _roleNotifier);
            }
        }
        public IEnumerable<User> GetAllUsers() => _users.AsReadOnly();
        public User GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        public void BlockDocumentForUser(string filePath, string username)
        {
            _blockManager.BlockDocument(filePath, username);
        }

        public void UnblockDocumentForUser(string filePath, string username)
        {
            _blockManager.UnblockDocument(filePath, username);
        }

        public List<DocumentBlock> GetBlockedDocuments()
        {
            return _blockManager.GetAllBlocks();
        }
    }
}

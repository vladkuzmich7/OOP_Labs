using DocMaster.Roles.Observers;
using DocMaster.Roles.Strategies;

namespace DocMaster.Roles
{
    public class RoleContext 
    {
        private IRoleStrategy _strategy;
        private readonly RoleChangeNotifier _notifier = new();

        public UserRole CurrentRole { get; private set; }

        public RoleContext()
        {
            
            SetRole(UserRole.Viewer);
        }
        public void SetRole(UserRole newRole)
        {
            CurrentRole = newRole;
            _strategy = CreateStrategy(newRole);
        }
        private IRoleStrategy CreateStrategy(UserRole role) => role switch
        {
            UserRole.Editor => new EditorStrategy(),
            UserRole.Admin => new AdminStrategy(),
            _ => new ViewerStrategy()
        };
        
        public bool CanEditDocument => _strategy.CanEdit;
        public bool CanSaveDocument => _strategy.CanSave;
        public bool CanManageUsers => _strategy.CanManageUsers;
    }
}

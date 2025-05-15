using DocMaster.Models;

namespace DocMaster.Roles.Observers
{
    public interface IRoleChangeObserver
    {
        void OnRoleChanged(User user, UserRole newRole);
    }
}

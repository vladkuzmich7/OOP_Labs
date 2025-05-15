namespace DocMaster.Roles.Strategies
{
    public interface IRoleStrategy
    {
        bool CanEdit { get; }
        bool CanSave { get; }
        bool CanManageUsers { get; }
        void HandleRoleChange(UserRole newRole);

    }
}

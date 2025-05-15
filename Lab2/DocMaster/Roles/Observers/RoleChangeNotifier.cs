using DocMaster.Models;

namespace DocMaster.Roles.Observers
{
    public class RoleChangeNotifier
    {
        private readonly List<IRoleChangeObserver> _observers = new();

        public void Subscribe(IRoleChangeObserver observer)
        {
            _observers.Add(observer);
        }
        public void Unsubscribe(IRoleChangeObserver observer)
        {
            _observers.Remove(observer);
        }
        public void Notify(User user, UserRole newRole)
        {
            foreach(var observer in _observers)
            {
                observer.OnRoleChanged(user, newRole);
            }
        }

    }
}

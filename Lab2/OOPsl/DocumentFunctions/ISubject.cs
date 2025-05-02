using OOPsl.UserFunctions;

namespace OOPsl.DocumentFunctions
{
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify();
    }
}

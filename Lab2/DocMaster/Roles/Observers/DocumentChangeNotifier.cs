using DocMaster.Models;

namespace DocMaster.Roles.Observers
{
    public class DocumentChangeNotifier
    {
        private readonly List<IDocumentChangeObserver> _observers = new();

        public void Subscribe(IDocumentChangeObserver observer)
        {
            _observers.Add(observer);
        }
        public void Unsubscribe(IDocumentChangeObserver observer)
        {
            _observers.Remove(observer);
        }
        public void Notify(Document doc, string editedBy)
        {
            foreach (var observer in _observers)
            {
                observer.OnDocumentChanged(doc, editedBy);
            }
        }
    }
}

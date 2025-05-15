using DocMaster.Models;

namespace DocMaster.Roles.Observers
{
    public interface IDocumentChangeObserver
    {
        void OnDocumentChanged(Document document, string editedBy);
    }
}

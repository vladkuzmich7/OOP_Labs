using DocMaster.Models;

namespace DocMaster.Services
{
    public static class DocumentHistoryService
    {
        private static readonly List<DocumentChangeRecord> _history = new();

        public static void AddRecord(Document document, string user, string changeType)
        {
            _history.Add(new DocumentChangeRecord(document.Name, user, changeType));
        }

        public static IEnumerable<DocumentChangeRecord> GetFullHistory() => _history.AsReadOnly();

        public static IEnumerable<DocumentChangeRecord> GetUserHistory(string username) =>
            _history.Where(r => r.EditedBy == username);
    }
}

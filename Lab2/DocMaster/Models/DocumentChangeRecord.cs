
namespace DocMaster.Models
{
    public class DocumentChangeRecord
    {
        public string DocumentName { get; }
        public string EditedBy { get; }
        public DateTime ChangeTime { get; }
        public string ChangeType { get; }

        public DocumentChangeRecord(string docName, string user, string changeType)
        {
            DocumentName = docName;
            EditedBy = user;
            ChangeTime = DateTime.Now;
            ChangeType = changeType;
        }

        public override string ToString() =>
            $"[{ChangeTime:HH:mm:ss}] {EditedBy} {ChangeType} in the document '{DocumentName}'";
    }
}

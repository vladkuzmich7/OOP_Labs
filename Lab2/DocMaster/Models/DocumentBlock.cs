
namespace DocMaster.Models
{
    public class DocumentBlock
    {
        public string FilePath { get; set; }
        public List<string> BlockedUsers { get; set; } = new();
    }
}

using DocMaster.Models;
using System.Text.Json;

namespace DocMaster.Services
{
    public class BlockedDocumentManager
    {
        private const string BlockedFileName = "blocked_documents.json";
        private List<DocumentBlock> _blockedDocuments = new();

        public BlockedDocumentManager()
        {
            LoadBlockedDocuments();
        }

        public void BlockDocument(string filePath, string username)
        {
            var existing = _blockedDocuments.FirstOrDefault(b => b.FilePath == filePath);
            if (existing == null)
            {
                existing = new DocumentBlock { FilePath = filePath };
                _blockedDocuments.Add(existing);
            }

            if (!existing.BlockedUsers.Contains(username))
            {
                existing.BlockedUsers.Add(username);
            }

            SaveBlockedDocuments();
        }

        public void UnblockDocument(string filePath, string username)
        {
            var existing = _blockedDocuments.FirstOrDefault(b => b.FilePath == filePath);
            if (existing != null && existing.BlockedUsers.Contains(username))
            {
                existing.BlockedUsers.Remove(username);
                if (!existing.BlockedUsers.Any())
                {
                    _blockedDocuments.Remove(existing);
                }
                SaveBlockedDocuments();
            }
        }

        public bool IsDocumentBlocked(string filePath, string username)
        {
            return _blockedDocuments
                .Any(b => b.FilePath == filePath && b.BlockedUsers.Contains(username));
        }

        public List<DocumentBlock> GetAllBlocks()
        {
            return _blockedDocuments;
        }

        private void LoadBlockedDocuments()
        {
            if (File.Exists(BlockedFileName))
            {
                var json = File.ReadAllText(BlockedFileName);
                _blockedDocuments = JsonSerializer.Deserialize<List<DocumentBlock>>(json) ?? new();
            }
        }

        private void SaveBlockedDocuments()
        {
            var json = JsonSerializer.Serialize(_blockedDocuments);
            File.WriteAllText(BlockedFileName, json);
        }
    }
}

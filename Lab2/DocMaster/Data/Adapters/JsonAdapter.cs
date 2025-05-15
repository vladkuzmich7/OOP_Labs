using DocMaster.Models;

namespace DocMaster.Data.Adapters
{
    public class JsonAdapter : IDocumentAdapter
    {
        public string Convert(Document doc)
        {
            return $$"""
            {
                "name": "{{doc.Name}}",
                "format": "{{doc.Format}}",
                "content": "{{EscapeJson(doc.Content)}}"
            }
            """;
        }

        private string EscapeJson(string content)
            => content.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n");
    }
}

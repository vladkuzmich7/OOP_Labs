using DocMaster.Models;

namespace DocMaster.Data.Adapters
{
    public class XmlAdapter : IDocumentAdapter
    {
        public string Convert(Document doc)
        {
            return $"""
            <Document>
                <Name>{doc.Name}</Name>
                <Format>{doc.Format}</Format>
                <Content><![CDATA[{doc.Content}]]></Content>
            </Document>
            """;
        }
    }
}

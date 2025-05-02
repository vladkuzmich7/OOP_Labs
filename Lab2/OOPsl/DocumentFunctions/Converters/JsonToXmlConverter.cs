using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace OOPsl.DocumentFunctions.Converters
{
    public class JsonToXmlConverter : IFormatConverter
    {
        public string Convert(string input)
        {
            JObject jObj;
            try
            {
                jObj = JObject.Parse(input);
            }
            catch (JsonReaderException ex)
            {
                throw new FormatException("Некорректный JSON формат", ex);
            }

            try
            {
                XDocument xDoc;
                if (jObj.Count == 1)
                {
                    string rootName = jObj.Properties().First().Name;
                    xDoc = JsonConvert.DeserializeXNode(input, rootName);
                    return xDoc.Root.ToString();
                }
                else
                {
                    xDoc = JsonConvert.DeserializeXNode(input, "root");
                    return xDoc.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Ошибка конвертации JSON → XML: {ex.Message}", ex);
            }
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Xml.Linq;

namespace OOPsl.DocumentFunctions.Converters
{
    public class XmlToJsonConverter : IFormatConverter
    {
        public string Convert(string input)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(input);
            }
            catch (System.Xml.XmlException ex)
            {
                throw new FormatException("Некорректный XML формат", ex);
            }

            try
            {
                var root = doc.Root;
                if (!root.HasElements && !root.Attributes().Any())
                {
                    var jsonObject = new JObject();
                    jsonObject[root.Name.LocalName] = root.Value;
                    return jsonObject.ToString(Formatting.Indented);
                }
                else
                {
                    string json;
                    if (root.Name.LocalName == "root" && root.Elements().Count() == 1)
                    {
                        var realRoot = root.Elements().First();
                        json = JsonConvert.SerializeXNode(realRoot, Formatting.Indented, omitRootObject: false);
                    }
                    else
                    {
                        json = JsonConvert.SerializeXNode(doc, Formatting.Indented, omitRootObject: true);
                    }
                    return json;
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Ошибка конвертации XML → JSON: {ex.Message}", ex);
            }
        }
    }
}
using System;
using System.Text;
using System.Text.Json;

namespace OOPsl.DocumentFunctions.Displays
{
    public class JsonDisplayStrategy : IDisplayStrategy
    {
        private const string Reset = "\x1b[0m";
        private const string KeyColor = "\x1b[38;5;33m";    // Синий
        private const string StringColor = "\x1b[38;5;40m"; // Зеленый
        private const string NumberColor = "\x1b[38;5;208m";// Оранжевый
        private const string BoolColor = "\x1b[38;5;199m";  // Розовый
        private const string NullColor = "\x1b[38;5;240m";  // Серый

        public void Display(Document document)
        {
            try
            {
                var jsonDoc = JsonDocument.Parse(document.Content);
                var output = new StringBuilder();
                FormatJsonElement(jsonDoc.RootElement, output, 0);
                Console.WriteLine(output.ToString());
            }
            catch (JsonException)
            {
                Console.WriteLine("Invalid JSON format");
            }
        }

        private void FormatJsonElement(JsonElement element, StringBuilder output, int indent)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    FormatObject(element, output, indent);
                    break;
                case JsonValueKind.Array:
                    FormatArray(element, output, indent);
                    break;
                default:
                    FormatValue(element, output);
                    break;
            }
        }

        private void FormatObject(JsonElement obj, StringBuilder output, int indent)
        {
            output.Append("{\n");
            var first = true;

            foreach (var property in obj.EnumerateObject())
            {
                if (!first) output.Append(",\n");
                first = false;

                output.Append($"{GetIndent(indent + 1)}{KeyColor}\"{property.Name}\"{Reset}: ");

                FormatJsonElement(property.Value, output, indent + 1);
            }

            output.Append($"\n{GetIndent(indent)}}}");
        }

        private void FormatArray(JsonElement array, StringBuilder output, int indent)
        {
            output.Append("[\n");
            var first = true;

            foreach (var element in array.EnumerateArray())
            {
                if (!first) output.Append(",\n");
                first = false;

                output.Append(GetIndent(indent + 1));
                FormatJsonElement(element, output, indent + 1);
            }

            output.Append($"\n{GetIndent(indent)}]");
        }

        private void FormatValue(JsonElement value, StringBuilder output)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    output.Append($"{StringColor}\"{value}\"{Reset}");
                    break;
                case JsonValueKind.Number:
                    output.Append($"{NumberColor}{value}{Reset}");
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    output.Append($"{BoolColor}{value.ToString().ToLower()}{Reset}");
                    break;
                case JsonValueKind.Null:
                    output.Append($"{NullColor}null{Reset}");
                    break;
                default:
                    output.Append(value);
                    break;
            }
        }

        private string GetIndent(int level) => new string(' ', level * 4);
    }
}
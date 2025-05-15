using DocMaster.Models;
using Microsoft.Data.Sqlite;

namespace DocMaster.Data.StorageStrategies
{
    public class DatabaseStorageStrategy : IStorageStrategy
    {
        private readonly string _dbPath;

        public DatabaseStorageStrategy()
        {
            _dbPath = Path.Combine(Environment.CurrentDirectory, "DocMaster.db");
        }

        public void Save(Document document, DocumentFormat targetFormat)
        {
            string content = document.ConvertTo(targetFormat);

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            const string query = @"
                INSERT INTO Documents (Name, Format, Content)
                VALUES (@name, @format, @content)";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@name", document.Name);
            command.Parameters.AddWithValue("@format", targetFormat.ToString());
            command.Parameters.AddWithValue("@content", content);

            command.ExecuteNonQuery();
        }
    }
}

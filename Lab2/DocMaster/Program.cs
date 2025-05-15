using DocMaster.UI;
using Microsoft.Data.Sqlite;
using System.Runtime.InteropServices;
public static class Program
{
    [DllImport("D:\\OOP\\LR2\\DocMaster\\bin\\Debug\\net8.0\\setfont.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setFontSize(int fontSize);

    public static void InitializeDatabase()
    {
        // Path to db
        string dbPath = Path.Combine(Environment.CurrentDirectory, "DocMaster.db");

        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        // Create table
        var command = connection.CreateCommand();
        command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Documents (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Format TEXT NOT NULL,
            Content TEXT NOT NULL,
            CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
        )";
        command.ExecuteNonQuery();
    }
    public static void Main(string[] args)
    {
        InitializeDatabase();
        App app = new App();
        app.Run();
    }
}
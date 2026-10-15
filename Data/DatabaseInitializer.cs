using System;
using System.IO;
using System.Data.SQLite;

namespace DnDesktop.Data
{
    public static class DatabaseInitializer
    {
        private static readonly string AppDataFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "DnDesktop");

        public static string DatabasePath
        {
            get
            {
                return Path.Combine(
                    AppDataFolder,
                    "DnDesktop.db");
            }
        }

        public static void Initialize()
        {
            // Create the application data directory if needed.
            Directory.CreateDirectory(AppDataFolder);

            // Create the database file automatically if missing.
            if (!File.Exists(DatabasePath))
            {
                SQLiteConnection.CreateFile(DatabasePath);
            }

            string connectionString =
                "Data Source=" + DatabasePath + ";Version=3;";

            using (SQLiteConnection connection =
                   new SQLiteConnection(connectionString))
            {
                connection.Open();

                string createCharactersTable = @"
                    CREATE TABLE IF NOT EXISTS Characters
                    (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        PlayerName TEXT,
                        Race TEXT,
                        Subrace TEXT,
                        CharacterClass TEXT,
                        Subclass TEXT,
                        Level INTEGER NOT NULL DEFAULT 1,
                        Background TEXT,
                        CreatedDate TEXT NOT NULL,
                        LastModified TEXT NOT NULL
                    );";

                using (SQLiteCommand command =
                       new SQLiteCommand(
                           createCharactersTable,
                           connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    internal static class SQLiteFileExtensions
    {
        public static void CreateFileIfNotExists(
            string databasePath)
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
            }
        }
    }
}

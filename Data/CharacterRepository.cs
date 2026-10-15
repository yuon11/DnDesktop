
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class CharacterRepository
    {
        private readonly string _connectionString;

        public CharacterRepository()
        {
            _connectionString =
                "Data Source=" +
                DatabaseInitializer.DatabasePath +
                ";Version=3;";
        }

        public List<Character> GetAll()
        {
            var characters = new List<Character>();

            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    SELECT Id, Name, PlayerName, Race, Subrace,
                           CharacterClass, Subclass, Level,
                           Background, CreatedDate, LastModified
                    FROM Characters
                    ORDER BY Name;";

                using (var command =
                       new SQLiteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var character = new Character
                        {
                            Id = Guid.Parse(
                                reader["Id"].ToString()),

                            Name = reader["Name"].ToString(),
                            PlayerName = reader["PlayerName"].ToString(),
                            Race = reader["Race"].ToString(),
                            Subrace = reader["Subrace"].ToString(),
                            CharacterClass =
                                reader["CharacterClass"].ToString(),
                            Subclass = reader["Subclass"].ToString(),

                            Level = Convert.ToInt32(
                                reader["Level"]),

                            Background =
                                reader["Background"].ToString(),

                            CreatedDate = DateTime.Parse(
                                reader["CreatedDate"].ToString(),
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind),

                            LastModified = DateTime.Parse(
                                reader["LastModified"].ToString(),
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.RoundtripKind)
                        };

                        characters.Add(character);
                    }
                }
            }

            return characters;
        }

        public void Add(Character character)
        {
            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    INSERT INTO Characters
                    (
                        Id, Name, PlayerName, Race, Subrace,
                        CharacterClass, Subclass, Level,
                        Background, CreatedDate, LastModified
                    )
                    VALUES
                    (
                        @Id, @Name, @PlayerName, @Race, @Subrace,
                        @CharacterClass, @Subclass, @Level,
                        @Background, @CreatedDate, @LastModified
                    );";

                using (var command =
                       new SQLiteCommand(sql, connection))
                {
                    AddParameters(command, character);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(Character character)
        {
            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    UPDATE Characters
                    SET Name = @Name,
                        PlayerName = @PlayerName,
                        Race = @Race,
                        Subrace = @Subrace,
                        CharacterClass = @CharacterClass,
                        Subclass = @Subclass,
                        Level = @Level,
                        Background = @Background,
                        LastModified = @LastModified
                    WHERE Id = @Id;";

                using (var command =
                       new SQLiteCommand(sql, connection))
                {
                    AddParameters(command, character);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException(
                            "The character could not be found in the database.");
                    }
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql =
                    "DELETE FROM Characters WHERE Id = @Id;";

                using (var command =
                       new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id", id.ToString());

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new InvalidOperationException(
                            "The character could not be found in the database.");
                    }
                }
            }
        }

        private static void AddParameters(
            SQLiteCommand command,
            Character character)
        {
            command.Parameters.AddWithValue(
                "@Id", character.Id.ToString());

            command.Parameters.AddWithValue(
                "@Name", character.Name ?? "");

            command.Parameters.AddWithValue(
                "@PlayerName", character.PlayerName ?? "");

            command.Parameters.AddWithValue(
                "@Race", character.Race ?? "");

            command.Parameters.AddWithValue(
                "@Subrace", character.Subrace ?? "");

            command.Parameters.AddWithValue(
                "@CharacterClass",
                character.CharacterClass ?? "");

            command.Parameters.AddWithValue(
                "@Subclass", character.Subclass ?? "");

            command.Parameters.AddWithValue(
                "@Level", character.Level);

            command.Parameters.AddWithValue(
                "@Background", character.Background ?? "");

            command.Parameters.AddWithValue(
                "@CreatedDate",
                character.CreatedDate.ToString("o"));

            command.Parameters.AddWithValue(
                "@LastModified",
                character.LastModified.ToString("o"));
        }
    }
}


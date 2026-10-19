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
                ";Version=3;Foreign Keys=True;";
        }

        public List<Character> GetAll()
        {
            var characters = new List<Character>();

            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
                    SELECT
                        Id, Name, PlayerName, Race, Subrace,
                        CharacterClass, Subclass, Level,
                        Background, CreatedDate, LastModified,
                        Strength, Dexterity, Constitution,
                        Intelligence, Wisdom, Charisma,
                        ArmorClass, Initiative, MaxHP,
                        CurrentHP, TemporaryHP, Speed, HitDice
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
                                Convert.ToString(reader["Id"])),

                            Name = ReadString(reader, "Name"),
                            PlayerName = ReadString(reader, "PlayerName"),
                            Race = ReadString(reader, "Race"),
                            Subrace = ReadString(reader, "Subrace"),
                            CharacterClass = ReadString(reader, "CharacterClass"),
                            Subclass = ReadString(reader, "Subclass"),
                            Level = ReadInt(reader, "Level", 1),
                            Background = ReadString(reader, "Background"),

                            CreatedDate = ReadDateTime(reader, "CreatedDate"),
                            LastModified = ReadDateTime(reader, "LastModified"),

                            AbilityScores = new AbilityScores
                            {
                                Strength = ReadInt(reader, "Strength", 10),
                                Dexterity = ReadInt(reader, "Dexterity", 10),
                                Constitution = ReadInt(reader, "Constitution", 10),
                                Intelligence = ReadInt(reader, "Intelligence", 10),
                                Wisdom = ReadInt(reader, "Wisdom", 10),
                                Charisma = ReadInt(reader, "Charisma", 10)
                            },

                            Combat = new CombatStats
                            {
                                ArmorClass = ReadInt(reader, "ArmorClass", 10),
                                Initiative = ReadInt(reader, "Initiative", 0),
                                MaxHitPoints = ReadInt(reader, "MaxHP", 10),
                                CurrentHitPoints = ReadInt(reader, "CurrentHP", 10),
                                TemporaryHitPoints = ReadInt(reader, "TemporaryHP", 0),
                                Speed = ReadInt(reader, "Speed", 30),
                                HitDice = ReadInt(reader, "HitDice", 1)
                            },

                            Feats = new List<Feat>()
                        };

                        characters.Add(character);
                    }
                }

                // Load feats after the character reader has closed.
                foreach (Character character in characters)
                {
                    character.Feats = GetFeats(
                        connection,
                        character.Id);
                }
            }

            return characters;
        }

        public void Add(Character character)
        {
            EnsureNestedObjects(character);

            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
                            INSERT INTO Characters
                            (
                                Id, Name, PlayerName, Race, Subrace,
                                CharacterClass, Subclass, Level,
                                Background, CreatedDate, LastModified,
                                Strength, Dexterity, Constitution,
                                Intelligence, Wisdom, Charisma,
                                ArmorClass, Initiative, MaxHP,
                                CurrentHP, TemporaryHP, Speed, HitDice
                            )
                            VALUES
                            (
                                @Id, @Name, @PlayerName, @Race, @Subrace,
                                @CharacterClass, @Subclass, @Level,
                                @Background, @CreatedDate, @LastModified,
                                @Strength, @Dexterity, @Constitution,
                                @Intelligence, @Wisdom, @Charisma,
                                @ArmorClass, @Initiative, @MaxHP,
                                @CurrentHP, @TemporaryHP, @Speed, @HitDice
                            );";

                        using (var command = new SQLiteCommand(
                            sql, connection, transaction))
                        {
                            AddParameters(command, character);
                            command.ExecuteNonQuery();
                        }

                        SaveFeats(
                            connection,
                            transaction,
                            character);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Update(Character character)
        {
            EnsureNestedObjects(character);

            using (var connection =
                   new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string sql = @"
                            UPDATE Characters
                            SET
                                Name = @Name,
                                PlayerName = @PlayerName,
                                Race = @Race,
                                Subrace = @Subrace,
                                CharacterClass = @CharacterClass,
                                Subclass = @Subclass,
                                Level = @Level,
                                Background = @Background,

                                Strength = @Strength,
                                Dexterity = @Dexterity,
                                Constitution = @Constitution,
                                Intelligence = @Intelligence,
                                Wisdom = @Wisdom,
                                Charisma = @Charisma,

                                ArmorClass = @ArmorClass,
                                Initiative = @Initiative,
                                MaxHP = @MaxHP,
                                CurrentHP = @CurrentHP,
                                TemporaryHP = @TemporaryHP,
                                Speed = @Speed,
                                HitDice = @HitDice,

                                LastModified = @LastModified
                            WHERE Id = @Id;";

                        using (var command = new SQLiteCommand(
                            sql, connection, transaction))
                        {
                            AddParameters(command, character);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                throw new InvalidOperationException(
                                    "The character could not be found in the database.");
                            }
                        }

                        // Replace the stored feat list with the current list.
                        using (var command = new SQLiteCommand(
                            "DELETE FROM CharacterFeats WHERE CharacterId = @CharacterId;",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@CharacterId",
                                character.Id.ToString());

                            command.ExecuteNonQuery();
                        }

                        SaveFeats(
                            connection,
                            transaction,
                            character);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
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

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete child records explicitly.
                        using (var command = new SQLiteCommand(
                            "DELETE FROM CharacterFeats WHERE CharacterId = @Id;",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@Id",
                                id.ToString());

                            command.ExecuteNonQuery();
                        }

                        using (var command = new SQLiteCommand(
                            "DELETE FROM Characters WHERE Id = @Id;",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@Id",
                                id.ToString());

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                throw new InvalidOperationException(
                                    "The character could not be found in the database.");
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static List<Feat> GetFeats(
            SQLiteConnection connection,
            Guid characterId)
        {
            var feats = new List<Feat>();

            const string sql = @"
                SELECT Name, Description
                FROM CharacterFeats
                WHERE CharacterId = @CharacterId
                ORDER BY Name;";

            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CharacterId",
                    characterId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        feats.Add(new Feat
                        {
                            Name = ReadString(reader, "Name"),
                            Description = ReadString(reader, "Description")
                        });
                    }
                }
            }

            return feats;
        }

        private static void SaveFeats(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            Character character)
        {
            if (character.Feats == null)
                return;

            const string sql = @"
                INSERT INTO CharacterFeats
                    (CharacterId, Name, Description)
                VALUES
                    (@CharacterId, @Name, @Description);";

            foreach (Feat feat in character.Feats)
            {
                if (feat == null ||
                    string.IsNullOrWhiteSpace(feat.Name))
                {
                    continue;
                }

                using (var command = new SQLiteCommand(
                    sql, connection, transaction))
                {
                    command.Parameters.AddWithValue(
                        "@CharacterId",
                        character.Id.ToString());

                    command.Parameters.AddWithValue(
                        "@Name",
                        feat.Name.Trim());

                    command.Parameters.AddWithValue(
                        "@Description",
                        feat.Description ?? "");

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AddParameters(
            SQLiteCommand command,
            Character character)
        {
            command.Parameters.AddWithValue("@Id", character.Id.ToString());
            command.Parameters.AddWithValue("@Name", character.Name ?? "");
            command.Parameters.AddWithValue("@PlayerName", character.PlayerName ?? "");
            command.Parameters.AddWithValue("@Race", character.Race ?? "");
            command.Parameters.AddWithValue("@Subrace", character.Subrace ?? "");
            command.Parameters.AddWithValue("@CharacterClass", character.CharacterClass ?? "");
            command.Parameters.AddWithValue("@Subclass", character.Subclass ?? "");
            command.Parameters.AddWithValue("@Level", character.Level);
            command.Parameters.AddWithValue("@Background", character.Background ?? "");

            command.Parameters.AddWithValue(
                "@CreatedDate",
                character.CreatedDate.ToString("o"));

            command.Parameters.AddWithValue(
                "@LastModified",
                character.LastModified.ToString("o"));

            command.Parameters.AddWithValue(
                "@Strength", character.AbilityScores.Strength);
            command.Parameters.AddWithValue(
                "@Dexterity", character.AbilityScores.Dexterity);
            command.Parameters.AddWithValue(
                "@Constitution", character.AbilityScores.Constitution);
            command.Parameters.AddWithValue(
                "@Intelligence", character.AbilityScores.Intelligence);
            command.Parameters.AddWithValue(
                "@Wisdom", character.AbilityScores.Wisdom);
            command.Parameters.AddWithValue(
                "@Charisma", character.AbilityScores.Charisma);

            command.Parameters.AddWithValue(
                "@ArmorClass", character.Combat.ArmorClass);
            command.Parameters.AddWithValue(
                "@Initiative", character.Combat.Initiative);
            command.Parameters.AddWithValue(
                "@MaxHP", character.Combat.MaxHitPoints);
            command.Parameters.AddWithValue(
                "@CurrentHP", character.Combat.CurrentHitPoints);
            command.Parameters.AddWithValue(
                "@TemporaryHP", character.Combat.TemporaryHitPoints);
            command.Parameters.AddWithValue(
                "@Speed", character.Combat.Speed);
            command.Parameters.AddWithValue(
                "@HitDice",
                character.Combat.HitDice.ToString(
                    CultureInfo.InvariantCulture));
        }

        private static void EnsureNestedObjects(Character character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            if (character.AbilityScores == null)
                character.AbilityScores = new AbilityScores();

            if (character.Combat == null)
                character.Combat = new CombatStats();

            if (character.Feats == null)
                character.Feats = new List<Feat>();
        }

        private static string ReadString(
            SQLiteDataReader reader,
            string column)
        {
            if (reader[column] == DBNull.Value)
                return string.Empty;

            return Convert.ToString(reader[column]);
        }

        private static int ReadInt(
            SQLiteDataReader reader,
            string column,
            int defaultValue)
        {
            if (reader[column] == DBNull.Value)
                return defaultValue;

            int value;

            if (int.TryParse(
                Convert.ToString(reader[column]),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out value))
            {
                return value;
            }

            return defaultValue;
        }

        private static DateTime ReadDateTime(
            SQLiteDataReader reader,
            string column)
        {
            DateTime value;

            if (DateTime.TryParse(
                ReadString(reader, column),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out value))
            {
                return value;
            }

            return DateTime.Now;
        }
    }
}
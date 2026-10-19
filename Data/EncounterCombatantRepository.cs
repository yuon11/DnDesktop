
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DnDesktop.Data
{
    public class EncounterCombatantRepository
    {
        private readonly string _connectionString;

        public EncounterCombatantRepository()
        {
            _connectionString =
                "Data Source=" +
                DatabaseInitializer.DatabasePath +
                ";Version=3;Foreign Keys=True;";
        }

        public List<EncounterCombatant> GetByEncounterId(
            Guid encounterId)
        {
            var combatants = new List<EncounterCombatant>();

            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                const string sql = @"
                    SELECT
                        Id,
                        EncounterId,
                        ParticipantType,
                        SourceId,
                        DisplayName,
                        Initiative,
                        CurrentHP,
                        MaxHP,
                        ArmorClass,
                        Conditions,
                        IsDefeated,
                        TurnOrder
                    FROM EncounterCombatants
                    WHERE EncounterId = @EncounterId
                    ORDER BY TurnOrder, Initiative DESC;";

                using (var command =
                    new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@EncounterId",
                        encounterId.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            combatants.Add(ReadCombatant(reader));
                        }
                    }
                }
            }

            return combatants;
        }

        public EncounterCombatant GetById(Guid id)
        {
            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                const string sql = @"
                    SELECT
                        Id,
                        EncounterId,
                        ParticipantType,
                        SourceId,
                        DisplayName,
                        Initiative,
                        CurrentHP,
                        MaxHP,
                        ArmorClass,
                        Conditions,
                        IsDefeated,
                        TurnOrder
                    FROM EncounterCombatants
                    WHERE Id = @Id;";

                using (var command =
                    new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id", id.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadCombatant(reader);
                        }
                    }
                }
            }

            return null;
        }

        public void Add(EncounterCombatant combatant)
        {
            if (combatant == null)
                throw new ArgumentNullException(nameof(combatant));

            ValidateCombatant(combatant);

            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    ValidateEncounter(
                        connection, transaction,
                        combatant.EncounterId);

                    ValidateSource(
                        connection, transaction,
                        combatant.ParticipantType,
                        combatant.SourceId);

                    const string sql = @"
                        INSERT INTO EncounterCombatants
                        (
                            Id, EncounterId, ParticipantType,
                            SourceId, DisplayName, Initiative,
                            CurrentHP, MaxHP, ArmorClass,
                            Conditions, IsDefeated, TurnOrder
                        )
                        VALUES
                        (
                            @Id, @EncounterId, @ParticipantType,
                            @SourceId, @DisplayName, @Initiative,
                            @CurrentHP, @MaxHP, @ArmorClass,
                            @Conditions, @IsDefeated, @TurnOrder
                        );";

                    using (var command =
                        new SQLiteCommand(sql, connection, transaction))
                    {
                        AddParameters(command, combatant);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public void Update(EncounterCombatant combatant)
        {
            if (combatant == null)
                throw new ArgumentNullException(nameof(combatant));

            ValidateCombatant(combatant);

            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    ValidateEncounter(
                        connection, transaction,
                        combatant.EncounterId);

                    ValidateSource(
                        connection, transaction,
                        combatant.ParticipantType,
                        combatant.SourceId);

                    const string sql = @"
                        UPDATE EncounterCombatants
                        SET
                            EncounterId = @EncounterId,
                            ParticipantType = @ParticipantType,
                            SourceId = @SourceId,
                            DisplayName = @DisplayName,
                            Initiative = @Initiative,
                            CurrentHP = @CurrentHP,
                            MaxHP = @MaxHP,
                            ArmorClass = @ArmorClass,
                            Conditions = @Conditions,
                            IsDefeated = @IsDefeated,
                            TurnOrder = @TurnOrder
                        WHERE Id = @Id;";

                    using (var command =
                        new SQLiteCommand(sql, connection, transaction))
                    {
                        AddParameters(command, combatant);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new InvalidOperationException(
                                "The combatant could not be found.");
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public void Delete(Guid id)
        {
            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();
                EnableForeignKeys(connection);

                const string sql = @"
                    DELETE FROM EncounterCombatants
                    WHERE Id = @Id;";

                using (var command =
                    new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@Id", id.ToString());

                    command.ExecuteNonQuery();
                }
            }
        }

        private static EncounterCombatant ReadCombatant(
            SQLiteDataReader reader)
        {
            return new EncounterCombatant
            {
                Id = Guid.Parse(reader["Id"].ToString()),
                EncounterId = Guid.Parse(
                    reader["EncounterId"].ToString()),
                ParticipantType =
                    reader["ParticipantType"].ToString(),
                SourceId = Guid.Parse(
                    reader["SourceId"].ToString()),
                DisplayName = reader["DisplayName"].ToString(),
                Initiative = Convert.ToInt32(reader["Initiative"]),
                CurrentHP = Convert.ToInt32(reader["CurrentHP"]),
                MaxHP = Convert.ToInt32(reader["MaxHP"]),
                ArmorClass = Convert.ToInt32(reader["ArmorClass"]),
                Conditions = reader["Conditions"] == DBNull.Value
                    ? string.Empty
                    : reader["Conditions"].ToString(),
                IsDefeated = Convert.ToInt32(
                    reader["IsDefeated"]) != 0,
                TurnOrder = Convert.ToInt32(reader["TurnOrder"])
            };
        }

        private static void AddParameters(
            SQLiteCommand command,
            EncounterCombatant combatant)
        {
            command.Parameters.AddWithValue(
                "@Id", combatant.Id.ToString());

            command.Parameters.AddWithValue(
                "@EncounterId", combatant.EncounterId.ToString());

            command.Parameters.AddWithValue(
                "@ParticipantType", combatant.ParticipantType);

            command.Parameters.AddWithValue(
                "@SourceId", combatant.SourceId.ToString());

            command.Parameters.AddWithValue(
                "@DisplayName", combatant.DisplayName);

            command.Parameters.AddWithValue(
                "@Initiative", combatant.Initiative);

            command.Parameters.AddWithValue(
                "@CurrentHP", combatant.CurrentHP);

            command.Parameters.AddWithValue(
                "@MaxHP", combatant.MaxHP);

            command.Parameters.AddWithValue(
                "@ArmorClass", combatant.ArmorClass);

            command.Parameters.AddWithValue(
                "@Conditions",
                combatant.Conditions ?? string.Empty);

            command.Parameters.AddWithValue(
                "@IsDefeated", combatant.IsDefeated ? 1 : 0);

            command.Parameters.AddWithValue(
                "@TurnOrder", combatant.TurnOrder);
        }

        private static void ValidateCombatant(
            EncounterCombatant combatant)
        {
            if (combatant.Id == Guid.Empty)
                throw new ArgumentException("Combatant ID is required.");

            if (combatant.EncounterId == Guid.Empty)
                throw new ArgumentException("Encounter ID is required.");

            if (combatant.SourceId == Guid.Empty)
                throw new ArgumentException("Source ID is required.");

            if (string.IsNullOrWhiteSpace(combatant.DisplayName))
                throw new ArgumentException("A combatant name is required.");

            if (combatant.ParticipantType != "Character" &&
                combatant.ParticipantType != "NPC" &&
                combatant.ParticipantType != "Creature")
            {
                throw new ArgumentException(
                    "Participant type must be Character, NPC, or Creature.");
            }

            if (combatant.CurrentHP < 0 || combatant.MaxHP < 0)
                throw new ArgumentException("HP cannot be negative.");

            if (combatant.CurrentHP > combatant.MaxHP)
                throw new ArgumentException(
                    "Current HP cannot exceed maximum HP.");
        }

        private static void ValidateEncounter(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            Guid encounterId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM SessionEncounters
                WHERE Id = @Id;";

            using (var command =
                new SQLiteCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue(
                    "@Id", encounterId.ToString());

                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    throw new InvalidOperationException(
                        "The encounter does not exist.");
                }
            }
        }

        private static void ValidateSource(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string participantType,
            Guid sourceId)
        {
            // Both player characters and NPCs are stored in Characters.
            string tableName = participantType == "Creature"
                ? "Creatures"
                : "Characters";

            string sql =
                "SELECT COUNT(*) FROM " + tableName +
                " WHERE Id = @Id;";

            using (var command =
                new SQLiteCommand(sql, connection, transaction))
            {
                command.Parameters.AddWithValue(
                    "@Id", sourceId.ToString());

                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    throw new InvalidOperationException(
                        "The selected character or creature no longer exists.");
                }
            }
        }

        private static void EnableForeignKeys(
            SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(
                "PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
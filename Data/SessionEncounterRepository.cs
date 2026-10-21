
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Data.SQLite;

namespace DnDesktop.Data
{
    public class SessionEncounterRepository
    {
        private readonly string _connectionString =
            "Data Source=" + DatabaseInitializer.DatabasePath +
            ";Version=3;Foreign Keys=True;";

        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection =
                new SQLiteConnection(_connectionString);

            connection.Open();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            return connection;
        }

        // Get all encounters belonging to a session.
        public List<SessionEncounter> GetBySessionId(Guid sessionId)
        {
            List<SessionEncounter> encounters =
                new List<SessionEncounter>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT *
                    FROM SessionEncounters
                    WHERE SessionId = @SessionId
                    ORDER BY Name COLLATE NOCASE;";

                command.Parameters.AddWithValue(
                    "@SessionId", sessionId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        encounters.Add(ReadEncounter(reader));
                    }
                }
            }

            return encounters;
        }

        // Get one encounter by its ID.
        public SessionEncounter GetById(Guid id)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT *
                    FROM SessionEncounters
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadEncounter(reader);
                    }
                }
            }

            return null;
        }

        // Create an encounter.
        public void Add(SessionEncounter encounter)
        {
            ValidateEncounter(encounter);

            if (encounter.Id == Guid.Empty)
            {
                encounter.Id = Guid.NewGuid();
            }

            encounter.Name = encounter.Name.Trim();

            if (encounter.CreatedDate == default(DateTime))
            {
                encounter.CreatedDate = DateTime.Now;
            }

            encounter.LastModified = DateTime.Now;

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO SessionEncounters
                    (
                        Id,
                        SessionId,
                        Name,
                        EncounterNotes,
                        CreatedDate,
                        LastModified
                    )
                    VALUES
                    (
                        @Id,
                        @SessionId,
                        @Name,
                        @EncounterNotes,
                        @CreatedDate,
                        @LastModified
                    );";

                AddEncounterParameters(command, encounter);
                command.ExecuteNonQuery();
            }
        }

        // Update an existing encounter.
        public void Update(SessionEncounter encounter)
        {
            ValidateEncounter(encounter);

            if (encounter.Id == Guid.Empty)
            {
                throw new ArgumentException(
                    "The encounter must have a valid ID.");
            }

            encounter.Name = encounter.Name.Trim();
            encounter.LastModified = DateTime.Now;

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE SessionEncounters
                    SET
                        SessionId = @SessionId,
                        Name = @Name,
                        EncounterNotes = @EncounterNotes,
                        LastModified = @LastModified
                    WHERE Id = @Id;";

                AddEncounterParameters(command, encounter);

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "The encounter could not be found.");
                }
            }
        }

        // Delete an encounter and its creature assignments.
        public void Delete(Guid id)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE FROM SessionEncounters
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id.ToString());

                if (command.ExecuteNonQuery() == 0)
                {
                    throw new InvalidOperationException(
                        "The encounter could not be found.");
                }
            }
        }

        // Get the creature IDs and quantities assigned to an encounter.
        public List<SessionEncounterCreature> GetCreaturesByEncounterId(
            Guid encounterId)
        {
            List<SessionEncounterCreature> assignments =
                new List<SessionEncounterCreature>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT EncounterId, CreatureId, Quantity
                    FROM SessionEncounterCreatures
                    WHERE EncounterId = @EncounterId
                    ORDER BY CreatureId;";

                command.Parameters.AddWithValue(
                    "@EncounterId", encounterId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        assignments.Add(
                            new SessionEncounterCreature
                            {
                                EncounterId = Guid.Parse(
                                    reader["EncounterId"].ToString()),

                                CreatureId = Guid.Parse(
                                    reader["CreatureId"].ToString()),

                                Quantity = Convert.ToInt32(
                                    reader["Quantity"])
                            });
                    }
                }
            }

            return assignments;
        }

        // Replace all creature assignments for an encounter.
        // The replacement happens in a single transaction.
        public void SaveCreatures(
            Guid encounterId,
            IEnumerable<SessionEncounterCreature> assignments)
        {
            if (encounterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid encounter ID is required.");
            }

            List<SessionEncounterCreature> items =
                assignments == null
                    ? new List<SessionEncounterCreature>()
                    : assignments.ToList();

            foreach (SessionEncounterCreature item in items)
            {
                if (item.CreatureId == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Every assignment must reference a valid creature.");
                }

                if (item.Quantity <= 0)
                {
                    throw new ArgumentException(
                        "Creature quantities must be greater than zero.");
                }

                item.EncounterId = encounterId;
            }

            if (items.Select(x => x.CreatureId).Distinct().Count()
                != items.Count)
            {
                throw new ArgumentException(
                    "Each creature can only appear once per encounter. " +
                    "Update its quantity instead.");
            }

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    // Confirm that the encounter exists.
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = @"
                            SELECT COUNT(*)
                            FROM SessionEncounters
                            WHERE Id = @Id;";

                        command.Parameters.AddWithValue(
                            "@Id", encounterId.ToString());

                        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                        {
                            throw new InvalidOperationException(
                                "The encounter could not be found.");
                        }
                    }

                    // Validate all creature references before replacing rows.
                    foreach (SessionEncounterCreature item in items)
                    {
                        using (SQLiteCommand command =
                            connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                SELECT COUNT(*)
                                FROM Creatures
                                WHERE Id = @CreatureId;";

                            command.Parameters.AddWithValue(
                                "@CreatureId", item.CreatureId.ToString());

                            if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                            {
                                throw new InvalidOperationException(
                                    "A selected creature no longer exists.");
                            }
                        }
                    }

                    // Remove previous assignments.
                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = @"
                            DELETE FROM SessionEncounterCreatures
                            WHERE EncounterId = @EncounterId;";

                        command.Parameters.AddWithValue(
                            "@EncounterId", encounterId.ToString());

                        command.ExecuteNonQuery();
                    }

                    // Insert the current assignments.
                    foreach (SessionEncounterCreature item in items)
                    {
                        using (SQLiteCommand command =
                            connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = @"
                                INSERT INTO SessionEncounterCreatures
                                (
                                    EncounterId,
                                    CreatureId,
                                    Quantity
                                )
                                VALUES
                                (
                                    @EncounterId,
                                    @CreatureId,
                                    @Quantity
                                );";

                            command.Parameters.AddWithValue(
                                "@EncounterId", encounterId.ToString());

                            command.Parameters.AddWithValue(
                                "@CreatureId", item.CreatureId.ToString());

                            command.Parameters.AddWithValue(
                                "@Quantity", item.Quantity);

                            command.ExecuteNonQuery();
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

        private void ValidateEncounter(SessionEncounter encounter)
        {
            if (encounter == null)
            {
                throw new ArgumentNullException(nameof(encounter));
            }

            if (encounter.SessionId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid campaign session is required.");
            }

            if (string.IsNullOrWhiteSpace(encounter.Name))
            {
                throw new ArgumentException(
                    "An encounter name is required.");
            }
        }

        private void AddEncounterParameters(
            SQLiteCommand command,
            SessionEncounter encounter)
        {
            command.Parameters.AddWithValue(
                "@Id", encounter.Id.ToString());

            command.Parameters.AddWithValue(
                "@SessionId", encounter.SessionId.ToString());

            command.Parameters.AddWithValue(
                "@Name", encounter.Name.Trim());

            command.Parameters.AddWithValue(
                "@EncounterNotes",
                encounter.EncounterNotes ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                encounter.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                encounter.LastModified.ToString(
                    "o", CultureInfo.InvariantCulture));
        }

        private SessionEncounter ReadEncounter(SQLiteDataReader reader)
        {
            return new SessionEncounter
            {
                Id = Guid.Parse(reader["Id"].ToString()),

                SessionId = Guid.Parse(
                    reader["SessionId"].ToString()),

                Name = reader["Name"].ToString(),

                EncounterNotes = reader["EncounterNotes"] == DBNull.Value
                    ? string.Empty
                    : reader["EncounterNotes"].ToString(),

                CreatedDate = DateTime.Parse(
                    reader["CreatedDate"].ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind),

                LastModified = DateTime.Parse(
                    reader["LastModified"].ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)
            };
        }

        public Guid GetCampaignIdForSession(Guid sessionId)
        {
            using (var connection =
                new SQLiteConnection(_connectionString))
            {
                connection.Open();

                const string sql = @"
            SELECT CampaignId
            FROM CampaignSessions
            WHERE Id = @SessionId;";

                using (var command =
                    new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue(
                        "@SessionId", sessionId.ToString());

                    object result = command.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "The campaign session could not be found.");
                    }

                    return Guid.Parse(result.ToString());
                }
            }
        }

    }
}

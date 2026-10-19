
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class QuestRepository
    {
        private readonly string _connectionString;

        public QuestRepository()
        {
            _connectionString =
                "Data Source=" + DatabaseInitializer.DatabasePath +
                ";Version=3;Foreign Keys=True;";
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            using (var command = new SQLiteCommand(
                "PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }

        public List<Quest> GetByCampaignId(Guid campaignId)
        {
            var quests = new List<Quest>();

            const string sql = @"
                SELECT
                    Id, CampaignId, Name, Description,
                    QuestGiverId, Status, Notes,
                    CreatedDate, LastModified
                FROM Quests
                WHERE CampaignId = @CampaignId
                ORDER BY Name;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CampaignId", campaignId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        quests.Add(ReadQuest(reader));
                    }
                }
            }

            return quests;
        }

        public Quest GetById(Guid id)
        {
            const string sql = @"
                SELECT
                    Id, CampaignId, Name, Description,
                    QuestGiverId, Status, Notes,
                    CreatedDate, LastModified
                FROM Quests
                WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadQuest(reader);
                    }
                }
            }

            return null;
        }

        public void Add(Quest quest)
        {
            ValidateQuest(quest);

            if (quest.Id == Guid.Empty)
                quest.Id = Guid.NewGuid();

            if (quest.CreatedDate == default(DateTime))
                quest.CreatedDate = DateTime.Now;

            quest.LastModified = DateTime.Now;

            const string sql = @"
                INSERT INTO Quests
                (
                    Id, CampaignId, Name, Description,
                    QuestGiverId, Status, Notes,
                    CreatedDate, LastModified
                )
                VALUES
                (
                    @Id, @CampaignId, @Name, @Description,
                    @QuestGiverId, @Status, @Notes,
                    @CreatedDate, @LastModified
                );";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                AddParameters(command, quest);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Quest quest)
        {
            ValidateQuest(quest);
            quest.LastModified = DateTime.Now;

            const string sql = @"
                UPDATE Quests SET
                    CampaignId = @CampaignId,
                    Name = @Name,
                    Description = @Description,
                    QuestGiverId = @QuestGiverId,
                    Status = @Status,
                    Notes = @Notes,
                    LastModified = @LastModified
                WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                AddParameters(command, quest);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The quest could not be found.");
                }
            }
        }

        public void Delete(Guid id)
        {
            const string sql =
                "DELETE FROM Quests WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The quest could not be found.");
                }
            }
        }

        private static void ValidateQuest(Quest quest)
        {
            if (quest == null)
                throw new ArgumentNullException(nameof(quest));

            if (quest.CampaignId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A campaign must be selected for this quest.");
            }

            if (string.IsNullOrWhiteSpace(quest.Name))
            {
                throw new ArgumentException(
                    "Quest name is required.");
            }

            if (string.IsNullOrWhiteSpace(quest.Status))
            {
                throw new ArgumentException(
                    "Quest status is required.");
            }
        }

        private static void AddParameters(
            SQLiteCommand command, Quest quest)
        {
            command.Parameters.AddWithValue(
                "@Id", quest.Id.ToString());

            command.Parameters.AddWithValue(
                "@CampaignId", quest.CampaignId.ToString());

            command.Parameters.AddWithValue(
                "@Name", quest.Name.Trim());

            command.Parameters.AddWithValue(
                "@Description", quest.Description ?? string.Empty);

            command.Parameters.AddWithValue(
                "@QuestGiverId",
                quest.QuestGiverId.HasValue
                    ? (object)quest.QuestGiverId.Value.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "@Status", quest.Status);

            command.Parameters.AddWithValue(
                "@Notes", quest.Notes ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                quest.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                quest.LastModified.ToString(
                    "o", CultureInfo.InvariantCulture));
        }

        private static Quest ReadQuest(SQLiteDataReader reader)
        {
            string questGiverValue =
                reader["QuestGiverId"] == DBNull.Value
                    ? null
                    : Convert.ToString(reader["QuestGiverId"]);

            Guid parsedQuestGiverId;

            Guid? questGiverId =
                Guid.TryParse(questGiverValue, out parsedQuestGiverId)
                    ? (Guid?)parsedQuestGiverId
                    : null;

            return new Quest
            {
                Id = Guid.Parse(Convert.ToString(reader["Id"])),

                CampaignId = Guid.Parse(
                    Convert.ToString(reader["CampaignId"])),

                Name = ReadString(reader, "Name"),
                Description = ReadString(reader, "Description"),
                QuestGiverId = questGiverId,
                Status = ReadString(reader, "Status"),
                Notes = ReadString(reader, "Notes"),

                CreatedDate = ReadDateTime(reader, "CreatedDate"),
                LastModified = ReadDateTime(reader, "LastModified")
            };
        }

        private static string ReadString(
            SQLiteDataReader reader, string column)
        {
            if (reader[column] == DBNull.Value)
                return string.Empty;

            return Convert.ToString(reader[column]);
        }

        private static DateTime ReadDateTime(
            SQLiteDataReader reader, string column)
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

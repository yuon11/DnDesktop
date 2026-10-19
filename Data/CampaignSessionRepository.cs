
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class CampaignSessionRepository
    {
        private readonly string _connectionString;

        public CampaignSessionRepository()
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

        public List<CampaignSession> GetByCampaignId(Guid campaignId)
        {
            var sessions = new List<CampaignSession>();

            const string sql = @"
                SELECT *
                FROM CampaignSessions
                WHERE CampaignId = @CampaignId
                ORDER BY
                    CASE WHEN SessionDate IS NULL THEN 1 ELSE 0 END,
                    SessionDate,
                    Title;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CampaignId", campaignId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sessions.Add(ReadSession(reader));
                    }
                }
            }

            return sessions;
        }

        public CampaignSession GetById(Guid id)
        {
            const string sql = @"
                SELECT *
                FROM CampaignSessions
                WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadSession(reader);
                    }
                }
            }

            return null;
        }

        public void Add(CampaignSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            ValidateSession(session);

            if (session.Id == Guid.Empty)
                session.Id = Guid.NewGuid();

            if (session.CreatedDate == default(DateTime))
                session.CreatedDate = DateTime.Now;

            session.LastModified = DateTime.Now;

            const string sql = @"
                INSERT INTO CampaignSessions
                (
                    Id, CampaignId, Title, SessionDate, Status,
                    Summary, SessionNotes, CreatedDate, LastModified
                )
                VALUES
                (
                    @Id, @CampaignId, @Title, @SessionDate, @Status,
                    @Summary, @SessionNotes, @CreatedDate, @LastModified
                );";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                AddSessionParameters(command, session);
                command.ExecuteNonQuery();
            }
        }

        public void Update(CampaignSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            ValidateSession(session);
            session.LastModified = DateTime.Now;

            const string sql = @"
                UPDATE CampaignSessions SET
                    CampaignId = @CampaignId,
                    Title = @Title,
                    SessionDate = @SessionDate,
                    Status = @Status,
                    Summary = @Summary,
                    SessionNotes = @SessionNotes,
                    LastModified = @LastModified
                WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                AddSessionParameters(command, session);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The session could not be found.");
                }
            }
        }

        public void Delete(Guid id)
        {
            const string sql =
                "DELETE FROM CampaignSessions WHERE Id = @Id;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The session could not be found.");
                }
            }
        }

        private static void ValidateSession(CampaignSession session)
        {
            if (session.CampaignId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid campaign is required.");
            }

            if (string.IsNullOrWhiteSpace(session.Title))
            {
                throw new ArgumentException(
                    "A session title is required.");
            }

            var validStatuses = new[]
            {
                "Planned",
                "In Progress",
                "Completed"
            };

            if (Array.IndexOf(validStatuses, session.Status) < 0)
            {
                throw new ArgumentException(
                    "Select a valid session status.");
            }
        }

        private static CampaignSession ReadSession(
            SQLiteDataReader reader)
        {
            var session = new CampaignSession();

            session.Id = Guid.Parse(reader["Id"].ToString());
            session.CampaignId =
                Guid.Parse(reader["CampaignId"].ToString());

            session.Title = reader["Title"].ToString();

            session.SessionDate =
                reader["SessionDate"] == DBNull.Value
                    ? (DateTime?)null
                    : DateTime.Parse(
                        reader["SessionDate"].ToString(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind);

            session.Status = reader["Status"].ToString();
            session.Summary = reader["Summary"].ToString();
            session.SessionNotes = reader["SessionNotes"].ToString();

            session.CreatedDate = DateTime.Parse(
                reader["CreatedDate"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            session.LastModified = DateTime.Parse(
                reader["LastModified"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            return session;
        }

        private static void AddSessionParameters(
            SQLiteCommand command,
            CampaignSession session)
        {
            command.Parameters.AddWithValue(
                "@Id", session.Id.ToString());

            command.Parameters.AddWithValue(
                "@CampaignId", session.CampaignId.ToString());

            command.Parameters.AddWithValue(
                "@Title", session.Title.Trim());

            command.Parameters.AddWithValue(
                "@SessionDate",
                session.SessionDate.HasValue
                    ? (object)session.SessionDate.Value.ToString(
                        "o", CultureInfo.InvariantCulture)
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "@Status", session.Status);

            command.Parameters.AddWithValue(
                "@Summary", session.Summary ?? string.Empty);

            command.Parameters.AddWithValue(
                "@SessionNotes", session.SessionNotes ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                session.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                session.LastModified.ToString(
                    "o", CultureInfo.InvariantCulture));
        }
    }
}


using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class CampaignRepository
    {
        private readonly string _connectionString;

        public CampaignRepository()
        {
            _connectionString =
                "Data Source=" + DatabaseInitializer.DatabasePath +
                ";Version=3;Foreign Keys=True;";
        }

        private SQLiteConnection OpenConnection()
        {
            SQLiteConnection connection =
                new SQLiteConnection(_connectionString);

            connection.Open();

            using (SQLiteCommand command =
                new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }

        public List<Campaign> GetAll()
        {
            List<Campaign> campaigns = new List<Campaign>();

            const string sql =
                "SELECT * FROM Campaigns ORDER BY Name;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    campaigns.Add(ReadCampaign(reader));
                }
            }

            return campaigns;
        }

        public Campaign GetById(Guid id)
        {
            const string sql =
                "SELECT * FROM Campaigns WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadCampaign(reader);
                    }
                }
            }

            return null;
        }

        public void Add(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (string.IsNullOrWhiteSpace(campaign.Name))
                throw new ArgumentException(
                    "Campaign name is required.");

            if (campaign.Id == Guid.Empty)
                campaign.Id = Guid.NewGuid();

            if (campaign.CreatedDate == default(DateTime))
                campaign.CreatedDate = DateTime.Now;

            campaign.LastModified = DateTime.Now;

            const string sql = @"
                INSERT INTO Campaigns
                (
                    Id, Name, Description, CampaignNotes,
                    CreatedDate, LastModified
                )
                VALUES
                (
                    @Id, @Name, @Description, @CampaignNotes,
                    @CreatedDate, @LastModified
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddParameters(command, campaign);
                command.ExecuteNonQuery();
            }
        }

        public void Update(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            if (string.IsNullOrWhiteSpace(campaign.Name))
                throw new ArgumentException(
                    "Campaign name is required.");

            campaign.LastModified = DateTime.Now;

            const string sql = @"
                UPDATE Campaigns SET
                    Name = @Name,
                    Description = @Description,
                    CampaignNotes = @CampaignNotes,
                    LastModified = @LastModified
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddParameters(command, campaign);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The campaign could not be found.");
                }
            }
        }

        public void Delete(Guid id)
        {
            const string sql =
                "DELETE FROM Campaigns WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The campaign could not be found.");
                }
            }
        }

        private static Campaign ReadCampaign(SQLiteDataReader reader)
        {
            return new Campaign
            {
                Id = Guid.Parse(reader["Id"].ToString()),
                Name = reader["Name"].ToString(),
                Description = reader["Description"].ToString(),
                CampaignNotes = reader["CampaignNotes"].ToString(),

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

        private static void AddParameters(
            SQLiteCommand command,
            Campaign campaign)
        {
            command.Parameters.AddWithValue(
                "@Id", campaign.Id.ToString());

            command.Parameters.AddWithValue(
                "@Name", campaign.Name.Trim());

            command.Parameters.AddWithValue(
                "@Description",
                campaign.Description ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CampaignNotes",
                campaign.CampaignNotes ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                campaign.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                campaign.LastModified.ToString(
                    "o", CultureInfo.InvariantCulture));
        }
    }
}

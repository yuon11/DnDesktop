
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace DnDesktop.Data
{
    public class CampaignLocationRepository
    {
        private readonly string _connectionString;
        private readonly LocationRepository _locationRepository =
            new LocationRepository();

        public CampaignLocationRepository()
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

        public List<Location> GetByCampaignId(Guid campaignId)
        {
            var locationIds = new HashSet<Guid>();

            const string sql = @"
                SELECT LocationId
                FROM CampaignLocations
                WHERE CampaignId = @CampaignId;";

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CampaignId", campaignId.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Guid id;

                        if (Guid.TryParse(
                            reader["LocationId"].ToString(), out id))
                        {
                            locationIds.Add(id);
                        }
                    }
                }
            }

            return _locationRepository.GetAll()
                .Where(location => locationIds.Contains(location.Id))
                .OrderBy(location => location.Name)
                .ToList();
        }

        public void SaveLocations(
            Guid campaignId,
            IEnumerable<Guid> locationIds)
        {
            if (campaignId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid campaign ID is required.");
            }

            var ids = new HashSet<Guid>(
                locationIds ?? Enumerable.Empty<Guid>());

            if (ids.Contains(Guid.Empty))
            {
                throw new ArgumentException(
                    "Location IDs must be valid.");
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = new SQLiteCommand(
                        "SELECT COUNT(*) FROM Campaigns WHERE Id = @Id;",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@Id", campaignId.ToString());

                        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                        {
                            throw new InvalidOperationException(
                                "The campaign could not be found.");
                        }
                    }

                    using (var command = new SQLiteCommand(
                        @"DELETE FROM CampaignLocations
                          WHERE CampaignId = @CampaignId;",
                        connection, transaction))
                    {
                        command.Parameters.AddWithValue(
                            "@CampaignId", campaignId.ToString());

                        command.ExecuteNonQuery();
                    }

                    const string insertSql = @"
                        INSERT INTO CampaignLocations
                            (CampaignId, LocationId)
                        VALUES
                            (@CampaignId, @LocationId);";

                    foreach (Guid locationId in ids)
                    {
                        using (var command = new SQLiteCommand(
                            insertSql, connection, transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@CampaignId", campaignId.ToString());

                            command.Parameters.AddWithValue(
                                "@LocationId", locationId.ToString());

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
    }
}

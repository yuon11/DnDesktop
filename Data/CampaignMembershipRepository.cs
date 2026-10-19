
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace DnDesktop.Data
{
    public class CampaignMembershipRepository
    {
        private readonly string _connectionString;
        private readonly CharacterRepository _characterRepository =
            new CharacterRepository();

        public CampaignMembershipRepository()
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

        public List<Character> GetPartyMembers(Guid campaignId)
        {
            return GetCharacters(
                "CampaignPartyMembers", campaignId);
        }

        public List<Character> GetNPCs(Guid campaignId)
        {
            return GetCharacters(
                "CampaignNPCs", campaignId);
        }

        private List<Character> GetCharacters(
            string tableName, Guid campaignId)
        {
            // These table names are internal constants supplied only
            // by the repository methods above.
            var characterIds = new HashSet<Guid>();

            string sql =
                "SELECT CharacterId FROM " + tableName +
                " WHERE CampaignId = @CampaignId;";

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
                            reader["CharacterId"].ToString(), out id))
                        {
                            characterIds.Add(id);
                        }
                    }
                }
            }

            return _characterRepository.GetAll()
                .Where(c => characterIds.Contains(c.Id))
                .OrderBy(c => c.Name)
                .ToList();
        }

        public void SaveMemberships(
            Guid campaignId,
            IEnumerable<Guid> partyCharacterIds,
            IEnumerable<Guid> npcCharacterIds)
        {
            if (campaignId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid campaign ID is required.");
            }

            var partyIds = new HashSet<Guid>(
                partyCharacterIds ?? Enumerable.Empty<Guid>());

            var npcIds = new HashSet<Guid>(
                npcCharacterIds ?? Enumerable.Empty<Guid>());

            if (partyIds.Contains(Guid.Empty) ||
                npcIds.Contains(Guid.Empty))
            {
                throw new ArgumentException(
                    "Character IDs must be valid.");
            }

            if (partyIds.Overlaps(npcIds))
            {
                throw new InvalidOperationException(
                    "A character cannot be both a party member " +
                    "and an NPC in the same campaign.");
            }

            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsureCampaignExists(
                        connection, transaction, campaignId);

                    DeleteMemberships(
                        connection, transaction,
                        "CampaignPartyMembers", campaignId);

                    DeleteMemberships(
                        connection, transaction,
                        "CampaignNPCs", campaignId);

                    InsertMemberships(
                        connection, transaction,
                        "CampaignPartyMembers", campaignId, partyIds);

                    InsertMemberships(
                        connection, transaction,
                        "CampaignNPCs", campaignId, npcIds);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static void EnsureCampaignExists(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            Guid campaignId)
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
        }

        private static void DeleteMemberships(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string tableName,
            Guid campaignId)
        {
            string sql =
                "DELETE FROM " + tableName +
                " WHERE CampaignId = @CampaignId;";

            using (var command = new SQLiteCommand(
                sql, connection, transaction))
            {
                command.Parameters.AddWithValue(
                    "@CampaignId", campaignId.ToString());

                command.ExecuteNonQuery();
            }
        }

        private static void InsertMemberships(
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string tableName,
            Guid campaignId,
            IEnumerable<Guid> characterIds)
        {
            string sql =
                "INSERT INTO " + tableName +
                " (CampaignId, CharacterId) " +
                "VALUES (@CampaignId, @CharacterId);";

            foreach (Guid characterId in characterIds)
            {
                using (var command = new SQLiteCommand(
                    sql, connection, transaction))
                {
                    command.Parameters.AddWithValue(
                        "@CampaignId", campaignId.ToString());

                    command.Parameters.AddWithValue(
                        "@CharacterId", characterId.ToString());

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}

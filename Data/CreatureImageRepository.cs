
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class CreatureImageRepository
    {
        private readonly string _connectionString;

        public CreatureImageRepository()
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

        public List<CreatureImage> GetImages(Guid creatureId)
        {
            if (creatureId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid creature ID is required.",
                    nameof(creatureId));
            }

            List<CreatureImage> images = new List<CreatureImage>();

            const string sql = @"
                SELECT Id, CreatureId, ImagePath, Caption,
                       ImageCategory, IsPrimary, DisplayOrder, CreatedDate
                FROM CreatureImages
                WHERE CreatureId = @CreatureId
                ORDER BY DisplayOrder, CreatedDate;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@CreatureId", creatureId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        images.Add(ReadImage(reader));
                    }
                }
            }

            return images;
        }

        public CreatureImage GetImageById(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "A valid image ID is required.", nameof(id));

            const string sql = @"
                SELECT Id, CreatureId, ImagePath, Caption,
                       ImageCategory, IsPrimary, DisplayOrder, CreatedDate
                FROM CreatureImages
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return ReadImage(reader);
                }
            }
        }

        public void AddImage(CreatureImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.CreatureId == Guid.Empty)
                throw new ArgumentException(
                    "A valid creature ID is required.");

            if (string.IsNullOrWhiteSpace(image.ImagePath))
                throw new ArgumentException(
                    "An image path is required.");

            if (image.Id == Guid.Empty)
                image.Id = Guid.NewGuid();

            if (image.CreatedDate == default(DateTime))
                image.CreatedDate = DateTime.Now;

            const string sql = @"
                INSERT INTO CreatureImages
                (
                    Id, CreatureId, ImagePath, Caption,
                    ImageCategory, IsPrimary, DisplayOrder, CreatedDate
                )
                VALUES
                (
                    @Id, @CreatureId, @ImagePath, @Caption,
                    @ImageCategory, @IsPrimary, @DisplayOrder, @CreatedDate
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddImageParameters(command, image);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateImage(CreatureImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image.Id == Guid.Empty)
                throw new ArgumentException(
                    "A valid image ID is required.");

            if (image.CreatureId == Guid.Empty)
                throw new ArgumentException(
                    "A valid creature ID is required.");

            if (string.IsNullOrWhiteSpace(image.ImagePath))
                throw new ArgumentException(
                    "An image path is required.");

            const string sql = @"
                UPDATE CreatureImages SET
                    CreatureId = @CreatureId,
                    ImagePath = @ImagePath,
                    Caption = @Caption,
                    ImageCategory = @ImageCategory,
                    IsPrimary = @IsPrimary,
                    DisplayOrder = @DisplayOrder
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddImageParameters(command, image);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The creature image record could not be found.");
                }
            }
        }

        public void DeleteImage(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "A valid image ID is required.", nameof(id));

            const string sql =
                "DELETE FROM CreatureImages WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The creature image record could not be found.");
                }
            }
        }

        private static CreatureImage ReadImage(SQLiteDataReader reader)
        {
            return new CreatureImage
            {
                Id = Guid.Parse(reader["Id"].ToString()),
                CreatureId = Guid.Parse(reader["CreatureId"].ToString()),
                ImagePath = reader["ImagePath"].ToString(),
                Caption = reader["Caption"].ToString(),
                ImageCategory = reader["ImageCategory"].ToString(),
                IsPrimary = Convert.ToInt32(reader["IsPrimary"]) == 1,
                DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                CreatedDate = DateTime.Parse(
                    reader["CreatedDate"].ToString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)
            };
        }

        private static void AddImageParameters(
            SQLiteCommand command, CreatureImage image)
        {
            command.Parameters.AddWithValue(
                "@Id", image.Id.ToString());

            command.Parameters.AddWithValue(
                "@CreatureId", image.CreatureId.ToString());

            command.Parameters.AddWithValue(
                "@ImagePath", image.ImagePath);

            command.Parameters.AddWithValue(
                "@Caption", image.Caption ?? string.Empty);

            command.Parameters.AddWithValue(
                "@ImageCategory",
                string.IsNullOrWhiteSpace(image.ImageCategory)
                    ? "Other"
                    : image.ImageCategory);

            command.Parameters.AddWithValue(
                "@IsPrimary", image.IsPrimary ? 1 : 0);

            command.Parameters.AddWithValue(
                "@DisplayOrder", image.DisplayOrder);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                image.CreatedDate.ToString(
                    "o", CultureInfo.InvariantCulture));
        }
    }
}

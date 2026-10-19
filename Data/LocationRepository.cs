
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace DnDesktop.Data
{
    public class LocationRepository
    {
        private readonly string _connectionString;

        public LocationRepository()
        {
            _connectionString =
                "Data Source=" + DatabaseInitializer.DatabasePath +
                ";Version=3;Foreign Keys=True;";
        }


        public LocationImage GetImageById(Guid id)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
            SELECT Id, LocationId, ImagePath, Caption,
                   ImageCategory, IsPrimary, DisplayOrder, CreatedDate
            FROM LocationImages
            WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id.ToString());

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new LocationImage
                    {
                        Id = Guid.Parse(reader["Id"].ToString()),
                        LocationId = Guid.Parse(reader["LocationId"].ToString()),
                        ImagePath = reader["ImagePath"].ToString(),
                        Caption = reader["Caption"].ToString(),
                        ImageCategory = reader["ImageCategory"].ToString(),
                        IsPrimary = Convert.ToInt32(reader["IsPrimary"]) == 1,
                        DisplayOrder = Convert.ToInt32(reader["DisplayOrder"]),
                        CreatedDate = DateTime.Parse(
                            reader["CreatedDate"].ToString(),
                            null,
                            System.Globalization.DateTimeStyles.RoundtripKind)
                    };
                }
            }
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

        // Retrieve all locations.
        public List<Location> GetAll()
        {
            List<Location> locations = new List<Location>();

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT * FROM Locations ORDER BY Name;", connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    locations.Add(ReadLocation(reader));
                }
            }

            return locations;
        }

        // Retrieve one location by its ID.
        public Location GetById(Guid id)
        {
            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command = new SQLiteCommand(
                "SELECT * FROM Locations WHERE Id = @Id;", connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadLocation(reader);
                    }
                }
            }

            return null;
        }

        // Insert a new location.
        public void Add(Location location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            if (string.IsNullOrWhiteSpace(location.Name))
                throw new ArgumentException("Location name is required.");

            if (location.Id == Guid.Empty)
                location.Id = Guid.NewGuid();

            if (location.CreatedDate == default(DateTime))
                location.CreatedDate = DateTime.Now;

            location.LastModified = DateTime.Now;

            const string sql = @"
                INSERT INTO Locations
                (
                    Id, Name, LocationType, ParentLocationId,
                    Description, Population, Government, NotableNPCs,
                    PointsOfInterest, GMNotes, CreatedDate, LastModified
                )
                VALUES
                (
                    @Id, @Name, @LocationType, @ParentLocationId,
                    @Description, @Population, @Government, @NotableNPCs,
                    @PointsOfInterest, @GMNotes, @CreatedDate, @LastModified
                );";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddLocationParameters(command, location);
                command.ExecuteNonQuery();
            }
        }

        // Update an existing location.
        public void Update(Location location)
        {
            if (location == null)
                throw new ArgumentNullException("location");

            if (string.IsNullOrWhiteSpace(location.Name))
                throw new ArgumentException("Location name is required.");

            location.LastModified = DateTime.Now;

            const string sql = @"
                UPDATE Locations SET
                    Name = @Name,
                    LocationType = @LocationType,
                    ParentLocationId = @ParentLocationId,
                    Description = @Description,
                    Population = @Population,
                    Government = @Government,
                    NotableNPCs = @NotableNPCs,
                    PointsOfInterest = @PointsOfInterest,
                    GMNotes = @GMNotes,
                    LastModified = @LastModified
                WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                AddLocationParameters(command, location);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The location could not be found.");
                }
            }
        }

        // Delete a location. Child locations are preserved by the
        // ON DELETE SET NULL foreign-key rule.
        public void Delete(Guid id)
        {
            const string sql =
                "DELETE FROM Locations WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The location could not be found.");
                }
            }
        }


        // Retrieve all image records for a location.
        public List<LocationImage> GetImages(Guid locationId)
        {
            List<LocationImage> images =
                new List<LocationImage>();

            const string sql = @"
                SELECT * FROM LocationImages
                WHERE LocationId = @LocationId
                ORDER BY DisplayOrder, CreatedDate;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue(
                    "@LocationId", locationId.ToString());

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        images.Add(ReadLocationImage(reader));
                    }
                }
            }

            return images;
        }

        // Add an image record. The image file must already exist
        // at ImagePath; copying files will be handled by the UI layer.
        public void AddImage(LocationImage image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (string.IsNullOrWhiteSpace(image.ImagePath))
                throw new ArgumentException("An image path is required.");

            if (image.Id == Guid.Empty)
                image.Id = Guid.NewGuid();

            if (image.CreatedDate == default(DateTime))
                image.CreatedDate = DateTime.Now;

            const string sql = @"
                INSERT INTO LocationImages
                (
                    Id, LocationId, ImagePath, Caption,
                    ImageCategory, IsPrimary, DisplayOrder, CreatedDate
                )
                VALUES
                (
                    @Id, @LocationId, @ImagePath, @Caption,
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

        // Update image metadata.
        public void UpdateImage(LocationImage image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            const string sql = @"
                UPDATE LocationImages SET
                    LocationId = @LocationId,
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
                        "The image record could not be found.");
                }
            }
        }

        // Delete an image record, not the physical image file.
        public void DeleteImage(Guid id)
        {
            const string sql =
                "DELETE FROM LocationImages WHERE Id = @Id;";

            using (SQLiteConnection connection = OpenConnection())
            using (SQLiteCommand command =
                new SQLiteCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", id.ToString());

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException(
                        "The image record could not be found.");
                }
            }
        }

        private static Location ReadLocation(SQLiteDataReader reader)
        {
            Location location = new Location();

            location.Id = Guid.Parse(reader["Id"].ToString());
            location.Name = reader["Name"].ToString();
            location.LocationType = reader["LocationType"].ToString();

            location.ParentLocationId =
                reader["ParentLocationId"] == DBNull.Value
                    ? (Guid?)null
                    : Guid.Parse(reader["ParentLocationId"].ToString());

            location.Description = reader["Description"].ToString();
            location.Population = reader["Population"].ToString();
            location.Government = reader["Government"].ToString();
            location.NotableNPCs = reader["NotableNPCs"].ToString();
            location.PointsOfInterest =
                reader["PointsOfInterest"].ToString();
            location.GMNotes = reader["GMNotes"].ToString();

            location.CreatedDate = DateTime.Parse(
                reader["CreatedDate"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            location.LastModified = DateTime.Parse(
                reader["LastModified"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            return location;
        }

        private static LocationImage ReadLocationImage(
            SQLiteDataReader reader)
        {
            LocationImage image = new LocationImage();

            image.Id = Guid.Parse(reader["Id"].ToString());
            image.LocationId =
                Guid.Parse(reader["LocationId"].ToString());
            image.ImagePath = reader["ImagePath"].ToString();
            image.Caption = reader["Caption"].ToString();
            image.ImageCategory = reader["ImageCategory"].ToString();
            image.IsPrimary =
                Convert.ToInt32(reader["IsPrimary"]) == 1;
            image.DisplayOrder =
                Convert.ToInt32(reader["DisplayOrder"]);

            image.CreatedDate = DateTime.Parse(
                reader["CreatedDate"].ToString(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            return image;
        }

        private static void AddLocationParameters(
            SQLiteCommand command, Location location)
        {
            command.Parameters.AddWithValue("@Id", location.Id.ToString());
            command.Parameters.AddWithValue("@Name", location.Name.Trim());

            command.Parameters.AddWithValue(
                "@LocationType", location.LocationType ?? "Other");

            command.Parameters.AddWithValue(
                "@ParentLocationId",
                location.ParentLocationId.HasValue
                    ? (object)location.ParentLocationId.Value.ToString()
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "@Description", location.Description ?? string.Empty);
            command.Parameters.AddWithValue(
                "@Population", location.Population ?? string.Empty);
            command.Parameters.AddWithValue(
                "@Government", location.Government ?? string.Empty);
            command.Parameters.AddWithValue(
                "@NotableNPCs", location.NotableNPCs ?? string.Empty);
            command.Parameters.AddWithValue(
                "@PointsOfInterest", location.PointsOfInterest ?? string.Empty);
            command.Parameters.AddWithValue(
                "@GMNotes", location.GMNotes ?? string.Empty);

            command.Parameters.AddWithValue(
                "@CreatedDate",
                location.CreatedDate.ToString("o",
                    CultureInfo.InvariantCulture));

            command.Parameters.AddWithValue(
                "@LastModified",
                location.LastModified.ToString("o",
                    CultureInfo.InvariantCulture));
        }

        private static void AddImageParameters(
            SQLiteCommand command, LocationImage image)
        {
            command.Parameters.AddWithValue("@Id", image.Id.ToString());
            command.Parameters.AddWithValue(
                "@LocationId", image.LocationId.ToString());
            command.Parameters.AddWithValue(
                "@ImagePath", image.ImagePath ?? string.Empty);
            command.Parameters.AddWithValue(
                "@Caption", image.Caption ?? string.Empty);
            command.Parameters.AddWithValue(
                "@ImageCategory", image.ImageCategory ?? "Other");
            command.Parameters.AddWithValue(
                "@IsPrimary", image.IsPrimary ? 1 : 0);
            command.Parameters.AddWithValue(
                "@DisplayOrder", image.DisplayOrder);
            command.Parameters.AddWithValue(
                "@CreatedDate",
                image.CreatedDate.ToString("o",
                    CultureInfo.InvariantCulture));
        }
    }
}


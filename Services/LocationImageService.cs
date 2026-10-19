
using System;
using System.Collections.Generic;
using System.IO;
using DnDesktop.Data;
using DnDesktop.Models;

namespace DnDesktop.Services
{
    public class LocationImageService
    {
        private readonly LocationRepository _repository;

        public LocationImageService()
        {
            _repository = new LocationRepository();
        }

        public LocationImage ImportImage(
            Guid locationId,
            string sourcePath,
            string caption = "",
            string imageCategory = "Other",
            bool isPrimary = false)
        {
            if (locationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid location ID is required.",
                    nameof(locationId));
            }

            // Copy the source image into managed storage.
            string storedPath =
                LocationImageStorage.CopyImageToStorage(sourcePath);

            // Create the database metadata record.
            LocationImage image = new LocationImage
            {
                Id = Guid.NewGuid(),
                LocationId = locationId,
                ImagePath = storedPath,
                Caption = caption ?? string.Empty,
                ImageCategory = string.IsNullOrWhiteSpace(imageCategory)
                    ? "Other"
                    : imageCategory,
                IsPrimary = isPrimary,
                CreatedDate = DateTime.Now
            };

            try
            {
                _repository.AddImage(image);
                return image;
            }
            catch
            {
                // If saving metadata fails, clean up the copy we just made.
                // The source image is never deleted.
                if (File.Exists(storedPath))
                {
                    File.Delete(storedPath);
                }

                throw;
            }
        }

        public List<LocationImage> GetImages(Guid locationId)
        {
            if (locationId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid location ID is required.",
                    nameof(locationId));
            }

            return _repository.GetImages(locationId);
        }

        public bool DeleteImage(Guid imageId)
        {
            // Find the database record before deleting anything.
            var image = _repository.GetImageById(imageId);

            if (image == null)
                return false;

            string managedDirectory = Path.GetFullPath(
                LocationImageStorage.ImageDirectory);

            string imagePath = Path.GetFullPath(image.ImagePath);

            // Only allow deletion of files directly inside the managed folder.
            string parentDirectory = Path.GetDirectoryName(imagePath);

            if (!string.Equals(
                    parentDirectory,
                    managedDirectory,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The image is outside the managed image directory. " +
                    "The file was not deleted.");
            }

            // Delete the metadata first. If this fails, keep the physical file.
            _repository.DeleteImage(imageId);

            // Remove the managed copy if it still exists.
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }

            return true;
        }

    }
}

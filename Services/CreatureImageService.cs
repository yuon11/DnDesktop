
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DnDesktop.Services
{
    public class CreatureImageService
    {
        private readonly CreatureImageRepository _repository;

        public CreatureImageService()
        {
            _repository = new CreatureImageRepository();
        }

        public CreatureImage ImportImage(
            Guid creatureId,
            string sourcePath,
            string caption = "",
            string imageCategory = "Other",
            bool isPrimary = false)
        {
            if (creatureId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid creature ID is required.",
                    nameof(creatureId));
            }

            // Copy the source image into managed storage.
            string storedPath =
                CreatureImageStorage.CopyImageToStorage(sourcePath);

            // Create the database metadata record.
            CreatureImage image = new CreatureImage
            {
                Id = Guid.NewGuid(),
                CreatureId = creatureId,
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
                // If saving metadata fails, clean up the copied file.
                // The original source image is never deleted.
                if (File.Exists(storedPath))
                {
                    File.Delete(storedPath);
                }

                throw;
            }
        }

        public List<CreatureImage> GetImages(Guid creatureId)
        {
            if (creatureId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid creature ID is required.",
                    nameof(creatureId));
            }

            return _repository.GetImages(creatureId);
        }

        public bool DeleteImage(Guid imageId)
        {
            // Find the database record before deleting anything.
            CreatureImage image = _repository.GetImageById(imageId);

            if (image == null)
                return false;

            string managedDirectory = Path.GetFullPath(
                CreatureImageStorage.ImageDirectory);

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

        public void SetPrimaryImage(Guid creatureId, Guid imageId)
        {
            if (creatureId == Guid.Empty)
                throw new ArgumentException(
                    "A valid creature ID is required.",
                    nameof(creatureId));

            if (imageId == Guid.Empty)
                throw new ArgumentException(
                    "A valid image ID is required.",
                    nameof(imageId));

            var images = _repository.GetImages(creatureId);

            CreatureImage selectedImage =
                images.FirstOrDefault(image => image.Id == imageId);

            if (selectedImage == null)
                throw new InvalidOperationException(
                    "The selected image does not belong to this creature.");

            // Clear the current primary image first.
            foreach (CreatureImage image in images)
            {
                if (image.IsPrimary)
                {
                    image.IsPrimary = false;
                    _repository.UpdateImage(image);
                }
            }

            // Mark and save the selected image as primary.
            selectedImage.IsPrimary = true;
            _repository.UpdateImage(selectedImage);
        }

    }
}

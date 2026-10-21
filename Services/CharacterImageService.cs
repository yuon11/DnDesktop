
using System;
using System.Collections.Generic;
using System.IO;
using DnDesktop.Data;
using DnDesktop.Models;

namespace DnDesktop.Services
{
    public class CharacterImageService
    {
        private readonly CharacterImageRepository _repository;

        public CharacterImageService()
        {
            _repository = new CharacterImageRepository();
        }

        public CharacterImage ImportImage(
            Guid characterId,
            string sourcePath,
            string caption = "",
            string imageCategory = "Other",
            bool isPrimary = false)
        {
            if (characterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid character ID is required.",
                    nameof(characterId));
            }

            // Copy the source image into managed storage.
            string storedPath =
                CharacterImageStorage.CopyImageToStorage(sourcePath);

            // Create the database metadata record.
            CharacterImage image = new CharacterImage
            {
                Id = Guid.NewGuid(),
                CharacterId = characterId,
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

        public List<CharacterImage> GetImages(Guid characterId)
        {
            if (characterId == Guid.Empty)
            {
                throw new ArgumentException(
                    "A valid character ID is required.",
                    nameof(characterId));
            }

            return _repository.GetImages(characterId);
        }

        public bool DeleteImage(Guid imageId)
        {
            // Find the database record before deleting anything.
            CharacterImage image = _repository.GetImageById(imageId);

            if (image == null)
                return false;

            string managedDirectory = Path.GetFullPath(
                CharacterImageStorage.ImageDirectory);

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

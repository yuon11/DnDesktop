
using System;
using System.IO;

namespace DnDesktop.Services
{
    public static class CreatureImageStorage
    {
        private static readonly string _imageDirectory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "DnDesktop",
                "CreatureImages");

        public static string ImageDirectory
        {
            get
            {
                EnsureImageDirectoryExists();
                return _imageDirectory;
            }
        }

        public static void EnsureImageDirectoryExists()
        {
            Directory.CreateDirectory(_imageDirectory);
        }

        public static string CopyImageToStorage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                throw new ArgumentException(
                    "An image file path is required.",
                    nameof(sourcePath));
            }

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    "The selected image file was not found.",
                    sourcePath);
            }

            string extension = Path.GetExtension(sourcePath)
                .ToLowerInvariant();

            string[] allowedExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".bmp",
                ".tif",
                ".tiff"
            };

            if (Array.IndexOf(allowedExtensions, extension) < 0)
            {
                throw new NotSupportedException(
                    "This image format is not supported: " + extension);
            }

            EnsureImageDirectoryExists();

            string uniqueFileName =
                Guid.NewGuid().ToString("N") + extension;

            string destinationPath =
                Path.Combine(_imageDirectory, uniqueFileName);

            File.Copy(sourcePath, destinationPath, false);

            return destinationPath;
        }
    }
}

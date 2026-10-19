
using DnDesktop.Data;
using DnDesktop.Models;
using DnDesktop.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DnDesktop
{
    public partial class LocationEditorPage : Page
    {
        private readonly LocationRepository _repository;
        private readonly LocationImageService _imageService;
        private readonly Location _location;
        private readonly bool _isNewLocation;
        private readonly Guid? _excludedParentId;

        private bool _loadingImageDetails;

        public LocationEditorPage(Location location)
        {
            InitializeComponent();

            _repository = new LocationRepository();
            _imageService = new LocationImageService();

            _isNewLocation = location == null;
            _location = location ?? new Location();
            _excludedParentId = _location.Id;

            EditorTitle.Text = _isNewLocation
                ? "Create Location"
                : "Edit Location";

            // Images require a saved location record.
            AddImageButton.IsEnabled = !_isNewLocation;

            LoadParentLocations();
            LoadLocation();

            if (!_isNewLocation)
            {
                LoadLocationImages();
            }
        }

        private void LoadParentLocations()
        {
            List<Location> locations = _repository.GetAll();

            locations = locations
                .Where(x => x.Id != _excludedParentId)
                .OrderBy(x => x.Name)
                .ToList();

            locations.Insert(0, new Location
            {
                Id = Guid.Empty,
                Name = "(None)"
            });

            ParentLocationComboBox.ItemsSource = locations;
            ParentLocationComboBox.SelectedValue = Guid.Empty;
        }

        private void LoadLocation()
        {
            NameTextBox.Text = _location.Name;
            DescriptionTextBox.Text = _location.Description;
            PopulationTextBox.Text = _location.Population;
            GovernmentTextBox.Text = _location.Government;
            NotableNPCsTextBox.Text = _location.NotableNPCs;
            PointsOfInterestTextBox.Text = _location.PointsOfInterest;
            GMNotesTextBox.Text = _location.GMNotes;

            foreach (ComboBoxItem item in LocationTypeComboBox.Items)
            {
                if (item.Content.ToString() == _location.LocationType)
                {
                    LocationTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            ParentLocationComboBox.SelectedValue =
                _location.ParentLocationId.HasValue
                    ? (object)_location.ParentLocationId.Value
                    : Guid.Empty;
        }

        private void LoadLocationImages(Guid? selectImageId = null)
        {
            try
            {
                List<LocationImage> images =
                    _imageService.GetImages(_location.Id);

                LocationImagesList.ItemsSource = images;

                if (selectImageId.HasValue)
                {
                    LocationImagesList.SelectedItem = images.FirstOrDefault(
                        x => x.Id == selectImageId.Value);
                }

                UpdateImageControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load location images.\n\n" + ex.Message,
                    "Image Loading Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateImageControls()
        {
            bool hasSelection = LocationImagesList.SelectedItem != null;

            RemoveImageButton.IsEnabled = hasSelection;
            ImageCaptionTextBox.IsEnabled = hasSelection;
            ImageCategoryComboBox.IsEnabled = hasSelection;
            PrimaryImageCheckBox.IsEnabled = hasSelection;
            SaveImageDetailsButton.IsEnabled = hasSelection;

            if (!hasSelection)
            {
                _loadingImageDetails = true;

                ImageCaptionTextBox.Text = string.Empty;
                ImageCategoryComboBox.SelectedIndex = -1;
                PrimaryImageCheckBox.IsChecked = false;

                _loadingImageDetails = false;
            }
        }

        private void LocationImagesList_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            LocationImage image =
                LocationImagesList.SelectedItem as LocationImage;

            _loadingImageDetails = true;

            if (image == null)
            {
                ImageCaptionTextBox.Text = string.Empty;
                ImageCategoryComboBox.SelectedIndex = -1;
                PrimaryImageCheckBox.IsChecked = false;
            }
            else
            {
                ImageCaptionTextBox.Text = image.Caption ?? string.Empty;
                PrimaryImageCheckBox.IsChecked = image.IsPrimary;

                bool categoryFound = false;

                foreach (ComboBoxItem item in ImageCategoryComboBox.Items)
                {
                    if (string.Equals(
                        item.Content.ToString(),
                        image.ImageCategory,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        ImageCategoryComboBox.SelectedItem = item;
                        categoryFound = true;
                        break;
                    }
                }

                if (!categoryFound)
                {
                    ImageCategoryComboBox.SelectedIndex = 0;
                }
            }

            _loadingImageDetails = false;

            UpdateImageControls();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewLocation)
            {
                MessageBox.Show(
                    "Save the location first, then reopen it to add images.",
                    "Save Location First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select Location Images",
                Filter =
                    "Image files (*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff)|" +
                    "*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff|" +
                    "All files (*.*)|*.*",
                Multiselect = true,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;

            int importedCount = 0;
            int failedCount = 0;
            List<string> errors = new List<string>();

            try
            {
                foreach (string filePath in dialog.FileNames)
                {
                    try
                    {
                        // Make the first image primary if this location
                        // does not have an image marked as primary.
                        List<LocationImage> existingImages =
                            _imageService.GetImages(_location.Id);

                        bool hasPrimary = existingImages.Any(
                            x => x.IsPrimary);

                        bool makePrimary = !hasPrimary &&
                            existingImages.Count == 0;

                        _imageService.ImportImage(
                            _location.Id,
                            filePath,
                            Path.GetFileNameWithoutExtension(filePath),
                            "Other",
                            makePrimary);

                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        failedCount++;

                        errors.Add(
                            Path.GetFileName(filePath) + ": " + ex.Message);
                    }
                }
            }
            finally
            {
                LoadLocationImages();
            }

            string message =
                "Images imported successfully: " + importedCount;

            if (failedCount > 0)
            {
                message += "\nImages that failed: " + failedCount +
                    "\n\n" + string.Join("\n", errors);
            }

            MessageBox.Show(
                message,
                failedCount == 0
                    ? "Image Import Complete"
                    : "Image Import Results",
                MessageBoxButton.OK,
                failedCount == 0
                    ? MessageBoxImage.Information
                    : MessageBoxImage.Warning);
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            LocationImage image =
                LocationImagesList.SelectedItem as LocationImage;

            if (image == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to remove this image?",
                "Remove Image",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                // Clear the selection and release the displayed image.
                LocationImagesList.SelectedItem = null;
                LocationImagesList.Items.Refresh();

                // Delete the database record and managed image file.
                bool deleted = _imageService.DeleteImage(image.Id);

                if (!deleted)
                {
                    MessageBox.Show(
                        "The image record could not be found.",
                        "Remove Image",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                LoadLocationImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not remove the image.\n\n" + ex.Message,
                    "Image Removal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoadLocationImages();
            }
        }

        private void SaveImageDetails_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_loadingImageDetails)
                return;

            LocationImage selectedImage =
                LocationImagesList.SelectedItem as LocationImage;

            if (selectedImage == null)
                return;

            try
            {
                ComboBoxItem selectedCategory =
                    ImageCategoryComboBox.SelectedItem as ComboBoxItem;

                selectedImage.Caption =
                    ImageCaptionTextBox.Text.Trim();

                selectedImage.ImageCategory = selectedCategory == null
                    ? "Other"
                    : selectedCategory.Content.ToString();

                selectedImage.IsPrimary =
                    PrimaryImageCheckBox.IsChecked == true;

                // If this image is primary, clear the primary flag on
                // all other images for this location.
                if (selectedImage.IsPrimary)
                {
                    List<LocationImage> images =
                        _imageService.GetImages(_location.Id);

                    foreach (LocationImage otherImage in images)
                    {
                        if (otherImage.Id != selectedImage.Id &&
                            otherImage.IsPrimary)
                        {
                            otherImage.IsPrimary = false;
                            _repository.UpdateImage(otherImage);
                        }
                    }
                }

                _repository.UpdateImage(selectedImage);

                Guid savedImageId = selectedImage.Id;

                LoadLocationImages(savedImageId);

                MessageBox.Show(
                    "Image details saved.",
                    "Image Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save image details.\n\n" + ex.Message,
                    "Image Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a location name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            _location.Name = NameTextBox.Text.Trim();

            ComboBoxItem selectedType =
                LocationTypeComboBox.SelectedItem as ComboBoxItem;

            _location.LocationType = selectedType == null
                ? "Other"
                : selectedType.Content.ToString();

            Guid? parentId = null;

            if (ParentLocationComboBox.SelectedValue is Guid)
            {
                Guid selectedId =
                    (Guid)ParentLocationComboBox.SelectedValue;

                if (selectedId != Guid.Empty)
                    parentId = selectedId;
            }

            if (parentId.HasValue &&
                parentId.Value == _location.Id)
            {
                MessageBox.Show(
                    "A location cannot be its own parent.");

                return;
            }

            _location.ParentLocationId = parentId;
            _location.Description = DescriptionTextBox.Text.Trim();
            _location.Population = PopulationTextBox.Text.Trim();
            _location.Government = GovernmentTextBox.Text.Trim();
            _location.NotableNPCs = NotableNPCsTextBox.Text.Trim();
            _location.PointsOfInterest = PointsOfInterestTextBox.Text.Trim();
            _location.GMNotes = GMNotesTextBox.Text.Trim();
            _location.LastModified = DateTime.Now;

            try
            {
                if (_isNewLocation)
                {
                    _location.CreatedDate = DateTime.Now;
                    _repository.Add(_location);
                }
                else
                {
                    _repository.Update(_location);
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the location.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

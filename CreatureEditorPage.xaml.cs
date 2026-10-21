
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DnDesktop.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace DnDesktop
{
    public partial class CreatureEditorPage : Page
    {
        private readonly CreatureRepository _repository;
        private readonly Creature _creature;
        private readonly bool _isNewCreature;
        private readonly CreatureImageService _imageService;
        private readonly ObservableCollection<CreatureImage> _images;

        public CreatureEditorPage(Creature creature)
        {
            InitializeComponent();

            _repository = new CreatureRepository();
            _imageService = new CreatureImageService();
            _images = new ObservableCollection<CreatureImage>();
            _isNewCreature = creature == null;
            _creature = creature ?? new Creature();

            EditorTitle.Text = _isNewCreature
                ? "Create Creature"
                : "Edit Creature";

            LoadCreature();
            CreatureImagesListBox.ItemsSource = _images;
            LoadCreatureImages();
        }

        private void LoadCreature()
        {
            NameTextBox.Text = _creature.Name ?? string.Empty;
            SubtypeTextBox.Text = _creature.Subtype ?? string.Empty;
            DescriptionTextBox.Text =
                _creature.Description ?? string.Empty;

            SetComboBoxValue(
                CreatureTypeComboBox,
                _creature.CreatureType,
                "Other");

            SetComboBoxValue(
                SizeComboBox,
                _creature.Size,
                "Medium");

            SetComboBoxValue(
                AlignmentComboBox,
                _creature.Alignment,
                "Unaligned");

            // Combat statistics
            ArmorClassTextBox.Text =
                _creature.ArmorClass.ToString();

            ArmorClassNotesTextBox.Text =
                _creature.ArmorClassNotes ?? string.Empty;

            HitPointsTextBox.Text =
                _creature.HitPoints.ToString();

            HitDiceTextBox.Text =
                _creature.HitDice ?? string.Empty;

            InitiativeModifierTextBox.Text =
                _creature.InitiativeModifier.ToString();

            ChallengeRatingTextBox.Text =
                _creature.ChallengeRating ?? string.Empty;

            ProficiencyBonusTextBox.Text =
                _creature.ProficiencyBonus.ToString();

            ExperiencePointsTextBox.Text =
                _creature.ExperiencePoints.ToString();

            // Ability scores
            StrengthTextBox.Text =
                _creature.Strength.ToString();

            DexterityTextBox.Text =
                _creature.Dexterity.ToString();

            ConstitutionTextBox.Text =
                _creature.Constitution.ToString();

            IntelligenceTextBox.Text =
                _creature.Intelligence.ToString();

            WisdomTextBox.Text =
                _creature.Wisdom.ToString();

            CharismaTextBox.Text =
                _creature.Charisma.ToString();
        }
        private void LoadCreatureImages()
        {
            try
            {
                _images.Clear();

                // A new creature has not been saved yet, so it has no images.
                if (_isNewCreature)
                {
                    CreatureImagePreview.Source = null;
                    return;
                }

                var creatureImages = _imageService.GetImages(_creature.Id);

                foreach (CreatureImage image in creatureImages)
                {
                    _images.Add(image);
                }

                // Show the primary image when opening an existing creature.
                CreatureImage primaryImage =
                    creatureImages.FirstOrDefault(image => image.IsPrimary);

                if (primaryImage != null &&
                    !string.IsNullOrWhiteSpace(primaryImage.ImagePath) &&
                    File.Exists(primaryImage.ImagePath))
                {
                    CreatureImagePreview.Source = new BitmapImage(
                        new Uri(primaryImage.ImagePath, UriKind.Absolute));
                }
                else
                {
                    CreatureImagePreview.Source = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load creature images.\n\n" + ex.Message,
                    "Image Loading Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreatureImagesListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            CreatureImage selectedImage =
                CreatureImagesListBox.SelectedItem as CreatureImage;

            if (selectedImage == null ||
                string.IsNullOrWhiteSpace(selectedImage.ImagePath) ||
                !File.Exists(selectedImage.ImagePath))
            {
                CreatureImagePreview.Source = null;
                return;
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(
                    selectedImage.ImagePath,
                    UriKind.Absolute);
                bitmap.EndInit();

                CreatureImagePreview.Source = bitmap;
            }
            catch (Exception ex)
            {
                CreatureImagePreview.Source = null;

                MessageBox.Show(
                    "Could not display the selected image.\n\n" + ex.Message,
                    "Image Preview Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }


        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            // A new creature must be saved before images can be attached.
            if (_isNewCreature)
            {
                MessageBox.Show(
                    "Please save the creature before importing images.",
                    "Save Creature First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select a Creature Image",
                Filter =
                    "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff|" +
                    "All Files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                CreatureImage importedImage = _imageService.ImportImage(
                    _creature.Id,
                    dialog.FileName,
                    Path.GetFileNameWithoutExtension(dialog.FileName),
                    "Other",
                    _images.Count == 0);

                _images.Add(importedImage);
                CreatureImagesListBox.SelectedItem = importedImage;

                MessageBox.Show(
                    "Image imported successfully.",
                    "Import Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not import the image.\n\n" + ex.Message,
                    "Image Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        private void SetPrimaryImage_Click(object sender, RoutedEventArgs e)
        {
            CreatureImage selectedImage =
                CreatureImagesListBox.SelectedItem as CreatureImage;

            if (selectedImage == null)
            {
                MessageBox.Show(
                    "Please select an image first.",
                    "No Image Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                _imageService.SetPrimaryImage(
                    _creature.Id,
                    selectedImage.Id);

                // Update the in-memory list to reflect the saved selection.
                foreach (CreatureImage image in _images)
                {
                    image.IsPrimary = image.Id == selectedImage.Id;
                }

                // Refresh the preview.
                CreatureImagesListBox.SelectedItem = null;
                CreatureImagesListBox.SelectedItem = selectedImage;

                MessageBox.Show(
                    "The primary image has been updated.",
                    "Primary Image Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not set the primary image.\n\n" + ex.Message,
                    "Image Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            CreatureImage selectedImage =
                CreatureImagesListBox.SelectedItem as CreatureImage;

            if (selectedImage == null)
            {
                MessageBox.Show(
                    "Please select an image to delete.",
                    "No Image Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this image?\n\n" +
                selectedImage.Caption,
                "Confirm Image Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                Guid deletedImageId = selectedImage.Id;

                _imageService.DeleteImage(deletedImageId);

                _images.Remove(selectedImage);
                CreatureImagesListBox.SelectedItem = null;

                // Refresh the preview after deleting the selected image.
                CreatureImagePreview.Source = null;

                MessageBox.Show(
                    "Image deleted successfully.",
                    "Delete Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the image.\n\n" + ex.Message,
                    "Image Deletion Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetComboBoxValue(
            ComboBox comboBox,
            string value,
            string defaultValue)
        {
            string targetValue = string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value;

            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (string.Equals(
                    item.Content.ToString(),
                    targetValue,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
            comboBox.Text = targetValue;
        }

        private string GetComboBoxValue(
            ComboBox comboBox,
            string defaultValue)
        {
            ComboBoxItem selectedItem =
                comboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
                return selectedItem.Content.ToString();

            return string.IsNullOrWhiteSpace(comboBox.Text)
                ? defaultValue
                : comboBox.Text.Trim();
        }

        private bool TryGetInteger(
            TextBox textBox,
            string fieldName,
            out int value)
        {
            string input = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return true;
            }

            if (int.TryParse(input, out value))
                return true;

            MessageBox.Show(
                fieldName + " must be a whole number.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            textBox.Focus();
            textBox.SelectAll();

            return false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a creature name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            int armorClass;
            int hitPoints;
            int initiativeModifier;
            int proficiencyBonus;
            int experiencePoints;

            // Ability score variables
            int strength;
            int dexterity;
            int constitution;
            int intelligence;
            int wisdom;
            int charisma;

            if (!TryGetInteger(
                ArmorClassTextBox,
                "Armor Class",
                out armorClass))
                return;

            if (!TryGetInteger(
                HitPointsTextBox,
                "Hit Points",
                out hitPoints))
                return;

            if (!TryGetInteger(
                InitiativeModifierTextBox,
                "Initiative Modifier",
                out initiativeModifier))
                return;

            if (!TryGetInteger(
                ProficiencyBonusTextBox,
                "Proficiency Bonus",
                out proficiencyBonus))
                return;

            if (!TryGetInteger(
                ExperiencePointsTextBox,
                "Experience Points",
                out experiencePoints))
                return;

            // Validate ability scores
            if (!TryGetAbilityScore(
                StrengthTextBox, "Strength", out strength))
                return;

            if (!TryGetAbilityScore(
                DexterityTextBox, "Dexterity", out dexterity))
                return;

            if (!TryGetAbilityScore(
                ConstitutionTextBox, "Constitution", out constitution))
                return;

            if (!TryGetAbilityScore(
                IntelligenceTextBox, "Intelligence", out intelligence))
                return;

            if (!TryGetAbilityScore(
                WisdomTextBox, "Wisdom", out wisdom))
                return;

            if (!TryGetAbilityScore(
                CharismaTextBox, "Charisma", out charisma))
                return;

            // Identity fields
            _creature.Name = NameTextBox.Text.Trim();

            _creature.CreatureType = GetComboBoxValue(
                CreatureTypeComboBox, "Other");

            _creature.Subtype = SubtypeTextBox.Text.Trim();

            _creature.Size = GetComboBoxValue(
                SizeComboBox, "Medium");

            _creature.Alignment = GetComboBoxValue(
                AlignmentComboBox, "Unaligned");

            _creature.Description =
                DescriptionTextBox.Text.Trim();

            // Combat statistics
            _creature.ArmorClass = armorClass;

            _creature.ArmorClassNotes =
                ArmorClassNotesTextBox.Text.Trim();

            _creature.HitPoints = hitPoints;

            _creature.HitDice =
                HitDiceTextBox.Text.Trim();

            _creature.InitiativeModifier = initiativeModifier;

            _creature.ChallengeRating =
                ChallengeRatingTextBox.Text.Trim();

            _creature.ProficiencyBonus = proficiencyBonus;

            _creature.ExperiencePoints = experiencePoints;

            // Save ability scores
            _creature.Strength = strength;
            _creature.Dexterity = dexterity;
            _creature.Constitution = constitution;
            _creature.Intelligence = intelligence;
            _creature.Wisdom = wisdom;
            _creature.Charisma = charisma;

            _creature.LastModified = DateTime.Now;

            try
            {
                if (_isNewCreature)
                {
                    _creature.CreatedDate = DateTime.Now;
                    _repository.Add(_creature);
                }
                else
                {
                    _repository.Update(_creature);
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the creature.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Ability scores must be between 1 and 30.
        private bool TryGetAbilityScore(
            TextBox textBox,
            string abilityName,
            out int score)
        {
            if (!TryGetInteger(textBox, abilityName, out score))
                return false;

            if (score < 1 || score > 30)
            {
                MessageBox.Show(
                    abilityName + " must be between 1 and 30.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                textBox.Focus();
                textBox.SelectAll();

                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

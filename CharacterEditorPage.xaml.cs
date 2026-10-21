
using DnDesktop.Models;
using DnDesktop.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using DnDesktop.Services;
using Microsoft.Win32;
using System.IO;

namespace DnDesktop
{
    public partial class CharacterEditorPage : Page
    {
        private readonly CharacterRepository _repository =
            new CharacterRepository();

        private readonly ObservableCollection<Character> _characters;
        private readonly Character _character;
        private readonly bool _isNewCharacter;

        public CharacterEditorPage(
            ObservableCollection<Character> characters,
            Character character)
        {
            InitializeComponent();

            _characters = characters;

            if (character == null)
            {
                _character = new Character();
                _isNewCharacter = true;
                PageTitle.Text = "Create Character";
            }
            else
            {
                _character = character;
                _isNewCharacter = false;
                PageTitle.Text = "Edit Character";
            }

            if (_character.AbilityScores == null)
                _character.AbilityScores = new AbilityScores();

            if (_character.Combat == null)
                _character.Combat = new CombatStats();

            if (_character.Feats == null)
                _character.Feats = new List<Feat>();

            LoadCharacter();
            RefreshFeatsList();
            RefreshCharacterImages();
        }

        private void LoadCharacter()
        {
            NameTextBox.Text = _character.Name ?? string.Empty;
            PlayerNameTextBox.Text = _character.PlayerName ?? string.Empty;
            RaceTextBox.Text = _character.Race ?? string.Empty;
            ClassTextBox.Text = _character.CharacterClass ?? string.Empty;
            SubraceTextBox.Text = _character.Subrace ?? string.Empty;
            SubclassTextBox.Text = _character.Subclass ?? string.Empty;
            LevelTextBox.Text = _character.Level.ToString();
            BackgroundTextBox.Text = _character.Background ?? string.Empty;

            ArmorClassTextBox.Text =
                _character.Combat.ArmorClass.ToString();

            HitPointsTextBox.Text =
                _character.Combat.MaxHitPoints.ToString();

            HitDiceTextBox.Text =
                _character.Combat.HitDice.ToString();

            InitiativeModifierTextBox.Text =
                _character.Combat.Initiative.ToString();

            StrengthTextBox.Text =
                _character.AbilityScores.Strength.ToString();

            DexterityTextBox.Text =
                _character.AbilityScores.Dexterity.ToString();

            ConstitutionTextBox.Text =
                _character.AbilityScores.Constitution.ToString();

            IntelligenceTextBox.Text =
                _character.AbilityScores.Intelligence.ToString();

            WisdomTextBox.Text =
                _character.AbilityScores.Wisdom.ToString();

            CharismaTextBox.Text =
                _character.AbilityScores.Charisma.ToString();
        }

        private void RefreshFeatsList()
        {
            FeatsListBox.ItemsSource = null;
            FeatsListBox.ItemsSource = _character.Feats;
        }

        private void AddFeat_Click(
            object sender,
            RoutedEventArgs e)
        {
            var feat = new Feat
            {
                Name = string.Empty,
                Description = string.Empty
            };

            if (ShowFeatDialog(feat, "Add Feat"))
            {
                _character.Feats.Add(feat);
                RefreshFeatsList();
                FeatsListBox.SelectedItem = feat;
            }
        }

        private void EditFeat_Click(
            object sender,
            RoutedEventArgs e)
        {
            Feat selectedFeat = FeatsListBox.SelectedItem as Feat;

            if (selectedFeat == null)
            {
                MessageBox.Show(
                    "Select a feat to edit.",
                    "No Feat Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Edit a copy so Cancel does not change the original feat.
            var editedFeat = new Feat
            {
                Name = selectedFeat.Name,
                Description = selectedFeat.Description
            };

            if (ShowFeatDialog(editedFeat, "Edit Feat"))
            {
                selectedFeat.Name = editedFeat.Name;
                selectedFeat.Description = editedFeat.Description;

                RefreshFeatsList();
                FeatsListBox.SelectedItem = selectedFeat;
            }
        }

        private void DeleteFeat_Click(
            object sender,
            RoutedEventArgs e)
        {
            Feat selectedFeat = FeatsListBox.SelectedItem as Feat;

            if (selectedFeat == null)
            {
                MessageBox.Show(
                    "Select a feat to delete.",
                    "No Feat Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Delete the feat '" + selectedFeat.Name + "'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _character.Feats.Remove(selectedFeat);
                RefreshFeatsList();
            }
        }

        private bool ShowFeatDialog(Feat feat, string title)
        {
            var nameTextBox = new TextBox
            {
                Text = feat.Name ?? string.Empty,
                MinWidth = 320,
                Margin = new Thickness(0, 4, 0, 12),
                Padding = new Thickness(6)
            };

            var descriptionTextBox = new TextBox
            {
                Text = feat.Description ?? string.Empty,
                MinWidth = 320,
                Height = 120,
                Margin = new Thickness(0, 4, 0, 12),
                Padding = new Thickness(6),
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                AcceptsTab = true
            };

            var content = new StackPanel
            {
                Margin = new Thickness(18)
            };

            content.Children.Add(new TextBlock
            {
                Text = "Feat Name",
                FontWeight = FontWeights.Bold
            });
            content.Children.Add(nameTextBox);

            content.Children.Add(new TextBlock
            {
                Text = "Description",
                FontWeight = FontWeights.Bold
            });
            content.Children.Add(descriptionTextBox);

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var saveButton = new Button
            {
                Content = "OK",
                Width = 85,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 85,
                Height = 30,
                IsCancel = true
            };

            buttons.Children.Add(saveButton);
            buttons.Children.Add(cancelButton);
            content.Children.Add(buttons);

            var dialog = new Window
            {
                Title = title,
                Content = content,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            saveButton.Click += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show(
                        dialog,
                        "Enter a name for the feat.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    nameTextBox.Focus();
                    return;
                }

                feat.Name = nameTextBox.Text.Trim();
                feat.Description = descriptionTextBox.Text.Trim();

                dialog.DialogResult = true;
            };

            nameTextBox.Focus();

            return dialog.ShowDialog() == true;
        }

        private bool TryGetInteger(
            TextBox textBox,
            string fieldName,
            out int value)
        {
            if (int.TryParse(textBox.Text.Trim(), out value))
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

        private void SaveCharacter_Click(
            object sender,
            RoutedEventArgs e)
        {
            int level;

            if (!int.TryParse(LevelTextBox.Text, out level)
                || level < 1 || level > 20)
            {
                MessageBox.Show(
                    "Enter a character level between 1 and 20.");

                LevelTextBox.Focus();
                LevelTextBox.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a character name.");
                NameTextBox.Focus();
                return;
            }

            int armorClass;
            int hitPoints;
            int hitDice;
            int initiative;

            if (!TryGetInteger(
                ArmorClassTextBox, "Armor Class", out armorClass))
                return;

            if (armorClass < 0)
            {
                MessageBox.Show("Armor Class cannot be negative.");
                ArmorClassTextBox.Focus();
                ArmorClassTextBox.SelectAll();
                return;
            }

            if (!TryGetInteger(
                HitPointsTextBox, "Maximum Hit Points", out hitPoints))
                return;

            if (hitPoints < 1)
            {
                MessageBox.Show("Maximum Hit Points must be at least 1.");
                HitPointsTextBox.Focus();
                HitPointsTextBox.SelectAll();
                return;
            }

            if (!TryGetInteger(
                HitDiceTextBox, "Hit Dice", out hitDice))
                return;

            if (hitDice < 1)
            {
                MessageBox.Show("Hit Dice must be at least 1.");
                HitDiceTextBox.Focus();
                HitDiceTextBox.SelectAll();
                return;
            }

            if (!TryGetInteger(
                InitiativeModifierTextBox,
                "Initiative Modifier", out initiative))
                return;

            int strength;
            int dexterity;
            int constitution;
            int intelligence;
            int wisdom;
            int charisma;

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

            _character.Name = NameTextBox.Text.Trim();
            _character.PlayerName = PlayerNameTextBox.Text.Trim();
            _character.Race = RaceTextBox.Text.Trim();
            _character.CharacterClass = ClassTextBox.Text.Trim();
            _character.Subrace = SubraceTextBox.Text.Trim();
            _character.Subclass = SubclassTextBox.Text.Trim();
            _character.Level = level;
            _character.Background = BackgroundTextBox.Text.Trim();

            _character.Combat.ArmorClass = armorClass;
            _character.Combat.MaxHitPoints = hitPoints;
            _character.Combat.HitDice = hitDice;
            _character.Combat.Initiative = initiative;

            if (_character.Combat.CurrentHitPoints > hitPoints)
                _character.Combat.CurrentHitPoints = hitPoints;

            if (_isNewCharacter)
                _character.Combat.CurrentHitPoints = hitPoints;

            _character.AbilityScores.Strength = strength;
            _character.AbilityScores.Dexterity = dexterity;
            _character.AbilityScores.Constitution = constitution;
            _character.AbilityScores.Intelligence = intelligence;
            _character.AbilityScores.Wisdom = wisdom;
            _character.AbilityScores.Charisma = charisma;

            _character.LastModified = DateTime.Now;

            try
            {
                if (_isNewCharacter)
                {
                    _character.Id = Guid.NewGuid();
                    _character.CreatedDate = DateTime.Now;

                    _repository.Add(_character);
                }
                else
                {
                    _repository.Update(_character);
                }

                _characters.Clear();

                foreach (Character character in _repository.GetAll())
                {
                    _characters.Add(character);
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the character.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
        private void TestCharacterImageService_Click(
            object sender, RoutedEventArgs e)
        {
            if (_character == null || _character.Id == Guid.Empty)
            {
                MessageBox.Show(
                    "Open an existing, saved character before running this test.",
                    "Character Image Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select a test image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff"
            };

            if (dialog.ShowDialog() != true)
                return;

            CharacterImageService service = new CharacterImageService();
            DnDesktop.Models.CharacterImage importedImage = null;

            try
            {
                // Import the image and save its metadata.
                importedImage = service.ImportImage(
                    _character.Id,
                    dialog.FileName,
                    "Temporary service test",
                    "Other",
                    false);

                // Confirm the stored file exists.
                bool fileExists = File.Exists(importedImage.ImagePath);

                // Confirm the image can be retrieved from SQLite.
                var images = service.GetImages(_character.Id);

                bool recordExists = images.Exists(
                    image => image.Id == importedImage.Id);

                if (!fileExists || !recordExists)
                {
                    throw new InvalidOperationException(
                        "The image file or database record could not be verified.");
                }

                // Delete only the imported copy and its database record.
                bool deleted = service.DeleteImage(importedImage.Id);

                if (!deleted)
                {
                    throw new InvalidOperationException(
                        "The imported image record could not be deleted.");
                }

                importedImage = null;

                MessageBox.Show(
                    "Character image service test passed!\n\n" +
                    "Image copied to managed storage: Yes\n" +
                    "Database record created and retrieved: Yes\n" +
                    "Managed copy and test record deleted: Yes\n\n" +
                    "Your original image was not modified.",
                    "Character Image Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Clean up the test import if it succeeded but a later check failed.
                if (importedImage != null)
                {
                    try
                    {
                        service.DeleteImage(importedImage.Id);
                    }
                    catch
                    {
                        // Preserve the original test error for reporting.
                    }
                }

                MessageBox.Show(
                    "Character image service test failed:\n\n" + ex.Message,
                    "Character Image Test",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshCharacterImages()
        {
            CharacterImagesListBox.ItemsSource = null;

            if (_character == null || _character.Id == Guid.Empty)
            {
                CharacterImagesListBox.ItemsSource =
                    new List<CharacterImage>();
                return;
            }

            try
            {
                CharacterImageService service =
                    new CharacterImageService();

                CharacterImagesListBox.ItemsSource =
                    service.GetImages(_character.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load character images.\n\n" + ex.Message,
                    "Image Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ImportCharacterImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_isNewCharacter || _character.Id == Guid.Empty)
            {
                MessageBox.Show(
                    "Save the character before importing images. " +
                    "After saving, reopen the character to manage its images.",
                    "Save Character First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select a Character Image",
                Filter =
                    "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif;*.tiff",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            string caption;
            string category;

            if (!ShowImageDetailsDialog(
                Path.GetFileNameWithoutExtension(dialog.FileName),
                out caption,
                out category))
            {
                return;
            }

            try
            {
                CharacterImageService service =
                    new CharacterImageService();

                service.ImportImage(
                    _character.Id,
                    dialog.FileName,
                    caption,
                    category,
                    false);

                RefreshCharacterImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not import the character image.\n\n" + ex.Message,
                    "Image Import Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private bool ShowImageDetailsDialog(
            string defaultCaption,
            out string caption,
            out string category)
        {
            caption = string.Empty;
            category = "Other";

            TextBox captionTextBox = new TextBox
            {
                Text = defaultCaption,
                MinWidth = 300,
                Margin = new Thickness(0, 4, 0, 12),
                Padding = new Thickness(6)
            };

            ComboBox categoryComboBox = new ComboBox
            {
                MinWidth = 300,
                Margin = new Thickness(0, 4, 0, 12),
                Padding = new Thickness(4),
                IsEditable = false
            };

            string[] categories =
            {
                "Portrait",
                "Appearance",
                "Equipment",
                "Reference",
                "Other"
            };

            foreach (string item in categories)
                categoryComboBox.Items.Add(item);

            categoryComboBox.SelectedItem = "Portrait";

            StackPanel content = new StackPanel
            {
                Margin = new Thickness(18)
            };

            content.Children.Add(new TextBlock
            {
                Text = "Caption",
                FontWeight = FontWeights.Bold
            });
            content.Children.Add(captionTextBox);

            content.Children.Add(new TextBlock
            {
                Text = "Image Category",
                FontWeight = FontWeights.Bold
            });
            content.Children.Add(categoryComboBox);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Button importButton = new Button
            {
                Content = "Import",
                Width = 85,
                Height = 30,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 85,
                Height = 30,
                IsCancel = true
            };

            buttons.Children.Add(importButton);
            buttons.Children.Add(cancelButton);
            content.Children.Add(buttons);

            Window dialog = new Window
            {
                Title = "Image Details",
                Content = content,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            // Store the values in local variables inside the event handler.
            string selectedCaption = string.Empty;
            string selectedCategory = "Other";

            importButton.Click += (sender, e) =>
            {
                selectedCaption = captionTextBox.Text.Trim();

                selectedCategory =
                    categoryComboBox.SelectedItem as string ?? "Other";

                dialog.DialogResult = true;
            };

            captionTextBox.Focus();

            bool confirmed = dialog.ShowDialog() == true;

            // Assign the out parameters after the lambda has finished.
            if (confirmed)
            {
                caption = selectedCaption;
                category = selectedCategory;
            }

            return confirmed;
        }


        private void SetPrimaryCharacterImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            CharacterImage selectedImage =
                CharacterImagesListBox.SelectedItem as CharacterImage;

            if (selectedImage == null)
            {
                MessageBox.Show(
                    "Select an image to make it the primary image.",
                    "No Image Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                CharacterImageService service =
                    new CharacterImageService();

                CharacterImageRepository repository =
                    new CharacterImageRepository();

                List<CharacterImage> images =
                    service.GetImages(_character.Id);

                foreach (CharacterImage image in images)
                {
                    bool shouldBePrimary = image.Id == selectedImage.Id;

                    if (image.IsPrimary != shouldBePrimary)
                    {
                        image.IsPrimary = shouldBePrimary;
                        repository.UpdateImage(image);
                    }
                }

                RefreshCharacterImages();

                MessageBox.Show(
                    "Primary character image updated.",
                    "Image Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not update the primary image.\n\n" + ex.Message,
                    "Image Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                RefreshCharacterImages();
            }
        }

        private void DeleteCharacterImage_Click(
            object sender,
            RoutedEventArgs e)
        {
            CharacterImage selectedImage =
                CharacterImagesListBox.SelectedItem as CharacterImage;

            if (selectedImage == null)
            {
                MessageBox.Show(
                    "Select an image to delete.",
                    "No Image Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Delete the selected character image?\n\n" +
                "The managed copy and its database record will be deleted. " +
                "Your original source image will not be affected.",
                "Confirm Image Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                CharacterImageService service =
                    new CharacterImageService();

                bool deleted = service.DeleteImage(selectedImage.Id);

                if (!deleted)
                {
                    MessageBox.Show(
                        "The image record could not be found.",
                        "Image Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                RefreshCharacterImages();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the character image.\n\n" + ex.Message,
                    "Image Deletion Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                RefreshCharacterImages();
            }
        }
    }
}

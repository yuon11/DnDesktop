using DnDesktop.Models;
using DnDesktop.Data;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DnDesktop
{
    public partial class CharacterEditorPage : Page
    {
        private readonly CharacterRepository _repository = new CharacterRepository();
        private readonly ObservableCollection<Character> _characters;

        private readonly Character _character;

        private readonly bool _isNewCharacter;


        public CharacterEditorPage(
            ObservableCollection<Character> characters,
            Character? character)
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

            LoadCharacter();
        }


        private void LoadCharacter()
        {
            NameTextBox.Text = _character.Name;

            PlayerNameTextBox.Text = _character.PlayerName;

            RaceTextBox.Text = _character.Race;

            ClassTextBox.Text = _character.CharacterClass;

            SubraceTextBox.Text = _character.Subrace;

            SubclassTextBox.Text = _character.Subclass;

            LevelTextBox.Text =
                _character.Level.ToString();

            BackgroundTextBox.Text =
                _character.Background;
        }



        private void SaveCharacter_Click(object sender,RoutedEventArgs e)
        {
            int level;

            if (!int.TryParse(LevelTextBox.Text, out level)
                || level < 1 || level > 20)
            {
                MessageBox.Show(
                    "Enter a character level between 1 and 20.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter a character name.");
                return;
            }

            // Populate the character object from the editor.

            _character.Name = NameTextBox.Text.Trim();
            _character.PlayerName = PlayerNameTextBox.Text.Trim();
            _character.Race = RaceTextBox.Text.Trim();
            _character.CharacterClass = ClassTextBox.Text.Trim();
            _character.Subrace = SubraceTextBox.Text.Trim();
            _character.Subclass = SubclassTextBox.Text.Trim();
            _character.Level = level;
            _character.Background = BackgroundTextBox.Text.Trim();

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

                // Reload the collection from the database.
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
            NavigationService.GoBack();
        }
    }
}

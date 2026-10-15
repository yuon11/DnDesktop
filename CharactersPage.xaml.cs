
using DnDesktop.Data;
using DnDesktop;
using DnDesktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;


namespace DnDesktop
{
    public partial class CharactersPage : Page
    {

        private void CharactersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCharacters();
        }

        public ObservableCollection<Character> Characters
        {
            get;
            set;
        }

        private readonly CharacterRepository _repository;

        public CharactersPage()
        {
            InitializeComponent();

            _repository = new CharacterRepository();
            Characters = new ObservableCollection<Character>();

            DataContext = this;

            LoadCharacters();
        }


        public void RefreshCharacters()
        {
            try
            {
                Characters.Clear();

                foreach (Character character in _repository.GetAll())
                {
                    Characters.Add(character);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not refresh characters.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        private void LoadCharacters()
        {
            RefreshCharacters();
        }

        private Character GetSelectedCharacter()
        {
            return CharacterList.SelectedItem as Character;
        }

        private void CreateCharacter_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new CharacterEditorPage(Characters, null));
        }

        private void EditCharacter_Click(object sender, RoutedEventArgs e)
        {
            Character character = GetSelectedCharacter();

            if (character == null)
            {
                MessageBox.Show("Please select a character to edit.");
                return;
            }

            NavigationService.Navigate(
                new CharacterEditorPage(Characters, character));
        }

        private void DeleteCharacter_Click(
            object sender,
            RoutedEventArgs e)
        {
            Character character = GetSelectedCharacter();

            if (character == null)
            {
                MessageBox.Show("Please select a character to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete "
                + character.Name + "?",
                "Delete Character",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.Delete(character.Id);

                Characters.Remove(character);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the character.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CharacterList_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            Character character = GetSelectedCharacter();

            if (character == null)
                return;

            NavigationService.Navigate(
                new CharacterEditorPage(Characters, character));
        }

    }
}

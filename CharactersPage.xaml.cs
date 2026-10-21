
using DnDesktop.Data;
using DnDesktop.Models;
using DnDesktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private Character _selectedCharacter;

        public Character SelectedCharacter
        {
            get { return _selectedCharacter; }
            set { _selectedCharacter = value; }
        }
        private readonly CharacterRepository _repository;
        private readonly CharacterImageService _characterImageService;

        public CharactersPage()
        {
            InitializeComponent();

            _repository = new CharacterRepository();
            _characterImageService = new CharacterImageService();

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
                    List<CharacterImage> images =
                        _characterImageService.GetImages(character.Id);

                    CharacterImage primaryImage =
                        images.FirstOrDefault(image => image.IsPrimary);

                    character.PrimaryImagePath =
                        primaryImage != null
                            ? primaryImage.ImagePath
                            : string.Empty;

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
            return SelectedCharacter;
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

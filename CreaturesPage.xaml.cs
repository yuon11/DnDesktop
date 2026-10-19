
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace DnDesktop
{
    public partial class CreaturesPage : Page
    {
        private readonly CreatureRepository _repository;

        public ObservableCollection<Creature> Creatures { get; private set; }

        public CreaturesPage()
        {
            InitializeComponent();

            _repository = new CreatureRepository();
            Creatures = new ObservableCollection<Creature>();

            CreatureList.ItemsSource = Creatures;

            Loaded += CreaturesPage_Loaded;

            LoadCreatures();
        }

        private void CreaturesPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCreatures();
        }


        /// <summary>
        /// Load creatures from the database and refresh the displayed list.
        /// </summary>
        private void LoadCreatures()
        {
            try
            {
                var creatures = _repository.GetAll();

                Creatures.Clear();

                foreach (Creature creature in creatures)
                {
                    Creatures.Add(creature);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load creatures.\n\n" + ex.Message,
                    "Creature Loading Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Open the editor to create a new creature.
        /// </summary>
        private void CreateCreature_Click(
            object sender,
            RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new CreatureEditorPage(null));
        }

        /// <summary>
        /// Open the editor for the selected creature.
        /// </summary>
        private void EditCreature_Click(
            object sender,
            RoutedEventArgs e)
        {
            Creature selected =
                CreatureList.SelectedItem as Creature;

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select a creature to edit.",
                    "No Creature Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            NavigationService.Navigate(
                new CreatureEditorPage(selected));
        }

        /// <summary>
        /// Delete the selected creature after confirmation.
        /// </summary>
        private void DeleteCreature_Click(
            object sender,
            RoutedEventArgs e)
        {
            Creature selected =
                CreatureList.SelectedItem as Creature;

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select a creature to delete.",
                    "No Creature Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete '" +
                selected.Name +
                "'?\n\nThis will also delete its associated attacks " +
                "and abilities.",
                "Delete Creature",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.Delete(selected.Id);

                LoadCreatures();

                MessageBox.Show(
                    "Creature deleted successfully.",
                    "Creature Deleted",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the creature.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Allow double-clicking a creature to edit it.
        /// </summary>
        private void CreatureList_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (CreatureList.SelectedItem is Creature)
            {
                EditCreature_Click(sender, e);
            }
        }
    }
}

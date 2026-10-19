using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using DnDesktop.Data;
using DnDesktop.Models;
using DnDesktop.ViewModels;


namespace DnDesktop
{
    /// <summary>
    /// Interaction logic for LocationsPage.xaml
    /// </summary>
    public partial class LocationsPage : Page
    {
        public ObservableCollection<LocationListItem> Locations
        { get; set; }

        private readonly LocationRepository _repository;

        public LocationsPage()
        {
            InitializeComponent();

            _repository = new LocationRepository();
            Locations = new ObservableCollection<LocationListItem>();

            DataContext = this;
            LoadLocations();
        }

        private void LocationsPage_Loaded(
            object sender, RoutedEventArgs e)
        {
            LoadLocations();
        }

        private void LoadLocations()
        {
            try
            {
                Locations.Clear();

                List<Location> allLocations = _repository.GetAll();

                foreach (Location location in allLocations)
                {
                    Location parent = null;

                    if (location.ParentLocationId.HasValue)
                    {
                        parent = allLocations.FirstOrDefault(
                            x => x.Id == location.ParentLocationId.Value);
                    }

                    List<LocationImage> images = _repository.GetImages(location.Id);

                    int imageCount = images.Count;

                    LocationImage primaryImage = images.FirstOrDefault(x => x.IsPrimary) ?? images.FirstOrDefault();

                    string primaryImagePath = primaryImage == null ? null : primaryImage.ImagePath;

                    Locations.Add(new LocationListItem
                    {
                        Location = location,
                        ParentLocationName = parent == null
                            ? string.Empty : parent.Name,
                        ImageCount = imageCount,
                        PrimaryImagePath = primaryImagePath
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load locations.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private LocationListItem GetSelectedLocation()
        {
            return LocationList.SelectedItem as LocationListItem;
        }

        private void CreateLocation_Click(
            object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "The location editor will be implemented in the next step.");
            NavigationService.Navigate(new LocationEditorPage(null));
        }

        private void EditLocation_Click(object sender, RoutedEventArgs e)
        {
            LocationListItem selected = GetSelectedLocation();

            if (selected == null)
            {
                MessageBox.Show("Please select a location to edit.");
                return;
            }

            NavigationService.Navigate(
                new LocationEditorPage(selected.Location));
        }

        private void LocationList_MouseDoubleClick(
            object sender, MouseButtonEventArgs e)
        {
            if (GetSelectedLocation() != null)
            {
                EditLocation_Click(sender, e);
            }
        }

        private void DeleteLocation_Click(
            object sender, RoutedEventArgs e)
        {
            LocationListItem selected = GetSelectedLocation();

            if (selected == null)
            {
                MessageBox.Show("Please select a location to delete.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Delete '" + selected.Name + "'?\n\n" +
                "Any child locations will be preserved without a parent. " +
                "Associated image records will be removed.",
                "Delete Location",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.Delete(selected.Id);
                LoadLocations();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the location.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

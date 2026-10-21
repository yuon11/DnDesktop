using System;
using System.Windows;
using System.IO;
using DnDesktop.Services;


namespace DnDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void SelectCampaignsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CampaignsPage());
        }

        void SelectCharactersClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CharactersPage());
        }

        //void SelectBattleMapClick(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("SelectBattleMapClick clicked!");
        //}

        void SelectLocationsClick(object sender, RoutedEventArgs e)
        {
           //MessageBox.Show("SelectLocationsClick clicked!");
            MainFrame.Navigate(new LocationsPage());
        }

        void SelectGeneralNotesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectGeneralNotesClick clicked!");
        }

        private void CharactersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CharactersPage());
        }

        void SelectCreaturesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CreaturesPage());
        }

        void SelectMainMenuClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectMainMenuClick clicked!");
        }
    }
}

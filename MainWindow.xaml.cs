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
            MessageBox.Show("SelectCampaignsClick clicked!");
        }

        void SelectCharactersClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectCharactersClick clicked!");
            MainFrame.Navigate(new CharactersPage());
        }

        void SelectBattleMapClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectBattleMapClick clicked!");
        }

        void SelectLocationsClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectLocationsClick clicked!");
        }

        void SelectGeneralNotesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectGeneralNotesClick clicked!");
        }

        void SelectQuestsClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("SelectQuestsClick clicked!");
        }

        private void CharactersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CharactersPage());
        }
    }
}

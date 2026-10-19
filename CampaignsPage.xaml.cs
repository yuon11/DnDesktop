
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DnDesktop
{
    public partial class CampaignsPage : Page
    {
        private readonly CampaignRepository _repository =
            new CampaignRepository();

        public CampaignsPage()
        {
            InitializeComponent();
            LoadCampaigns();
        }

        private void LoadCampaigns()
        {
            try
            {
                CampaignsListView.ItemsSource = _repository.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load campaigns.\n\n" + ex.Message,
                    "Campaigns",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private Campaign GetSelectedCampaign()
        {
            Campaign campaign =
                CampaignsListView.SelectedItem as Campaign;

            if (campaign == null)
            {
                MessageBox.Show(
                    "Please select a campaign first.",
                    "Campaigns",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return campaign;
        }

        private void CreateCampaign_Click(
            object sender, RoutedEventArgs e)
        {
            CampaignEditorPage editor =
                new CampaignEditorPage();

            NavigationService.Navigate(editor);
        }

        private void EditCampaign_Click(
            object sender, RoutedEventArgs e)
        {
            Campaign campaign = GetSelectedCampaign();

            if (campaign == null)
                return;

            NavigationService.Navigate(
                new CampaignEditorPage(campaign.Id));
        }

        private void DeleteCampaign_Click(
            object sender, RoutedEventArgs e)
        {
            Campaign campaign = GetSelectedCampaign();

            if (campaign == null)
                return;

            MessageBoxResult result = MessageBox.Show(
                "Delete the campaign '" + campaign.Name + "'?\n\n" +
                "This cannot be undone.",
                "Confirm Campaign Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.Delete(campaign.Id);
                LoadCampaigns();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the campaign.\n\n" + ex.Message,
                    "Campaigns",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

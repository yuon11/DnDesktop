
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DnDesktop
{
    public partial class CampaignSessionEditorWindow : Window
    {
        private readonly CampaignSessionRepository _repository =
            new CampaignSessionRepository();

        private readonly SessionEncounterRepository _encounterRepository =
            new SessionEncounterRepository();

        private readonly Guid _campaignId;
        private readonly CampaignSession _session;
        private readonly bool _isNewSession;

        // Create a new session.
        public CampaignSessionEditorWindow(Guid campaignId)
        {
            InitializeComponent();

            _campaignId = campaignId;

            _session = new CampaignSession
            {
                CampaignId = campaignId,
                Status = "Planned"
            };

            _isNewSession = true;

            LoadSessionIntoControls();

            // A new session must be saved before encounters can be added.
            EncountersSection.IsEnabled = false;
        }

        // Edit an existing session.
        public CampaignSessionEditorWindow(
            Guid campaignId,
            Guid sessionId)
        {
            InitializeComponent();

            _campaignId = campaignId;
            _session = _repository.GetById(sessionId);
            _isNewSession = false;

            if (_session == null ||
                _session.CampaignId != campaignId)
            {
                MessageBox.Show(
                    "The selected session could not be found for this campaign.",
                    "Session Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Loaded += (sender, args) => Close();
                return;
            }

            LoadSessionIntoControls();

            // This session already exists in the database.
            EncountersSection.IsEnabled = true;

            LoadEncounters();
        }

        private void LoadSessionIntoControls()
        {
            TitleTextBox.Text = _session.Title ?? string.Empty;

            SessionDatePicker.SelectedDate =
                _session.SessionDate;

            SummaryTextBox.Text =
                _session.Summary ?? string.Empty;

            SessionNotesTextBox.Text =
                _session.SessionNotes ?? string.Empty;

            foreach (ComboBoxItem item in StatusComboBox.Items)
            {
                if (string.Equals(
                    item.Content?.ToString(),
                    _session.Status,
                    StringComparison.OrdinalIgnoreCase))
                {
                    StatusComboBox.SelectedItem = item;
                    break;
                }
            }

            if (StatusComboBox.SelectedItem == null)
            {
                StatusComboBox.SelectedIndex = 0;
            }
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(
                    "Please enter a session title.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                TitleTextBox.Focus();
                return;
            }

            ComboBoxItem selectedStatus =
                StatusComboBox.SelectedItem as ComboBoxItem;

            if (selectedStatus == null)
            {
                MessageBox.Show(
                    "Please select a session status.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                _session.CampaignId = _campaignId;
                _session.Title = title;
                _session.SessionDate =
                    SessionDatePicker.SelectedDate;

                _session.Status =
                    selectedStatus.Content.ToString();

                _session.Summary =
                    SummaryTextBox.Text ?? string.Empty;

                _session.SessionNotes =
                    SessionNotesTextBox.Text ?? string.Empty;

                _session.LastModified = DateTime.Now;

                if (_isNewSession)
                {
                    // Persist the session before allowing encounters.
                    _repository.Add(_session);
                }
                else
                {
                    _repository.Update(_session);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the session.\n\n" + ex.Message,
                    "Save Session",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void LoadEncounters()
        {
            try
            {
                EncountersListView.ItemsSource =
                    _encounterRepository.GetBySessionId(
                        _session.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load encounters.\n\n" + ex.Message,
                    "Encounters",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreateEncounter_Click(
            object sender,
            RoutedEventArgs e)
        {
            // Defensive check in addition to the disabled UI.
            if (_isNewSession)
            {
                MessageBox.Show(
                    "Save the session before creating encounters.",
                    "Session Not Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SessionEncounterEditorWindow window =
                new SessionEncounterEditorWindow(_session.Id);

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadEncounters();
            }
        }

        private void EditEncounter_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_isNewSession)
            {
                MessageBox.Show(
                    "Save the session before managing encounters.",
                    "Session Not Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SessionEncounter selected =
                EncountersListView.SelectedItem
                    as SessionEncounter;

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select an encounter to edit.",
                    "No Encounter Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SessionEncounterEditorWindow window =
                new SessionEncounterEditorWindow(
                    _session.Id,
                    selected.Id);

            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                LoadEncounters();
            }
        }

        private void DeleteEncounter_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_isNewSession)
            {
                MessageBox.Show(
                    "Save the session before managing encounters.",
                    "Session Not Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SessionEncounter selected =
                EncountersListView.SelectedItem
                    as SessionEncounter;

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select an encounter to delete.",
                    "No Encounter Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete the encounter '" +
                selected.Name + "'?",
                "Delete Encounter",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _encounterRepository.Delete(selected.Id);
                LoadEncounters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the encounter.\n\n" +
                    ex.Message,
                    "Delete Encounter",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

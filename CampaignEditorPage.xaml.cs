
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DnDesktop
{
    public partial class CampaignEditorPage : Page
    {
        private readonly CampaignSessionRepository _sessionRepository =
            new CampaignSessionRepository();
        private readonly QuestRepository _questRepository =
            new QuestRepository();

        private readonly CampaignRepository _repository =
            new CampaignRepository();

        private readonly CampaignMembershipRepository _membershipRepository =
            new CampaignMembershipRepository();

        private readonly CampaignLocationRepository _locationMembershipRepository =
            new CampaignLocationRepository();

        private readonly LocationRepository _locationRepository =
            new LocationRepository();

        private readonly CharacterRepository _characterRepository =
            new CharacterRepository();

        private readonly Campaign _campaign;
        private readonly bool _isNewCampaign;

        private List<Character> _allCharacters =
            new List<Character>();

        private List<Character> _partyMembers =
            new List<Character>();

        private List<Character> _npcs =
            new List<Character>();

        private List<Location> _allLocations =
            new List<Location>();

        private List<Location> _campaignLocations =
            new List<Location>();

        public CampaignEditorPage()
        {
            InitializeComponent();

            _campaign = new Campaign();
            _isNewCampaign = true;

            PageTitleText.Text = "Create Campaign";

            LoadCharacters();
            LoadLocations();

            RefreshRosterControls();
            RefreshLocationControls();
        }

        public CampaignEditorPage(Guid campaignId)
        {
            InitializeComponent();

            Campaign campaign = _repository.GetById(campaignId);

            if (campaign == null)
            {
                throw new InvalidOperationException(
                    "The selected campaign could not be found.");
            }

            _campaign = campaign;
            _isNewCampaign = false;

            PageTitleText.Text = "Edit Campaign";

            LoadCampaignFields();
            LoadCharacters();
            LoadLocations();

            _partyMembers =
                _membershipRepository.GetPartyMembers(_campaign.Id);

            _npcs =
                _membershipRepository.GetNPCs(_campaign.Id);

            _campaignLocations =
                _locationMembershipRepository.GetByCampaignId(
                    _campaign.Id);

            RefreshRosterControls();
            RefreshLocationControls();

            LoadQuests();
            LoadSessions();
        }

        private void LoadCampaignFields()
        {
            NameTextBox.Text = _campaign.Name;
            DescriptionTextBox.Text = _campaign.Description;
            CampaignNotesTextBox.Text = _campaign.CampaignNotes;
        }

        private void LoadCharacters()
        {
            _allCharacters = _characterRepository.GetAll();
        }

        private void LoadLocations()
        {
            _allLocations = _locationRepository.GetAll();
        }

        private void RefreshRosterControls()
        {
            PartyMembersListBox.ItemsSource = null;
            PartyMembersListBox.ItemsSource = _partyMembers;

            NPCsListBox.ItemsSource = null;
            NPCsListBox.ItemsSource = _npcs;

            var availableCharacters = _allCharacters
                .Where(c =>
                    !_partyMembers.Any(p => p.Id == c.Id) &&
                    !_npcs.Any(n => n.Id == c.Id))
                .OrderBy(c => c.Name)
                .ToList();

            PartyCharacterComboBox.ItemsSource = null;
            PartyCharacterComboBox.ItemsSource = availableCharacters;

            NPCCharacterComboBox.ItemsSource = null;
            NPCCharacterComboBox.ItemsSource = availableCharacters;
        }

        private void RefreshLocationControls()
        {
            CampaignLocationsListBox.ItemsSource = null;

            CampaignLocationsListBox.ItemsSource =
                _campaignLocations.OrderBy(l => l.Name).ToList();

            var availableLocations = _allLocations
                .Where(location =>
                    !_campaignLocations.Any(
                        assigned => assigned.Id == location.Id))
                .OrderBy(location => location.Name)
                .ToList();

            LocationComboBox.ItemsSource = null;
            LocationComboBox.ItemsSource = availableLocations;
        }

        private void AddLocation_Click(
            object sender, RoutedEventArgs e)
        {
            Location location =
                LocationComboBox.SelectedItem as Location;

            if (location == null)
            {
                MessageBox.Show(
                    "Select a location to add to this campaign.",
                    "Locations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (_campaignLocations.Any(
                existing => existing.Id == location.Id))
            {
                MessageBox.Show(
                    "This location is already associated with the campaign.",
                    "Locations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _campaignLocations.Add(location);
            RefreshLocationControls();
        }

        private void RemoveLocation_Click(
            object sender, RoutedEventArgs e)
        {
            Location location =
                CampaignLocationsListBox.SelectedItem as Location;

            if (location == null)
            {
                MessageBox.Show(
                    "Select a location to remove from this campaign.",
                    "Locations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _campaignLocations.RemoveAll(
                existing => existing.Id == location.Id);

            RefreshLocationControls();
        }

        private void AddPartyMember_Click(
            object sender, RoutedEventArgs e)
        {
            Character character =
                PartyCharacterComboBox.SelectedItem as Character;

            if (character == null)
            {
                MessageBox.Show(
                    "Select a character to add to the party.",
                    "Main Party",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _partyMembers.Add(character);
            RefreshRosterControls();
        }

        private void RemovePartyMember_Click(
            object sender, RoutedEventArgs e)
        {
            Character character =
                PartyMembersListBox.SelectedItem as Character;

            if (character == null)
            {
                MessageBox.Show(
                    "Select a party member to remove.",
                    "Main Party",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _partyMembers.RemoveAll(c => c.Id == character.Id);
            RefreshRosterControls();
        }

        private void AddNPC_Click(
            object sender, RoutedEventArgs e)
        {
            Character character =
                NPCCharacterComboBox.SelectedItem as Character;

            if (character == null)
            {
                MessageBox.Show(
                    "Select a character to add as an NPC.",
                    "NPCs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _npcs.Add(character);
            RefreshRosterControls();
        }

        private void RemoveNPC_Click(
            object sender, RoutedEventArgs e)
        {
            Character character =
                NPCsListBox.SelectedItem as Character;

            if (character == null)
            {
                MessageBox.Show(
                    "Select an NPC to remove.",
                    "NPCs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _npcs.RemoveAll(c => c.Id == character.Id);
            RefreshRosterControls();
        }

        private void SaveCampaign_Click(
            object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a campaign name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            _campaign.Name = NameTextBox.Text.Trim();
            _campaign.Description = DescriptionTextBox.Text;
            _campaign.CampaignNotes = CampaignNotesTextBox.Text;

            try
            {
                if (_isNewCampaign)
                {
                    _repository.Add(_campaign);
                }
                else
                {
                    _repository.Update(_campaign);
                }

                _membershipRepository.SaveMemberships(
                    _campaign.Id,
                    _partyMembers.Select(c => c.Id),
                    _npcs.Select(c => c.Id));

                _locationMembershipRepository.SaveLocations(
                    _campaign.Id,
                    _campaignLocations.Select(l => l.Id));

                NavigationService.Navigate(new CampaignsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the campaign, its rosters, " +
                    "or its locations.\n\n" + ex.Message,
                    "Campaign Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CampaignsPage());
        }

        private void LoadQuests()
        {
            try
            {
                QuestsListView.ItemsSource =
                    _questRepository.GetByCampaignId(_campaign.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load quests.\n\n" + ex.Message,
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreateQuest_Click(
            object sender, RoutedEventArgs e)
        {
            if (_isNewCampaign)
            {
                MessageBox.Show(
                    "Save the campaign before creating quests.",
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                var editor = new QuestEditorWindow(
                    _campaign.Id,
                    _characterRepository.GetAll())
                {
                    Owner = Window.GetWindow(this)
                };

                if (editor.ShowDialog() == true)
                    LoadQuests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not open the quest editor.\n\n" + ex.Message,
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditQuest_Click(
            object sender, RoutedEventArgs e)
        {
            Quest quest = QuestsListView.SelectedItem as Quest;

            if (quest == null)
            {
                MessageBox.Show(
                    "Select a quest to edit.",
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                var editor = new QuestEditorWindow(
                    _campaign.Id,
                    quest.Id,
                    _characterRepository.GetAll())
                {
                    Owner = Window.GetWindow(this)
                };

                if (editor.ShowDialog() == true)
                    LoadQuests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not open the quest editor.\n\n" + ex.Message,
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteQuest_Click(
            object sender, RoutedEventArgs e)
        {
            Quest quest = QuestsListView.SelectedItem as Quest;

            if (quest == null)
            {
                MessageBox.Show(
                    "Select a quest to delete.",
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Delete the quest '" + quest.Name + "'?",
                "Confirm Quest Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _questRepository.Delete(quest.Id);
                LoadQuests();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the quest.\n\n" + ex.Message,
                    "Quests",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void LoadSessions()
        {
            try
            {
                SessionsListView.ItemsSource =
                    _sessionRepository.GetByCampaignId(_campaign.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load sessions.\n\n" + ex.Message,
                    "Sessions",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void CreateSession_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewCampaign)
            {
                MessageBox.Show(
                    "Please save the campaign before creating sessions.",
                    "Save Campaign First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            CampaignSessionEditorWindow window =
                new CampaignSessionEditorWindow(_campaign.Id);

            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                LoadSessions();
            }
        }

        private void EditSession_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewCampaign)
            {
                MessageBox.Show(
                    "Please save the campaign before editing sessions.",
                    "Save Campaign First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            CampaignSession selectedSession =
                SessionsListView.SelectedItem as CampaignSession;

            if (selectedSession == null)
            {
                MessageBox.Show(
                    "Please select a session to edit.",
                    "No Session Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            CampaignSessionEditorWindow window =
                new CampaignSessionEditorWindow(
                    _campaign.Id,
                    selectedSession.Id);

            window.Owner = Window.GetWindow(this);

            if (window.ShowDialog() == true)
            {
                LoadSessions();
            }
        }

        private void DeleteSession_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewCampaign)
            {
                MessageBox.Show(
                    "Please save the campaign before deleting sessions.",
                    "Save Campaign First",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            CampaignSession selectedSession =
                SessionsListView.SelectedItem as CampaignSession;

            if (selectedSession == null)
            {
                MessageBox.Show(
                    "Please select a session to delete.",
                    "No Session Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete the session '" +
                selectedSession.Title + "'?",
                "Delete Session",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _sessionRepository.Delete(selectedSession.Id);
                LoadSessions();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not delete the session.\n\n" + ex.Message,
                    "Delete Session",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

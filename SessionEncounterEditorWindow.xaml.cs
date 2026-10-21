
using DnDesktop.Data;
using DnDesktop.Models;
using DnDesktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DnDesktop
{
    public partial class SessionEncounterEditorWindow : Window
    {
        private readonly EncounterCombatService _combatService =
            new EncounterCombatService();

        private readonly SessionEncounterRepository _repository =
            new SessionEncounterRepository();


        private readonly Guid _sessionId;
        private readonly SessionEncounter _encounter;
        private readonly bool _isNewEncounter;

        private Guid _campaignId;

        private List<EncounterCombatant> _availableParticipants =
            new List<EncounterCombatant>();

        private readonly List<EncounterCombatant> _combatants =
            new List<EncounterCombatant>();

        // Create a new encounter.
        public SessionEncounterEditorWindow(Guid sessionId)
        {
            InitializeComponent();

            _sessionId = sessionId;

            _encounter = new SessionEncounter
            {
                SessionId = sessionId
            };

            _isNewEncounter = true;
        }

        // Edit an existing encounter.
        public SessionEncounterEditorWindow(
            Guid sessionId,
            Guid encounterId)
        {
            InitializeComponent();

            _sessionId = sessionId;

            _encounter = _repository.GetById(encounterId);
            _isNewEncounter = false;

            if (_encounter == null ||
                _encounter.SessionId != sessionId)
            {
                MessageBox.Show(
                    "The selected encounter could not be found for this session.",
                    "Encounter Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Loaded += (sender, args) => Close();
                return;
            }

            NameTextBox.Text = _encounter.Name ?? string.Empty;
            EncounterNotesTextBox.Text =
                _encounter.EncounterNotes ?? string.Empty;

            // Load the existing combatants and available participants.
            LoadCombatants();
        }        


        // Save encounter details and creature assignments.
        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(
                    "Please enter an encounter name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            try
            {
                _encounter.SessionId = _sessionId;
                _encounter.Name = name;
                _encounter.EncounterNotes =
                    EncounterNotesTextBox.Text ?? string.Empty;

                if (_isNewEncounter)
                {
                    _repository.Add(_encounter);
                }
                else
                {
                    _repository.Update(_encounter);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the encounter.\n\n" + ex.Message,
                    "Save Encounter",
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

        // Load combatants already assigned to the encounter.
        // Also load campaign participants who have not yet been added.
        private void LoadCombatants()
        {
            try
            {
                _campaignId =
                    _repository.GetCampaignIdForSession(_sessionId);

                _combatants.Clear();

                if (!_isNewEncounter)
                {
                    _combatants.AddRange(
                        _combatService.GetCombatants(_encounter.Id));
                }

                RefreshCombatants();
                LoadAvailableCombatParticipants();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load combatants.\n\n" + ex.Message,
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Populate the dropdown with actual EncounterCombatant objects.
        private void LoadAvailableCombatParticipants()
        {
            if (_isNewEncounter)
            {
                CombatParticipantComboBox.ItemsSource = null;
                CombatParticipantComboBox.IsEnabled = false;
                return;
            }

            _availableParticipants =
                _combatService.GetAvailableParticipants(
                    _campaignId,
                    _encounter.Id);

            CombatParticipantComboBox.ItemsSource = null;
            CombatParticipantComboBox.DisplayMemberPath =
                "DisplayName";

            CombatParticipantComboBox.ItemsSource =
                _availableParticipants;

            CombatParticipantComboBox.IsEnabled =
                _availableParticipants.Count > 0;

            CombatParticipantComboBox.SelectedIndex =
                _availableParticipants.Count > 0 ? 0 : -1;
        }

        // Refresh the combatant list, sorted by initiative.
        private void RefreshCombatants()
        {
            CombatantsListView.ItemsSource = null;

            CombatantsListView.ItemsSource =
                _combatants
                    .OrderByDescending(c => c.Initiative)
                    .ThenBy(c => c.DisplayName)
                    .ToList();
        }

        // Add the selected campaign participant to this encounter.

        private void AddCombatant_Click(
            object sender,
            RoutedEventArgs e)
        {
            EncounterCombatant selected =
                CombatParticipantComboBox.SelectedItem
                    as EncounterCombatant;

            if (selected == null)
            {
                MessageBox.Show(
                    "Select a participant to add.",
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            int quantity = 1;

            try
            {
                if (selected.ParticipantType != "Creature" &&
                    quantity != 1)
                {
                    quantity = 1;
                }

                for (int i = 0; i < quantity; i++)
                {
                    var combatant = new EncounterCombatant
                    {
                        EncounterId = _encounter.Id,
                        ParticipantType = selected.ParticipantType,
                        SourceId = selected.SourceId,
                        DisplayName = selected.DisplayName,
                        Initiative = selected.Initiative,
                        CurrentHP = selected.CurrentHP,
                        MaxHP = selected.MaxHP,
                        ArmorClass = selected.ArmorClass,
                        Conditions = selected.Conditions,
                        IsDefeated = selected.IsDefeated
                    };

                    _combatService.AddParticipant(combatant);
                }

                _combatants.Clear();

                _combatants.AddRange(
                    _combatService.GetCombatants(_encounter.Id));

                RefreshCombatants();

                // Characters and NPCs disappear once added.
                // Creature types remain available for additional instances.
                LoadAvailableCombatParticipants();

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not add the combatant.\n\n" + ex.Message,
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditCombatant_Click(
            object sender,
            RoutedEventArgs e)
        {
            EncounterCombatant selected =
                CombatantsListView.SelectedItem as EncounterCombatant;

            if (selected == null)
            {
                MessageBox.Show(
                    "Select a combatant to edit.",
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var editor = new CombatantEditorWindow(selected)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                // Reload from the database so the list shows saved values.
                _combatants.Clear();

                _combatants.AddRange(
                    _combatService.GetCombatants(_encounter.Id));

                RefreshCombatants();
            }
        }


        // Remove the selected combatant from this encounter.
        private void RemoveCombatant_Click(
            object sender,
            RoutedEventArgs e)
        {
            EncounterCombatant selected =
                CombatantsListView.SelectedItem
                    as EncounterCombatant;

            if (selected == null)
            {
                MessageBox.Show(
                    "Select a combatant to remove.",
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                _combatService.RemoveCombatant(selected.Id);

                _combatants.RemoveAll(
                    c => c.Id == selected.Id);

                RefreshCombatants();
                LoadAvailableCombatParticipants();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not remove the combatant.\n\n" + ex.Message,
                    "Combatants",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

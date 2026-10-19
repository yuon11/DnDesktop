
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DnDesktop
{
    public partial class SessionEncounterEditorWindow : Window
    {
        private readonly SessionEncounterRepository _repository =
            new SessionEncounterRepository();

        private readonly CreatureRepository _creatureRepository =
            new CreatureRepository();

        private readonly Guid _sessionId;
        private readonly SessionEncounter _encounter;
        private readonly bool _isNewEncounter;

        private readonly List<SessionEncounterCreature> _assignments =
            new List<SessionEncounterCreature>();

        private List<Creature> _availableCreatures =
            new List<Creature>();

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

            LoadCreatures();
            RefreshCreatureAssignments();
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

            if (_encounter == null || _encounter.SessionId != sessionId)
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

            LoadCreatures();

            _assignments.AddRange(
                _repository.GetCreaturesByEncounterId(_encounter.Id));

            RefreshCreatureAssignments();
        }

        private void LoadCreatures()
        {
            try
            {
                _availableCreatures = _creatureRepository.GetAll()
                    .OrderBy(c => c.Name)
                    .ToList();

                CreatureComboBox.ItemsSource = _availableCreatures;

                if (_availableCreatures.Count > 0)
                {
                    CreatureComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not load creatures.\n\n" + ex.Message,
                    "Creatures",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshCreatureAssignments()
        {
            foreach (SessionEncounterCreature assignment in _assignments)
            {
                Creature creature = _availableCreatures.FirstOrDefault(
                    c => c.Id == assignment.CreatureId);

                assignment.CreatureName =
                    creature == null
                        ? "(Creature no longer available)"
                        : creature.Name;
            }

            EncounterCreaturesListView.ItemsSource = null;
            EncounterCreaturesListView.ItemsSource =
                _assignments.OrderBy(a => a.CreatureName).ToList();
        }

        private void AddCreature_Click(object sender, RoutedEventArgs e)
        {
            Creature selectedCreature =
                CreatureComboBox.SelectedItem as Creature;

            if (selectedCreature == null)
            {
                MessageBox.Show(
                    "Please select a creature.",
                    "No Creature Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            int quantity;

            if (!int.TryParse(QuantityTextBox.Text, out quantity)
                || quantity <= 0)
            {
                MessageBox.Show(
                    "Enter a quantity greater than zero.",
                    "Invalid Quantity",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                QuantityTextBox.Focus();
                return;
            }

            SessionEncounterCreature existing = _assignments.FirstOrDefault(
                a => a.CreatureId == selectedCreature.Id);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _assignments.Add(
                    new SessionEncounterCreature
                    {
                        EncounterId = _encounter.Id,
                        CreatureId = selectedCreature.Id,
                        CreatureName = selectedCreature.Name,
                        Quantity = quantity
                    });
            }

            QuantityTextBox.Text = "1";
            RefreshCreatureAssignments();
        }

        private void RemoveCreature_Click(object sender, RoutedEventArgs e)
        {
            SessionEncounterCreature selected =
                EncounterCreaturesListView.SelectedItem
                    as SessionEncounterCreature;

            if (selected == null)
            {
                MessageBox.Show(
                    "Please select a creature assignment to remove.",
                    "No Creature Selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SessionEncounterCreature existing = _assignments.FirstOrDefault(
                a => a.CreatureId == selected.CreatureId);

            if (existing != null)
            {
                _assignments.Remove(existing);
            }

            RefreshCreatureAssignments();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
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

                _repository.SaveCreatures(
                    _encounter.Id,
                    _assignments);

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

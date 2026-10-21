
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Windows;

namespace DnDesktop
{
    public partial class CombatantEditorWindow : Window
    {
        private readonly EncounterCombatantRepository _repository =
            new EncounterCombatantRepository();

        private readonly EncounterCombatant _combatant;

        public CombatantEditorWindow(EncounterCombatant combatant)
        {
            InitializeComponent();

            if (combatant == null)
            {
                throw new ArgumentNullException(nameof(combatant));
            }

            // Work on a copy so Cancel does not change the original.
            _combatant = new EncounterCombatant
            {
                Id = combatant.Id,
                EncounterId = combatant.EncounterId,
                ParticipantType = combatant.ParticipantType,
                SourceId = combatant.SourceId,
                DisplayName = combatant.DisplayName,
                Initiative = combatant.Initiative,
                CurrentHP = combatant.CurrentHP,
                MaxHP = combatant.MaxHP,
                ArmorClass = combatant.ArmorClass,
                Conditions = combatant.Conditions,
                IsDefeated = combatant.IsDefeated,
                TurnOrder = combatant.TurnOrder
            };

            LoadCombatant();
        }

        private void LoadCombatant()
        {
            CombatantNameTextBlock.Text = _combatant.DisplayName;
            CombatantTypeTextBlock.Text = _combatant.ParticipantType;

            InitiativeTextBox.Text =
                _combatant.Initiative.ToString();

            CurrentHPTextBox.Text =
                _combatant.CurrentHP.ToString();

            MaxHPTextBox.Text =
                _combatant.MaxHP.ToString();

            ArmorClassTextBox.Text =
                _combatant.ArmorClass.ToString();

            ConditionsTextBox.Text =
                _combatant.Conditions ?? string.Empty;

            IsDefeatedCheckBox.IsChecked =
                _combatant.IsDefeated;
        }

        private void Save_Click(
            object sender,
            RoutedEventArgs e)
        {
            int initiative;
            int currentHP;
            int maxHP;
            int armorClass;

            if (!int.TryParse(
                    InitiativeTextBox.Text, out initiative))
            {
                ShowValidationError("Initiative must be a whole number.");
                InitiativeTextBox.Focus();
                return;
            }

            if (!int.TryParse(
                    CurrentHPTextBox.Text, out currentHP) ||
                currentHP < 0)
            {
                ShowValidationError(
                    "Current HP must be a non-negative whole number.");
                CurrentHPTextBox.Focus();
                return;
            }

            if (!int.TryParse(
                    MaxHPTextBox.Text, out maxHP) ||
                maxHP < 0)
            {
                ShowValidationError(
                    "Maximum HP must be a non-negative whole number.");
                MaxHPTextBox.Focus();
                return;
            }

            if (currentHP > maxHP)
            {
                ShowValidationError(
                    "Current HP cannot exceed maximum HP.");
                CurrentHPTextBox.Focus();
                return;
            }

            if (!int.TryParse(
                    ArmorClassTextBox.Text, out armorClass))
            {
                ShowValidationError(
                    "Armor Class must be a whole number.");
                ArmorClassTextBox.Focus();
                return;
            }

            _combatant.Initiative = initiative;
            _combatant.CurrentHP = currentHP;
            _combatant.MaxHP = maxHP;
            _combatant.ArmorClass = armorClass;
            _combatant.Conditions =
                ConditionsTextBox.Text ?? string.Empty;
            _combatant.IsDefeated =
                IsDefeatedCheckBox.IsChecked == true;

            try
            {
                _repository.Update(_combatant);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save combatant changes.\n\n" +
                    ex.Message,
                    "Edit Combatant",
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

        private void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "Invalid Combatant Details",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}

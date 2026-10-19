
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace DnDesktop
{
    public partial class CreatureEditorPage : Page
    {
        private readonly CreatureRepository _repository;
        private readonly Creature _creature;
        private readonly bool _isNewCreature;

        public CreatureEditorPage(Creature creature)
        {
            InitializeComponent();

            _repository = new CreatureRepository();

            _isNewCreature = creature == null;
            _creature = creature ?? new Creature();

            EditorTitle.Text = _isNewCreature
                ? "Create Creature"
                : "Edit Creature";

            LoadCreature();
        }

        private void LoadCreature()
        {
            NameTextBox.Text = _creature.Name ?? string.Empty;
            SubtypeTextBox.Text = _creature.Subtype ?? string.Empty;
            DescriptionTextBox.Text =
                _creature.Description ?? string.Empty;

            SetComboBoxValue(
                CreatureTypeComboBox,
                _creature.CreatureType,
                "Other");

            SetComboBoxValue(
                SizeComboBox,
                _creature.Size,
                "Medium");

            SetComboBoxValue(
                AlignmentComboBox,
                _creature.Alignment,
                "Unaligned");

            // Combat statistics
            ArmorClassTextBox.Text =
                _creature.ArmorClass.ToString();

            ArmorClassNotesTextBox.Text =
                _creature.ArmorClassNotes ?? string.Empty;

            HitPointsTextBox.Text =
                _creature.HitPoints.ToString();

            HitDiceTextBox.Text =
                _creature.HitDice ?? string.Empty;

            InitiativeModifierTextBox.Text =
                _creature.InitiativeModifier.ToString();

            ChallengeRatingTextBox.Text =
                _creature.ChallengeRating ?? string.Empty;

            ProficiencyBonusTextBox.Text =
                _creature.ProficiencyBonus.ToString();

            ExperiencePointsTextBox.Text =
                _creature.ExperiencePoints.ToString();

            // Ability scores
            StrengthTextBox.Text =
                _creature.Strength.ToString();

            DexterityTextBox.Text =
                _creature.Dexterity.ToString();

            ConstitutionTextBox.Text =
                _creature.Constitution.ToString();

            IntelligenceTextBox.Text =
                _creature.Intelligence.ToString();

            WisdomTextBox.Text =
                _creature.Wisdom.ToString();

            CharismaTextBox.Text =
                _creature.Charisma.ToString();
        }

        private void SetComboBoxValue(
            ComboBox comboBox,
            string value,
            string defaultValue)
        {
            string targetValue = string.IsNullOrWhiteSpace(value)
                ? defaultValue
                : value;

            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (string.Equals(
                    item.Content.ToString(),
                    targetValue,
                    StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
            comboBox.Text = targetValue;
        }

        private string GetComboBoxValue(
            ComboBox comboBox,
            string defaultValue)
        {
            ComboBoxItem selectedItem =
                comboBox.SelectedItem as ComboBoxItem;

            if (selectedItem != null)
                return selectedItem.Content.ToString();

            return string.IsNullOrWhiteSpace(comboBox.Text)
                ? defaultValue
                : comboBox.Text.Trim();
        }

        private bool TryGetInteger(
            TextBox textBox,
            string fieldName,
            out int value)
        {
            string input = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                value = 0;
                return true;
            }

            if (int.TryParse(input, out value))
                return true;

            MessageBox.Show(
                fieldName + " must be a whole number.",
                "Validation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            textBox.Focus();
            textBox.SelectAll();

            return false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a creature name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            int armorClass;
            int hitPoints;
            int initiativeModifier;
            int proficiencyBonus;
            int experiencePoints;

            // Ability score variables
            int strength;
            int dexterity;
            int constitution;
            int intelligence;
            int wisdom;
            int charisma;

            if (!TryGetInteger(
                ArmorClassTextBox,
                "Armor Class",
                out armorClass))
                return;

            if (!TryGetInteger(
                HitPointsTextBox,
                "Hit Points",
                out hitPoints))
                return;

            if (!TryGetInteger(
                InitiativeModifierTextBox,
                "Initiative Modifier",
                out initiativeModifier))
                return;

            if (!TryGetInteger(
                ProficiencyBonusTextBox,
                "Proficiency Bonus",
                out proficiencyBonus))
                return;

            if (!TryGetInteger(
                ExperiencePointsTextBox,
                "Experience Points",
                out experiencePoints))
                return;

            // Validate ability scores
            if (!TryGetAbilityScore(
                StrengthTextBox, "Strength", out strength))
                return;

            if (!TryGetAbilityScore(
                DexterityTextBox, "Dexterity", out dexterity))
                return;

            if (!TryGetAbilityScore(
                ConstitutionTextBox, "Constitution", out constitution))
                return;

            if (!TryGetAbilityScore(
                IntelligenceTextBox, "Intelligence", out intelligence))
                return;

            if (!TryGetAbilityScore(
                WisdomTextBox, "Wisdom", out wisdom))
                return;

            if (!TryGetAbilityScore(
                CharismaTextBox, "Charisma", out charisma))
                return;

            // Identity fields
            _creature.Name = NameTextBox.Text.Trim();

            _creature.CreatureType = GetComboBoxValue(
                CreatureTypeComboBox, "Other");

            _creature.Subtype = SubtypeTextBox.Text.Trim();

            _creature.Size = GetComboBoxValue(
                SizeComboBox, "Medium");

            _creature.Alignment = GetComboBoxValue(
                AlignmentComboBox, "Unaligned");

            _creature.Description =
                DescriptionTextBox.Text.Trim();

            // Combat statistics
            _creature.ArmorClass = armorClass;

            _creature.ArmorClassNotes =
                ArmorClassNotesTextBox.Text.Trim();

            _creature.HitPoints = hitPoints;

            _creature.HitDice =
                HitDiceTextBox.Text.Trim();

            _creature.InitiativeModifier = initiativeModifier;

            _creature.ChallengeRating =
                ChallengeRatingTextBox.Text.Trim();

            _creature.ProficiencyBonus = proficiencyBonus;

            _creature.ExperiencePoints = experiencePoints;

            // Save ability scores
            _creature.Strength = strength;
            _creature.Dexterity = dexterity;
            _creature.Constitution = constitution;
            _creature.Intelligence = intelligence;
            _creature.Wisdom = wisdom;
            _creature.Charisma = charisma;

            _creature.LastModified = DateTime.Now;

            try
            {
                if (_isNewCreature)
                {
                    _creature.CreatedDate = DateTime.Now;
                    _repository.Add(_creature);
                }
                else
                {
                    _repository.Update(_creature);
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the creature.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Ability scores must be between 1 and 30.
        private bool TryGetAbilityScore(
            TextBox textBox,
            string abilityName,
            out int score)
        {
            if (!TryGetInteger(textBox, abilityName, out score))
                return false;

            if (score < 1 || score > 30)
            {
                MessageBox.Show(
                    abilityName + " must be between 1 and 30.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                textBox.Focus();
                textBox.SelectAll();

                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

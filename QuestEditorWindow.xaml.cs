
using DnDesktop.Data;
using DnDesktop.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DnDesktop
{
    public partial class QuestEditorWindow : Window
    {
        private readonly QuestRepository _repository =
            new QuestRepository();

        private readonly Guid _campaignId;
        private readonly Quest _quest;
        private readonly bool _isNewQuest;

        private readonly List<Character> _characters;

        public QuestEditorWindow(
            Guid campaignId,
            List<Character> characters)
        {
            InitializeComponent();

            _campaignId = campaignId;
            _characters = characters ?? new List<Character>();

            _quest = new Quest
            {
                CampaignId = campaignId
            };

            _isNewQuest = true;

            WindowTitleText.Text = "Create Quest";

            LoadQuestGivers();
        }

        public QuestEditorWindow(
            Guid campaignId,
            Guid questId,
            List<Character> characters)
        {
            InitializeComponent();

            _campaignId = campaignId;
            _characters = characters ?? new List<Character>();

            Quest existingQuest = _repository.GetById(questId);

            if (existingQuest == null ||
                existingQuest.CampaignId != campaignId)
            {
                throw new InvalidOperationException(
                    "The selected quest could not be found in this campaign.");
            }

            _quest = existingQuest;
            _isNewQuest = false;

            WindowTitleText.Text = "Edit Quest";

            LoadQuestGivers();
            LoadQuestFields();
        }

        private void LoadQuestGivers()
        {
            var questGivers = new List<Character>
            {
                new Character { Name = "(No quest giver)" }
            };

            questGivers.AddRange(_characters);

            QuestGiverComboBox.ItemsSource = questGivers;
            QuestGiverComboBox.SelectedIndex = 0;
        }

        private void LoadQuestFields()
        {
            NameTextBox.Text = _quest.Name;
            DescriptionTextBox.Text = _quest.Description;
            NotesTextBox.Text = _quest.Notes;

            for (int i = 0; i < StatusComboBox.Items.Count; i++)
            {
                ComboBoxItem item =
                    StatusComboBox.Items[i] as ComboBoxItem;

                if (item != null &&
                    Convert.ToString(item.Content) == _quest.Status)
                {
                    StatusComboBox.SelectedIndex = i;
                    break;
                }
            }

            if (_quest.QuestGiverId.HasValue)
            {
                foreach (Character character in _characters)
                {
                    if (character.Id == _quest.QuestGiverId.Value)
                    {
                        QuestGiverComboBox.SelectedItem = character;
                        break;
                    }
                }
            }
        }

        private void SaveQuest_Click(
            object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show(
                    "Please enter a quest name.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                NameTextBox.Focus();
                return;
            }

            ComboBoxItem selectedStatus =
                StatusComboBox.SelectedItem as ComboBoxItem;

            if (selectedStatus == null)
            {
                MessageBox.Show(
                    "Please select a quest status.",
                    "Validation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Character questGiver =
                QuestGiverComboBox.SelectedItem as Character;

            _quest.Name = NameTextBox.Text.Trim();
            _quest.Description = DescriptionTextBox.Text;
            _quest.Notes = NotesTextBox.Text;
            _quest.Status = Convert.ToString(selectedStatus.Content);
            _quest.QuestGiverId =
                questGiver == null || questGiver.Id == Guid.Empty
                    ? (Guid?)null
                    : questGiver.Id;

            try
            {
                if (_isNewQuest)
                {
                    _repository.Add(_quest);
                }
                else
                {
                    _repository.Update(_quest);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save the quest.\n\n" + ex.Message,
                    "Quest Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(
            object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

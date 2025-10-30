using System.Windows;
using System.Windows.Input;
using DnDAI.Core.Models;
using DnDAI.Core.Enums;

namespace DnDAI.Desktop.Dialogs;

public partial class PlayerCharacterDialog : Window
{
    public PlayerCharacter? Character { get; private set; }

    public PlayerCharacterDialog()
    {
        InitializeComponent();
        PopulateComboBoxes();

        // Fix scroll wheel not working - bubble up PreviewMouseWheel events
        PreviewMouseWheel += (sender, e) =>
        {
            if (sender is Window window)
            {
                var scrollViewer = FindScrollViewer(window);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        };
    }

    private System.Windows.Controls.ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is System.Windows.Controls.ScrollViewer scrollViewer)
                return scrollViewer;

            var result = FindScrollViewer(child);
            if (result != null)
                return result;
        }
        return null;
    }

    private void PopulateComboBoxes()
    {
        // Populate Race
        RaceComboBox.ItemsSource = Enum.GetValues(typeof(CharacterRace));
        RaceComboBox.SelectedIndex = 0;

        // Populate Class
        ClassComboBox.ItemsSource = Enum.GetValues(typeof(CharacterClass));
        ClassComboBox.SelectedIndex = 0;

        // Populate Alignment
        AlignmentComboBox.ItemsSource = Enum.GetValues(typeof(Alignment));
        AlignmentComboBox.SelectedIndex = 0;
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Please enter a character name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(PlayerNameTextBox.Text))
        {
            MessageBox.Show("Please enter a player name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Parse numeric fields with validation
        if (!int.TryParse(LevelTextBox.Text, out int level) || level < 1 || level > 20)
        {
            MessageBox.Show("Please enter a valid level (1-20).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(MaxHPTextBox.Text, out int maxHP) || maxHP < 1)
        {
            MessageBox.Show("Please enter a valid Max HP.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(CurrentHPTextBox.Text, out int currentHP) || currentHP < 0)
        {
            MessageBox.Show("Please enter a valid Current HP.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Parse ability scores
        if (!TryParseAbilityScore(StrengthTextBox.Text, "Strength", out int str)) return;
        if (!TryParseAbilityScore(DexterityTextBox.Text, "Dexterity", out int dex)) return;
        if (!TryParseAbilityScore(ConstitutionTextBox.Text, "Constitution", out int con)) return;
        if (!TryParseAbilityScore(IntelligenceTextBox.Text, "Intelligence", out int intel)) return;
        if (!TryParseAbilityScore(WisdomTextBox.Text, "Wisdom", out int wis)) return;
        if (!TryParseAbilityScore(CharismaTextBox.Text, "Charisma", out int cha)) return;

        if (!int.TryParse(GoldTextBox.Text, out int gold) || gold < 0)
        {
            MessageBox.Show("Please enter a valid Gold amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Parse spell slots
        int[] spellSlots = new int[9];
        if (!TryParseSpellSlot(SpellSlots1TextBox.Text, 1, out spellSlots[0])) return;
        if (!TryParseSpellSlot(SpellSlots2TextBox.Text, 2, out spellSlots[1])) return;
        if (!TryParseSpellSlot(SpellSlots3TextBox.Text, 3, out spellSlots[2])) return;
        if (!TryParseSpellSlot(SpellSlots4TextBox.Text, 4, out spellSlots[3])) return;
        if (!TryParseSpellSlot(SpellSlots5TextBox.Text, 5, out spellSlots[4])) return;
        if (!TryParseSpellSlot(SpellSlots6TextBox.Text, 6, out spellSlots[5])) return;
        if (!TryParseSpellSlot(SpellSlots7TextBox.Text, 7, out spellSlots[6])) return;
        if (!TryParseSpellSlot(SpellSlots8TextBox.Text, 8, out spellSlots[7])) return;
        if (!TryParseSpellSlot(SpellSlots9TextBox.Text, 9, out spellSlots[8])) return;

        // Create character
        Character = new PlayerCharacter
        {
            Name = NameTextBox.Text.Trim(),
            PlayerName = PlayerNameTextBox.Text.Trim(),
            Level = level,
            Race = (CharacterRace)RaceComboBox.SelectedItem,
            Class = (CharacterClass)ClassComboBox.SelectedItem,
            Alignment = (Alignment)AlignmentComboBox.SelectedItem,
            MaxHitPoints = maxHP,
            CurrentHitPoints = currentHP,
            Strength = str,
            Dexterity = dex,
            Constitution = con,
            Intelligence = intel,
            Wisdom = wis,
            Charisma = cha,
            Equipment = EquipmentTextBox.Text.Trim(),
            Gold = gold,
            Background = BackgroundTextBox.Text.Trim(),
            Backstory = BackstoryTextBox.Text.Trim(),
            SpellSlots1Max = spellSlots[0],
            SpellSlots1Current = spellSlots[0],
            SpellSlots2Max = spellSlots[1],
            SpellSlots2Current = spellSlots[1],
            SpellSlots3Max = spellSlots[2],
            SpellSlots3Current = spellSlots[2],
            SpellSlots4Max = spellSlots[3],
            SpellSlots4Current = spellSlots[3],
            SpellSlots5Max = spellSlots[4],
            SpellSlots5Current = spellSlots[4],
            SpellSlots6Max = spellSlots[5],
            SpellSlots6Current = spellSlots[5],
            SpellSlots7Max = spellSlots[6],
            SpellSlots7Current = spellSlots[6],
            SpellSlots8Max = spellSlots[7],
            SpellSlots8Current = spellSlots[7],
            SpellSlots9Max = spellSlots[8],
            SpellSlots9Current = spellSlots[8]
        };

        DialogResult = true;
        Close();
    }

    private bool TryParseAbilityScore(string text, string abilityName, out int value)
    {
        if (!int.TryParse(text, out value) || value < 1 || value > 30)
        {
            MessageBox.Show($"Please enter a valid {abilityName} score (1-30).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private bool TryParseSpellSlot(string text, int level, out int value)
    {
        if (!int.TryParse(text, out value) || value < 0)
        {
            MessageBox.Show($"Please enter a valid spell slot count for level {level}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

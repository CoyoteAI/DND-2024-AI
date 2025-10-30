using System.Windows;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class LevelUpDialog : Window
{
    public int NewLevel { get; private set; }
    public int NewMaxHP { get; private set; }
    public int[] NewSpellSlots { get; private set; } = new int[9];

    private readonly PlayerCharacter _character;

    public LevelUpDialog(PlayerCharacter character)
    {
        InitializeComponent();
        _character = character;

        // Calculate new level
        NewLevel = _character.Level + 1;

        // Display current and new level
        CurrentLevelText.Text = _character.Level.ToString();
        NewLevelText.Text = NewLevel.ToString();

        // Display proficiency bonus
        int currentProf = CalculateProficiencyBonus(_character.Level);
        int newProf = CalculateProficiencyBonus(NewLevel);
        ProficiencyBonusText.Text = $"Proficiency Bonus: +{currentProf} → +{newProf}";

        // Display current max HP
        CurrentMaxHPText.Text = _character.MaxHitPoints.ToString();
        NewMaxHPTextBox.Text = _character.MaxHitPoints.ToString();

        // Populate spell slots with current values
        SpellSlots1TextBox.Text = _character.SpellSlots1Max.ToString();
        SpellSlots2TextBox.Text = _character.SpellSlots2Max.ToString();
        SpellSlots3TextBox.Text = _character.SpellSlots3Max.ToString();
        SpellSlots4TextBox.Text = _character.SpellSlots4Max.ToString();
        SpellSlots5TextBox.Text = _character.SpellSlots5Max.ToString();
        SpellSlots6TextBox.Text = _character.SpellSlots6Max.ToString();
        SpellSlots7TextBox.Text = _character.SpellSlots7Max.ToString();
        SpellSlots8TextBox.Text = _character.SpellSlots8Max.ToString();
        SpellSlots9TextBox.Text = _character.SpellSlots9Max.ToString();

        // Hide spell slots section if not a spellcaster
        bool isSpellcaster = IsSpellcaster(_character.Class);
        SpellSlotsGroupBox.Visibility = isSpellcaster ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool IsSpellcaster(Core.Enums.CharacterClass characterClass)
    {
        return characterClass switch
        {
            Core.Enums.CharacterClass.Wizard => true,
            Core.Enums.CharacterClass.Sorcerer => true,
            Core.Enums.CharacterClass.Warlock => true,
            Core.Enums.CharacterClass.Cleric => true,
            Core.Enums.CharacterClass.Druid => true,
            Core.Enums.CharacterClass.Bard => true,
            Core.Enums.CharacterClass.Paladin => true,
            Core.Enums.CharacterClass.Ranger => true,
            Core.Enums.CharacterClass.Artificer => true,
            _ => false
        };
    }

    private int CalculateProficiencyBonus(int level)
    {
        return 2 + (level - 1) / 4;
    }

    private void LevelUp_Click(object sender, RoutedEventArgs e)
    {
        // Validate max level
        if (NewLevel > 20)
        {
            MessageBox.Show("Maximum level is 20.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validate new max HP
        if (!int.TryParse(NewMaxHPTextBox.Text, out int newMaxHP) || newMaxHP < _character.MaxHitPoints)
        {
            MessageBox.Show("New Max HP must be a valid number and cannot be less than current Max HP.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        NewMaxHP = newMaxHP;

        // Parse spell slots
        if (!TryParseSpellSlot(SpellSlots1TextBox.Text, out NewSpellSlots[0])) return;
        if (!TryParseSpellSlot(SpellSlots2TextBox.Text, out NewSpellSlots[1])) return;
        if (!TryParseSpellSlot(SpellSlots3TextBox.Text, out NewSpellSlots[2])) return;
        if (!TryParseSpellSlot(SpellSlots4TextBox.Text, out NewSpellSlots[3])) return;
        if (!TryParseSpellSlot(SpellSlots5TextBox.Text, out NewSpellSlots[4])) return;
        if (!TryParseSpellSlot(SpellSlots6TextBox.Text, out NewSpellSlots[5])) return;
        if (!TryParseSpellSlot(SpellSlots7TextBox.Text, out NewSpellSlots[6])) return;
        if (!TryParseSpellSlot(SpellSlots8TextBox.Text, out NewSpellSlots[7])) return;
        if (!TryParseSpellSlot(SpellSlots9TextBox.Text, out NewSpellSlots[8])) return;

        DialogResult = true;
        Close();
    }

    private bool TryParseSpellSlot(string text, out int value)
    {
        if (!int.TryParse(text, out value) || value < 0)
        {
            MessageBox.Show("Spell slots must be valid non-negative numbers.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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

using System.Windows;
using System.Windows.Controls;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class SpellDialog : Window
{
    public Spell? Spell { get; private set; }

    public SpellDialog()
    {
        InitializeComponent();
    }

    private void MaterialCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        MaterialTextBox.Visibility = Visibility.Visible;
    }

    private void MaterialCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        MaterialTextBox.Visibility = Visibility.Collapsed;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a spell name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var description = DescriptionTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show("Please enter a spell description.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int level = int.Parse((LevelComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "0");
        string school = (SchoolComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Evocation";
        string castingTime = CastingTimeComboBox.Text;
        string range = RangeComboBox.Text;
        string duration = DurationComboBox.Text;

        string? damageDice = DamageDiceTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(damageDice)) damageDice = null;

        var damageType = (DamageTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        if (damageType == "(None)") damageType = null;

        var attackType = (AttackTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        if (attackType == "(None)") attackType = null;

        var saveType = (SaveTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        if (saveType == "(None)") saveType = null;

        string? materials = null;
        if (MaterialCheckBox.IsChecked == true)
        {
            materials = MaterialTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(materials)) materials = "(material components)";
        }

        Spell = new Spell
        {
            Name = name,
            Level = level,
            School = school,
            CastingTime = castingTime,
            Range = range,
            Duration = duration,
            HasVerbal = VerbalCheckBox.IsChecked == true,
            HasSomatic = SomaticCheckBox.IsChecked == true,
            HasMaterial = MaterialCheckBox.IsChecked == true,
            MaterialComponents = materials ?? string.Empty,
            RequiresConcentration = ConcentrationCheckBox.IsChecked == true,
            IsRitual = RitualCheckBox.IsChecked == true,
            Description = description,
            DamageDice = damageDice,
            DamageType = damageType,
            AttackType = attackType,
            SaveType = saveType,
            IsCustom = true,
            Source = "Homebrew"
        };

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

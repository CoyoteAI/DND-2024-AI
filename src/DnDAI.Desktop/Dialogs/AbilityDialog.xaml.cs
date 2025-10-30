using System.Windows;
using System.Windows.Controls;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class AbilityDialog : Window
{
    public WeaponAbility? Ability { get; private set; }

    public AbilityDialog()
    {
        InitializeComponent();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var name = AbilityNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter an ability name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var diceRoll = DiceRollTextBox.Text.Trim();
        var damageType = (DamageTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        if (damageType == "(None)") damageType = null;

        var usageLimit = (UsageLimitComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "at will";
        var actionType = (ActionTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "action";

        Ability = new WeaponAbility
        {
            Name = name,
            Description = DescriptionTextBox.Text.Trim(),
            DiceRoll = string.IsNullOrWhiteSpace(diceRoll) ? null : diceRoll,
            DamageType = damageType,
            UsageLimit = usageLimit,
            ActionType = actionType
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

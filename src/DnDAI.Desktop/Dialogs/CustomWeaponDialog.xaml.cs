using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class CustomWeaponDialog : Window
{
    private List<WeaponAbility> _specialAbilities = new();
    public CustomWeapon? Weapon { get; private set; }

    // Base weapon defaults: (damage, versatile damage, is finesse)
    private readonly Dictionary<string, (string damage, string? versatile, bool finesse)> _baseWeaponDefaults = new()
    {
        { "Longsword", ("1d8", "1d10", false) },
        { "Shortsword", ("1d6", null, true) },
        { "Greatsword", ("2d6", null, false) },
        { "Rapier", ("1d8", null, true) },
        { "Dagger", ("1d4", null, true) },
        { "Battleaxe", ("1d8", "1d10", false) },
        { "Greataxe", ("1d12", null, false) },
        { "Warhammer", ("1d8", "1d10", false) },
        { "Spear", ("1d6", "1d8", false) },
        { "Quarterstaff", ("1d6", "1d8", false) },
        { "Mace", ("1d6", null, false) },
        { "Shortbow", ("1d6", null, true) },
        { "Longbow", ("1d8", null, true) },
        { "Light Crossbow", ("1d8", null, true) },
        { "Heavy Crossbow", ("1d10", null, true) }
    };

    public CustomWeaponDialog()
    {
        InitializeComponent();
    }

    private void BaseWeaponComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BaseWeaponComboBox.SelectedItem is not ComboBoxItem selectedItem)
            return;

        string selectedWeapon = selectedItem.Content.ToString() ?? "";

        if (selectedWeapon == "(None - Custom)")
        {
            // Reset to defaults
            DamageDiceTextBox.Text = "1d8";
            VersatileCheckBox.IsChecked = false;
            FinesseCheckBox.IsChecked = false;
            return;
        }

        if (_baseWeaponDefaults.TryGetValue(selectedWeapon, out var defaults))
        {
            DamageDiceTextBox.Text = defaults.damage;
            FinesseCheckBox.IsChecked = defaults.finesse;

            if (defaults.versatile != null)
            {
                VersatileCheckBox.IsChecked = true;
                VersatileDamageTextBox.Text = defaults.versatile;
            }
            else
            {
                VersatileCheckBox.IsChecked = false;
            }
        }
    }

    private void VersatileCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        VersatilePanel.Visibility = Visibility.Visible;
    }

    private void VersatileCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        VersatilePanel.Visibility = Visibility.Collapsed;
    }

    private void AddAbility_Click(object sender, RoutedEventArgs e)
    {
        var abilityDialog = new AbilityDialog
        {
            Owner = this
        };

        if (abilityDialog.ShowDialog() == true && abilityDialog.Ability != null)
        {
            _specialAbilities.Add(abilityDialog.Ability);
            RefreshAbilitiesList();
        }
    }

    private void RefreshAbilitiesList()
    {
        AbilitiesPanel.Children.Clear();

        if (!_specialAbilities.Any())
        {
            AbilitiesPanel.Children.Add(new TextBlock
            {
                Text = "No special abilities added yet",
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                FontStyle = FontStyles.Italic
            });
            return;
        }

        foreach (var ability in _specialAbilities)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(236, 240, 241)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var panel = new StackPanel();

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var nameText = new TextBlock
            {
                Text = ability.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            };
            header.Children.Add(nameText);

            if (!string.IsNullOrWhiteSpace(ability.DiceRoll))
            {
                var diceText = new TextBlock
                {
                    Text = $" - {ability.DiceRoll}",
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    Margin = new Thickness(5, 0, 0, 0)
                };
                header.Children.Add(diceText);

                if (!string.IsNullOrWhiteSpace(ability.DamageType))
                {
                    var typeText = new TextBlock
                    {
                        Text = $" {ability.DamageType}",
                        FontStyle = FontStyles.Italic,
                        Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                        Margin = new Thickness(2, 0, 0, 0)
                    };
                    header.Children.Add(typeText);
                }
            }

            panel.Children.Add(header);

            if (!string.IsNullOrWhiteSpace(ability.Description))
            {
                var descText = new TextBlock
                {
                    Text = ability.Description,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                    Margin = new Thickness(0, 3, 0, 0)
                };
                panel.Children.Add(descText);
            }

            var metaText = new TextBlock
            {
                Text = $"{ability.UsageLimit} • {ability.ActionType}",
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(0, 3, 0, 0)
            };
            panel.Children.Add(metaText);

            // Remove button
            var removeBtn = new Button
            {
                Content = "Remove",
                Height = 22,
                Width = 60,
                Margin = new Thickness(0, 5, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Tag = ability
            };
            removeBtn.Click += RemoveAbility_Click;
            panel.Children.Add(removeBtn);

            border.Child = panel;
            AbilitiesPanel.Children.Add(border);
        }
    }

    private void RemoveAbility_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is WeaponAbility ability)
        {
            _specialAbilities.Remove(ability);
            RefreshAbilitiesList();
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter a weapon name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var damageDice = DamageDiceTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(damageDice))
        {
            MessageBox.Show("Please enter damage dice (e.g., 1d8).", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(MagicBonusTextBox.Text.Trim(), out int magicBonus))
        {
            MessageBox.Show("Magic bonus must be a number.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var baseWeapon = (BaseWeaponComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        if (baseWeapon == "(None - Custom)") baseWeapon = null;

        Weapon = new CustomWeapon
        {
            Name = name,
            BaseWeaponType = baseWeapon,
            MagicBonus = magicBonus,
            DamageDice = damageDice,
            IsVersatile = VersatileCheckBox.IsChecked == true,
            VersatileDamageDice = VersatileCheckBox.IsChecked == true ? VersatileDamageTextBox.Text.Trim() : null,
            IsFinesse = FinesseCheckBox.IsChecked == true,
            SpecialAbilities = _specialAbilities,
            Notes = NotesTextBox.Text.Trim()
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

using System.Windows;
using System.Windows.Controls;
using DnDAI.Core.Enums;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class CustomEquipmentDialog : Window
{
    public Equipment? Equipment { get; private set; }

    public CustomEquipmentDialog()
    {
        InitializeComponent();
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedType = (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        if (selectedType == "Weapon")
        {
            WeaponPropertiesPanel.Visibility = Visibility.Visible;
            ArmorPropertiesPanel.Visibility = Visibility.Collapsed;
        }
        else if (selectedType == "Armor")
        {
            WeaponPropertiesPanel.Visibility = Visibility.Collapsed;
            ArmorPropertiesPanel.Visibility = Visibility.Visible;
        }
        else
        {
            WeaponPropertiesPanel.Visibility = Visibility.Collapsed;
            ArmorPropertiesPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void VersatileCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        VersatileDamageTextBox.Visibility = Visibility.Visible;
    }

    private void VersatileCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        VersatileDamageTextBox.Visibility = Visibility.Collapsed;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Validate name
        var name = NameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter an item name.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Validate description
        var description = DescriptionTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            MessageBox.Show("Please enter a description.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Parse equipment type
        var typeTag = (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "AdventuringGear";
        var equipmentType = typeTag switch
        {
            "Weapon" => EquipmentType.Weapon,
            "Armor" => EquipmentType.Armor,
            "Tool" => EquipmentType.Tool,
            _ => EquipmentType.AdventuringGear
        };

        // Parse rarity
        var rarityTag = (RarityComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Uncommon";
        var rarity = rarityTag switch
        {
            "Common" => ItemRarity.Common,
            "Uncommon" => ItemRarity.Uncommon,
            "Rare" => ItemRarity.Rare,
            "VeryRare" => ItemRarity.VeryRare,
            "Legendary" => ItemRarity.Legendary,
            "Artifact" => ItemRarity.Artifact,
            _ => ItemRarity.Uncommon
        };

        // Parse cost and weight
        if (!decimal.TryParse(CostTextBox.Text, out decimal cost))
        {
            cost = 0;
        }

        if (!decimal.TryParse(WeightTextBox.Text, out decimal weight))
        {
            weight = 0;
        }

        // Parse max charges
        int? maxCharges = null;
        if (int.TryParse(MaxChargesTextBox.Text, out int charges))
        {
            maxCharges = charges;
        }

        var chargeRegen = ChargeRegenTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(chargeRegen) || chargeRegen.StartsWith("e.g.,"))
        {
            chargeRegen = null;
        }

        // Create equipment object
        Equipment = new Equipment
        {
            Name = name,
            Description = description,
            Type = equipmentType,
            IsStandard = false, // Custom item
            CostInGold = cost,
            Weight = weight,
            Rarity = rarity,
            RequiresAttunement = RequiresAttunementCheckBox.IsChecked == true,
            MaxCharges = maxCharges,
            ChargeRegeneration = chargeRegen
        };

        // Handle weapon properties
        if (equipmentType == EquipmentType.Weapon)
        {
            var damage = DamageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(damage))
            {
                MessageBox.Show("Please enter weapon damage (e.g., 1d8).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var damageType = (DamageTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Slashing";
            var weaponCategoryTag = (WeaponCategoryComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "SimpleMelee";

            var weaponCategory = weaponCategoryTag switch
            {
                "SimpleMelee" => WeaponCategory.SimpleMelee,
                "SimpleRanged" => WeaponCategory.SimpleRanged,
                "MartialMelee" => WeaponCategory.MartialMelee,
                "MartialRanged" => WeaponCategory.MartialRanged,
                _ => WeaponCategory.SimpleMelee
            };

            Equipment.Damage = damage;
            Equipment.DamageType = damageType;
            Equipment.WeaponCategory = weaponCategory;
            Equipment.IsFinesse = FinesseCheckBox.IsChecked == true;
            Equipment.IsVersatile = VersatileCheckBox.IsChecked == true;

            if (Equipment.IsVersatile)
            {
                Equipment.VersatileDamage = VersatileDamageTextBox.Text.Trim();
            }

            var range = RangeTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(range) && !range.StartsWith("e.g.,"))
            {
                Equipment.Range = range;
            }
        }

        // Handle armor properties
        if (equipmentType == EquipmentType.Armor)
        {
            if (!int.TryParse(ArmorClassTextBox.Text, out int ac))
            {
                MessageBox.Show("Please enter a valid armor class.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var armorCategoryTag = (ArmorCategoryComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Light";
            var armorCategory = armorCategoryTag switch
            {
                "Light" => ArmorCategory.Light,
                "Medium" => ArmorCategory.Medium,
                "Heavy" => ArmorCategory.Heavy,
                "Shield" => ArmorCategory.Shield,
                _ => ArmorCategory.Light
            };

            Equipment.ArmorClass = ac;
            Equipment.ArmorCategory = armorCategory;
            Equipment.StealthDisadvantage = StealthDisadvantageCheckBox.IsChecked == true;

            // Set DEX modifier rules based on armor category
            Equipment.AddDexModifier = armorCategory != ArmorCategory.Heavy;
            if (armorCategory == ArmorCategory.Medium)
            {
                Equipment.MaxDexModifier = 2;
            }
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

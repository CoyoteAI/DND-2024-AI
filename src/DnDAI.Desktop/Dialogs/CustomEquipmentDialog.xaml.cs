using System.Windows;
using System.Windows.Controls;
using DnDAI.Core.Enums;
using DnDAI.Core.Models;

namespace DnDAI.Desktop.Dialogs;

public partial class CustomEquipmentDialog : Window
{
    public Equipment? Equipment { get; private set; }
    private readonly bool _isEditMode;

    public CustomEquipmentDialog()
    {
        InitializeComponent();
        _isEditMode = false;
    }

    public CustomEquipmentDialog(Equipment equipment) : this()
    {
        _isEditMode = true;
        Equipment = equipment;
        LoadEquipmentData();

        // Update title and button
        this.Title = "Edit Equipment";
        // Find and update the save button text
        this.Loaded += (s, e) =>
        {
            // The button is in the XAML, we'll need to update it
        };
    }

    private void LoadEquipmentData()
    {
        if (Equipment == null) return;

        NameTextBox.Text = Equipment.Name;
        DescriptionTextBox.Text = Equipment.Description;
        CostTextBox.Text = Equipment.CostInGold.ToString();
        WeightTextBox.Text = Equipment.Weight.ToString();

        // Set equipment type
        foreach (ComboBoxItem item in TypeComboBox.Items)
        {
            if (item.Tag?.ToString() == Equipment.Type.ToString())
            {
                TypeComboBox.SelectedItem = item;
                break;
            }
        }

        // Set rarity
        if (Equipment.Rarity.HasValue)
        {
            foreach (ComboBoxItem item in RarityComboBox.Items)
            {
                if (item.Tag?.ToString() == Equipment.Rarity.ToString())
                {
                    RarityComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        RequiresAttunementCheckBox.IsChecked = Equipment.RequiresAttunement;

        if (Equipment.MaxCharges.HasValue)
        {
            MaxChargesTextBox.Text = Equipment.MaxCharges.ToString();
        }
        if (!string.IsNullOrEmpty(Equipment.ChargeRegeneration))
        {
            ChargeRegenTextBox.Text = Equipment.ChargeRegeneration;
        }

        // Load type-specific properties
        if (Equipment.Type == EquipmentType.Weapon && Equipment.Damage != null)
        {
            DamageTextBox.Text = Equipment.Damage;

            foreach (ComboBoxItem item in DamageTypeComboBox.Items)
            {
                if (item.Content?.ToString() == Equipment.DamageType)
                {
                    DamageTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            if (Equipment.WeaponCategory.HasValue)
            {
                foreach (ComboBoxItem item in WeaponCategoryComboBox.Items)
                {
                    var tag = item.Tag?.ToString();
                    if (tag == Equipment.WeaponCategory.ToString())
                    {
                        WeaponCategoryComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Equipment.Range))
            {
                RangeTextBox.Text = Equipment.Range;
            }

            FinesseCheckBox.IsChecked = Equipment.IsFinesse;
            VersatileCheckBox.IsChecked = Equipment.IsVersatile;

            if (Equipment.IsVersatile && !string.IsNullOrEmpty(Equipment.VersatileDamage))
            {
                VersatileDamageTextBox.Text = Equipment.VersatileDamage;
            }
        }
        else if (Equipment.Type == EquipmentType.Armor && Equipment.ArmorClass.HasValue)
        {
            ArmorClassTextBox.Text = Equipment.ArmorClass.ToString();

            if (Equipment.ArmorCategory.HasValue)
            {
                foreach (ComboBoxItem item in ArmorCategoryComboBox.Items)
                {
                    var tag = item.Tag?.ToString();
                    if ((tag == "Light" && Equipment.ArmorCategory == ArmorCategory.LightArmor) ||
                        (tag == "Medium" && Equipment.ArmorCategory == ArmorCategory.MediumArmor) ||
                        (tag == "Heavy" && Equipment.ArmorCategory == ArmorCategory.HeavyArmor) ||
                        (tag == "Shield" && Equipment.ArmorCategory == ArmorCategory.Shield))
                    {
                        ArmorCategoryComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            StealthDisadvantageCheckBox.IsChecked = Equipment.StealthDisadvantage;
        }
        else if (Equipment.Type == EquipmentType.Container)
        {
            if (Equipment.WeightCapacity.HasValue)
            {
                WeightCapacityTextBox.Text = Equipment.WeightCapacity.ToString();
            }
            if (Equipment.VolumeCapacity.HasValue)
            {
                VolumeCapacityTextBox.Text = Equipment.VolumeCapacity.ToString();
            }
        }
    }

    private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedType = (TypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

        WeaponPropertiesPanel.Visibility = selectedType == "Weapon" ? Visibility.Visible : Visibility.Collapsed;
        ArmorPropertiesPanel.Visibility = selectedType == "Armor" ? Visibility.Visible : Visibility.Collapsed;
        ContainerPropertiesPanel.Visibility = selectedType == "Container" ? Visibility.Visible : Visibility.Collapsed;
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
            "Container" => EquipmentType.Container,
            _ => EquipmentType.Adventuring
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

        // Create or update equipment object
        if (!_isEditMode || Equipment == null)
        {
            Equipment = new Equipment();
        }

        // Update equipment properties
        Equipment.Name = name;
        Equipment.Description = description;
        Equipment.Type = equipmentType;
        Equipment.IsStandard = _isEditMode ? Equipment.IsStandard : false; // Preserve IsStandard in edit mode
        Equipment.CostInGold = cost;
        Equipment.Weight = weight;
        Equipment.Rarity = rarity;
        Equipment.RequiresAttunement = RequiresAttunementCheckBox.IsChecked == true;
        Equipment.MaxCharges = maxCharges;
        Equipment.ChargeRegeneration = chargeRegen;

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
                "Light" => ArmorCategory.LightArmor,
                "Medium" => ArmorCategory.MediumArmor,
                "Heavy" => ArmorCategory.HeavyArmor,
                "Shield" => ArmorCategory.Shield,
                _ => ArmorCategory.LightArmor
            };

            Equipment.ArmorClass = ac;
            Equipment.ArmorCategory = armorCategory;
            Equipment.StealthDisadvantage = StealthDisadvantageCheckBox.IsChecked == true;

            // Set DEX modifier rules based on armor category
            Equipment.AddDexModifier = armorCategory != ArmorCategory.HeavyArmor;
            if (armorCategory == ArmorCategory.MediumArmor)
            {
                Equipment.MaxDexModifier = 2;
            }
        }

        // Handle container properties
        if (equipmentType == EquipmentType.Container)
        {
            if (decimal.TryParse(WeightCapacityTextBox.Text, out decimal weightCapacity))
            {
                Equipment.WeightCapacity = weightCapacity;
            }

            if (decimal.TryParse(VolumeCapacityTextBox.Text, out decimal volumeCapacity))
            {
                Equipment.VolumeCapacity = volumeCapacity;
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

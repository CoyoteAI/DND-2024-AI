using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Models;
using DnDAI.Services;

namespace DnDAI.Desktop.Windows;

public partial class EquipmentWindow : Window
{
    private readonly PlayerCharacter _character;
    private readonly GameService _gameService;
    private List<CharacterEquipment> _characterEquipment = new();
    private List<CustomWeapon> _customWeapons = new();

    public EquipmentWindow(PlayerCharacter character, GameService gameService)
    {
        InitializeComponent();
        _character = character;
        _gameService = gameService;

        LoadCharacterInfo();
        LoadEquipmentAsync();
    }

    private void LoadCharacterInfo()
    {
        CharacterNameText.Text = $"{_character.Name}'s Equipment";
        GoldText.Text = _character.Gold.ToString();
        // TODO: Calculate weight
        WeightText.Text = "Weight: 0 / 150 lbs"; // Will calculate properly later
    }

    private async void LoadEquipmentAsync()
    {
        try
        {
            _characterEquipment = await _gameService.GetCharacterEquipmentAsync(_character.Id);

            // Load equipment details
            var allEquipment = await _gameService.GetAllEquipmentAsync();
            foreach (var charEquip in _characterEquipment)
            {
                charEquip.Equipment = allEquipment.FirstOrDefault(e => e.Id == charEquip.EquipmentId)!;
            }

            // Load custom weapons (legacy system)
            _customWeapons = await _gameService.GetCustomWeaponsAsync(_character.Id);

            DisplayEquipment();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading equipment: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DisplayEquipment()
    {
        EquippedPanel.Children.Clear();
        InventoryPanel.Children.Clear();

        var equipped = _characterEquipment.Where(ce => ce.IsEquipped).ToList();
        var inventory = _characterEquipment.Where(ce => !ce.IsEquipped).ToList();

        // Display equipped items
        if (!equipped.Any())
        {
            EquippedPanel.Children.Add(new TextBlock
            {
                Text = "No items equipped",
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(5)
            });
        }
        else
        {
            foreach (var item in equipped)
            {
                EquippedPanel.Children.Add(CreateEquipmentPanel(item, true));
            }
        }

        // Display inventory items
        if (!inventory.Any() && !_customWeapons.Any())
        {
            InventoryPanel.Children.Add(new TextBlock
            {
                Text = "No items in inventory",
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(5)
            });
        }
        else
        {
            foreach (var item in inventory)
            {
                InventoryPanel.Children.Add(CreateEquipmentPanel(item, false));
            }

            // Display custom weapons (legacy system)
            foreach (var weapon in _customWeapons)
            {
                InventoryPanel.Children.Add(CreateCustomWeaponPanel(weapon));
            }
        }
    }

    private Border CreateEquipmentPanel(CharacterEquipment charEquip, bool isEquipped)
    {
        var equipment = charEquip.Equipment;
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 3, 0, 3)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Left side: Item info
        var leftPanel = new StackPanel();

        var namePanel = new StackPanel { Orientation = Orientation.Horizontal };
        var nameText = new TextBlock
        {
            Text = equipment.Name,
            FontWeight = FontWeights.Bold,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };
        namePanel.Children.Add(nameText);

        if (charEquip.Quantity > 1)
        {
            var quantityText = new TextBlock
            {
                Text = $" (x{charEquip.Quantity})",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };
            namePanel.Children.Add(quantityText);
        }

        leftPanel.Children.Add(namePanel);

        // Equipment details
        var detailsText = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 3, 0, 0)
        };

        if (equipment.Type == Core.Enums.EquipmentType.Weapon && equipment.Damage != null)
        {
            detailsText.Text = $"{equipment.Damage} {equipment.DamageType}";
            if (equipment.IsVersatile && equipment.VersatileDamage != null)
            {
                detailsText.Text += $" (Versatile: {equipment.VersatileDamage})";
            }
            if (equipment.IsFinesse)
            {
                detailsText.Text += " • Finesse";
            }
        }
        else
        {
            detailsText.Text = equipment.Description;
        }

        leftPanel.Children.Add(detailsText);

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Right side: Action buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

        var equipButton = new Button
        {
            Content = isEquipped ? "Unequip" : "Equip",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(isEquipped ? Color.FromRgb(231, 76, 60) : Color.FromRgb(46, 204, 113)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 5, 0)
        };
        equipButton.Click += (s, e) => ToggleEquip_Click(charEquip);
        buttonPanel.Children.Add(equipButton);

        var deleteButton = new Button
        {
            Content = "Remove",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        deleteButton.Click += (s, e) => RemoveEquipment_Click(charEquip);
        buttonPanel.Children.Add(deleteButton);

        Grid.SetColumn(buttonPanel, 1);
        grid.Children.Add(buttonPanel);

        border.Child = grid;
        return border;
    }

    private async void ToggleEquip_Click(CharacterEquipment charEquip)
    {
        try
        {
            charEquip.IsEquipped = !charEquip.IsEquipped;
            await _gameService.UpdateCharacterEquipmentAsync(charEquip);
            DisplayEquipment();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error toggling equipment: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void RemoveEquipment_Click(CharacterEquipment charEquip)
    {
        var result = MessageBox.Show($"Remove {charEquip.Equipment.Name} from inventory?",
            "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _gameService.RemoveEquipmentFromCharacterAsync(charEquip.Id);
                _characterEquipment.Remove(charEquip);
                DisplayEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing equipment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private Border CreateCustomWeaponPanel(CustomWeapon weapon)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 3, 0, 3)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Left side: Weapon info
        var leftPanel = new StackPanel();

        var nameText = new TextBlock
        {
            Text = $"{weapon.Name} (Custom Weapon)",
            FontWeight = FontWeights.Bold,
            FontSize = 13
        };
        leftPanel.Children.Add(nameText);

        // Weapon details
        var detailsText = new TextBlock
        {
            Text = $"{weapon.DamageDice} + {weapon.MagicBonus}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 3, 0, 0)
        };
        leftPanel.Children.Add(detailsText);

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Right side: Remove button
        var deleteButton = new Button
        {
            Content = "Remove",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.FromRgb(149, 165, 166)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        deleteButton.Click += (s, e) => RemoveCustomWeapon_Click(weapon);

        Grid.SetColumn(deleteButton, 1);
        grid.Children.Add(deleteButton);

        border.Child = grid;
        return border;
    }

    private async void RemoveCustomWeapon_Click(CustomWeapon weapon)
    {
        var result = MessageBox.Show($"Remove {weapon.Name} from inventory?",
            "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _gameService.DeleteCustomWeaponAsync(weapon.Id);
                _customWeapons.Remove(weapon);
                DisplayEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing weapon: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void AddStandardEquipment_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Create equipment picker dialog
        MessageBox.Show("Equipment picker dialog coming soon!", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void AddCustomEquipment_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Create custom equipment dialog
        MessageBox.Show("Custom equipment dialog coming soon!", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

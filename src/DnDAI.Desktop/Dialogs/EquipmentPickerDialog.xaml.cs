using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Enums;
using DnDAI.Core.Models;
using DnDAI.Services;

namespace DnDAI.Desktop.Dialogs;

public partial class EquipmentPickerDialog : Window
{
    private readonly GameService _gameService;
    private readonly PlayerCharacter _character;
    private List<Equipment> _allEquipment = new();
    private List<Equipment> _filteredEquipment = new();
    private Equipment? _selectedEquipment;

    public event EventHandler? EquipmentAdded; // Notify parent window to refresh

    public EquipmentPickerDialog(GameService gameService, PlayerCharacter character)
    {
        InitializeComponent();
        _gameService = gameService;
        _character = character;
        LoadEquipmentAsync();
    }

    private async void LoadEquipmentAsync()
    {
        try
        {
            _allEquipment = await _gameService.GetAllEquipmentAsync();
            _filteredEquipment = _allEquipment.Where(e => e.IsStandard).OrderBy(e => e.Name).ToList();
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
        EquipmentTreeView.Items.Clear();

        if (!_filteredEquipment.Any())
        {
            var emptyItem = new TreeViewItem
            {
                Header = new TextBlock
                {
                    Text = "No equipment found",
                    FontStyle = FontStyles.Italic,
                    Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                    Margin = new Thickness(5)
                }
            };
            EquipmentTreeView.Items.Add(emptyItem);
            return;
        }

        // Group equipment by type
        var grouped = _filteredEquipment
            .GroupBy(e => e.Type)
            .OrderBy(g => g.Key.ToString());

        foreach (var group in grouped)
        {
            // Create category header
            var categoryHeader = new TreeViewItem
            {
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                IsExpanded = true
            };

            // Set category name
            var categoryName = group.Key switch
            {
                EquipmentType.Weapon => "⚔️ Weapons",
                EquipmentType.Armor => "🛡️ Armor",
                EquipmentType.Container => "📦 Containers",
                EquipmentType.Adventuring => "🎒 Adventuring Gear",
                EquipmentType.Tool => "🔧 Tools",
                EquipmentType.Consumable => "🧪 Consumables",
                _ => group.Key.ToString()
            };

            categoryHeader.Header = new TextBlock
            {
                Text = $"{categoryName} ({group.Count()})",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Margin = new Thickness(0, 5, 0, 5)
            };

            // Add items to category
            foreach (var equipment in group.OrderBy(e => e.Name))
            {
                var itemNode = new TreeViewItem
                {
                    Header = CreateEquipmentPanel(equipment),
                    Tag = equipment
                };
                categoryHeader.Items.Add(itemNode);
            }

            EquipmentTreeView.Items.Add(categoryHeader);
        }
    }

    private Border CreateEquipmentPanel(Equipment equipment)
    {
        var isSelected = _selectedEquipment?.Id == equipment.Id;

        var border = new Border
        {
            Background = new SolidColorBrush(isSelected ? Color.FromRgb(52, 152, 219) : Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 3, 0, 3),
            Tag = equipment
        };

        border.MouseLeftButtonDown += (s, e) =>
        {
            SelectEquipment(equipment);
            e.Handled = true; // Prevent TreeView from handling the click
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Left side: Item info
        var leftPanel = new StackPanel
        {
            MaxWidth = 450 // Constrain width so text wraps
        };

        var nameText = new TextBlock
        {
            Text = equipment.Name,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Foreground = new SolidColorBrush(isSelected ? Colors.White : Color.FromRgb(44, 62, 80)),
            TextWrapping = TextWrapping.Wrap
        };
        leftPanel.Children.Add(nameText);

        // Equipment details
        var detailsText = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(236, 240, 241) : Color.FromRgb(127, 140, 141)),
            Margin = new Thickness(0, 3, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        var details = new List<string>();

        if (equipment.Type == EquipmentType.Weapon && equipment.Damage != null)
        {
            details.Add($"{equipment.Damage} {equipment.DamageType}");
            if (equipment.IsVersatile && equipment.VersatileDamage != null)
            {
                details.Add($"Versatile: {equipment.VersatileDamage}");
            }
            if (equipment.IsFinesse)
            {
                details.Add("Finesse");
            }
            if (equipment.Range != null)
            {
                details.Add($"Range: {equipment.Range}");
            }
        }
        else if (equipment.Type == EquipmentType.Armor && equipment.ArmorClass.HasValue)
        {
            details.Add($"AC: {equipment.ArmorClass}");
            if (equipment.StealthDisadvantage)
            {
                details.Add("Stealth Disadvantage");
            }
        }
        else
        {
            details.Add(equipment.Description);
        }

        detailsText.Text = string.Join(" • ", details);
        leftPanel.Children.Add(detailsText);

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Right side: Cost and weight
        var rightPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right };

        var costText = new TextBlock
        {
            Text = $"{equipment.CostInGold} gp",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(isSelected ? Colors.White : Color.FromRgb(243, 156, 18)),
            TextAlignment = TextAlignment.Right
        };
        rightPanel.Children.Add(costText);

        var weightText = new TextBlock
        {
            Text = $"{equipment.Weight} lbs",
            FontSize = 10,
            Foreground = new SolidColorBrush(isSelected ? Color.FromRgb(236, 240, 241) : Color.FromRgb(127, 140, 141)),
            TextAlignment = TextAlignment.Right,
            Margin = new Thickness(0, 2, 0, 0)
        };
        rightPanel.Children.Add(weightText);

        Grid.SetColumn(rightPanel, 1);
        grid.Children.Add(rightPanel);

        border.Child = grid;
        return border;
    }

    private void SelectEquipment(Equipment equipment)
    {
        _selectedEquipment = equipment;

        // Update selected item info
        SelectedItemBorder.Visibility = Visibility.Visible;
        SelectedItemName.Text = equipment.Name;

        var details = new List<string>();
        details.Add($"Cost: {equipment.CostInGold} gp");
        details.Add($"Weight: {equipment.Weight} lbs");

        if (equipment.Type == EquipmentType.Weapon && equipment.Damage != null)
        {
            details.Add($"Damage: {equipment.Damage} {equipment.DamageType}");
        }
        else if (equipment.Type == EquipmentType.Armor && equipment.ArmorClass.HasValue)
        {
            details.Add($"AC: {equipment.ArmorClass}");
        }

        SelectedItemDetails.Text = string.Join(" • ", details);

        AddButton.IsEnabled = true;
        EditButton.IsEnabled = true;

        // Refresh display to show selection
        DisplayEquipment();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        FilterEquipment();
    }

    private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        FilterEquipment();
    }

    private void FilterEquipment()
    {
        if (_allEquipment == null || !_allEquipment.Any())
            return;

        var searchText = SearchTextBox?.Text?.ToLower() ?? "";
        var selectedType = (TypeFilterComboBox?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";

        _filteredEquipment = _allEquipment.Where(e => e.IsStandard).ToList();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            _filteredEquipment = _filteredEquipment
                .Where(e => e.Name.ToLower().Contains(searchText) ||
                           e.Description.ToLower().Contains(searchText))
                .ToList();
        }

        // Apply type filter
        if (selectedType != "All")
        {
            _filteredEquipment = selectedType switch
            {
                "Weapons" => _filteredEquipment.Where(e => e.Type == EquipmentType.Weapon).ToList(),
                "Armor" => _filteredEquipment.Where(e => e.Type == EquipmentType.Armor).ToList(),
                "Adventuring Gear" => _filteredEquipment.Where(e => e.Type == EquipmentType.Adventuring).ToList(),
                "Tools" => _filteredEquipment.Where(e => e.Type == EquipmentType.Tool).ToList(),
                _ => _filteredEquipment
            };
        }

        _filteredEquipment = _filteredEquipment.OrderBy(e => e.Name).ToList();
        DisplayEquipment();
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedEquipment == null)
        {
            MessageBox.Show("Please select an item.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Parse quantity
        if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity < 1)
        {
            MessageBox.Show("Please enter a valid quantity (minimum 1).", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _gameService.AddEquipmentToCharacterAsync(
                _character.Id,
                _selectedEquipment.Id,
                quantity,
                isEquipped: false
            );

            MessageBox.Show($"Added {quantity}x {_selectedEquipment.Name} to inventory.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Notify parent to refresh
            EquipmentAdded?.Invoke(this, EventArgs.Empty);

            // Clear selection and reset quantity for next item
            _selectedEquipment = null;
            SelectedItemBorder.Visibility = Visibility.Collapsed;
            QuantityTextBox.Text = "1";
            AddButton.IsEnabled = false;
            EditButton.IsEnabled = false;
            DisplayEquipment(); // Refresh to clear selection highlight
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding equipment: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AddToLibrary_Click(object sender, RoutedEventArgs e)
    {
        var createDialog = new CustomEquipmentDialog
        {
            Owner = this
        };

        if (createDialog.ShowDialog() == true && createDialog.Equipment != null)
        {
            try
            {
                await _gameService.CreateStandardEquipmentAsync(createDialog.Equipment);

                MessageBox.Show($"Added {createDialog.Equipment.Name} to the equipment library.\n\nIt will now appear in the standard equipment list.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload equipment list to show new item
                LoadEquipmentAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding to library: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedEquipment == null)
        {
            MessageBox.Show("Please select an item to edit.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var editDialog = new CustomEquipmentDialog(_selectedEquipment)
        {
            Owner = this
        };

        if (editDialog.ShowDialog() == true && editDialog.Equipment != null)
        {
            try
            {
                await _gameService.UpdateEquipmentAsync(editDialog.Equipment);

                MessageBox.Show($"Updated {editDialog.Equipment.Name} successfully.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload equipment list to show changes
                LoadEquipmentAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating equipment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Models;
using DnDAI.Services;
using DnDAI.Desktop.Dialogs;

namespace DnDAI.Desktop.Windows;

public partial class EquipmentWindow : Window
{
    private readonly PlayerCharacter _character;
    private readonly GameService _gameService;
    private List<CharacterEquipment> _characterEquipment = new();
    private List<CustomWeapon> _customWeapons = new();
    private Dictionary<int, bool> _containerExpansionState = new(); // Track which containers are expanded

    public EquipmentWindow(PlayerCharacter character, GameService gameService)
    {
        InitializeComponent();
        _character = character;
        _gameService = gameService;

        LoadCharacterInfo();
        _ = LoadEquipmentAsync(); // Fire and forget - constructors can't be async
    }

    private void LoadCharacterInfo()
    {
        CharacterNameText.Text = $"{_character.Name}'s Equipment";
        GoldText.Text = _character.Gold.ToString();
        // TODO: Calculate weight
        WeightText.Text = "Weight: 0 / 150 lbs"; // Will calculate properly later
    }

    private async Task LoadEquipmentAsync()
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

        // Display inventory items with container hierarchy
        var rootItems = inventory.Where(i => i.ContainerId == null).ToList();

        if (!rootItems.Any() && !_customWeapons.Any())
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
            // Display root-level items (not in containers)
            foreach (var item in rootItems)
            {
                InventoryPanel.Children.Add(CreateEquipmentPanel(item, false, 0));

                // If this is a container and it's expanded, display its contents
                if (item.Equipment.Type == Core.Enums.EquipmentType.Container)
                {
                    bool isExpanded = !_containerExpansionState.ContainsKey(item.Id) || _containerExpansionState[item.Id];

                    if (isExpanded)
                    {
                        var containedItems = inventory.Where(i => i.ContainerId == item.Id).ToList();
                        foreach (var containedItem in containedItems)
                        {
                            InventoryPanel.Children.Add(CreateEquipmentPanel(containedItem, false, 1, item));
                        }
                    }
                }
            }

            // Display custom weapons (legacy system)
            foreach (var weapon in _customWeapons)
            {
                InventoryPanel.Children.Add(CreateCustomWeaponPanel(weapon));
            }
        }
    }

    private Border CreateEquipmentPanel(CharacterEquipment charEquip, bool isEquipped, int indentLevel = 0, CharacterEquipment? parentContainer = null)
    {
        var equipment = charEquip.Equipment;
        var isContainer = equipment.Type == Core.Enums.EquipmentType.Container;
        var border = new Border
        {
            Background = new SolidColorBrush(isContainer ? Color.FromRgb(230, 240, 250) : Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(indentLevel * 40, 2, 0, 2), // Indent nested items (increased from 30 to 40)
            Tag = charEquip, // Store for drag-and-drop
            AllowDrop = isContainer, // Only containers can accept drops
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Make all items draggable (except when equipped)
        if (!isEquipped)
        {
            border.MouseLeftButtonDown += Equipment_MouseLeftButtonDown;
        }

        // Allow dropping items into containers
        if (isContainer)
        {
            border.DragOver += Container_DragOver;
            border.Drop += Container_Drop;
        }

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Left side: Item info
        var leftPanel = new StackPanel { MaxWidth = 450 };

        var namePanel = new StackPanel { Orientation = Orientation.Horizontal };

        // Add expand/collapse indicator for containers
        if (isContainer && !isEquipped)
        {
            bool isExpanded = !_containerExpansionState.ContainsKey(charEquip.Id) || _containerExpansionState[charEquip.Id];
            var expandButton = new TextBlock
            {
                Text = isExpanded ? "▼ " : "▶ ",
                FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 5, 0)
            };
            expandButton.MouseLeftButtonDown += (s, e) =>
            {
                e.Handled = true; // Prevent drag from starting
                ToggleContainerExpansion(charEquip.Id);
            };
            namePanel.Children.Add(expandButton);
        }

        var nameText = new TextBlock
        {
            Text = equipment.Name,
            FontWeight = FontWeights.Bold,
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center
        };
        namePanel.Children.Add(nameText);

        if (charEquip.Quantity > 1)
        {
            var quantityText = new TextBlock
            {
                Text = $" (x{charEquip.Quantity})",
                FontSize = 10,
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
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 2, 0, 0),
            TextWrapping = TextWrapping.Wrap
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
        else if (isContainer)
        {
            var details = new List<string> { equipment.Description };
            if (equipment.WeightCapacity.HasValue)
            {
                // Calculate current weight in container
                var containedWeight = _characterEquipment
                    .Where(i => i.ContainerId == charEquip.Id)
                    .Sum(i => i.Equipment.Weight * i.Quantity);
                details.Add($"Weight: {containedWeight}/{equipment.WeightCapacity} lbs");
            }
            if (equipment.VolumeCapacity.HasValue)
            {
                details.Add($"Volume: {equipment.VolumeCapacity} cu ft");
            }
            detailsText.Text = string.Join(" • ", details);
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
            Width = 70,
            Height = 30,
            FontSize = 11,
            Background = new SolidColorBrush(isEquipped ? Color.FromRgb(231, 76, 60) : Color.FromRgb(46, 204, 113)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 5, 0)
        };
        equipButton.Click += (s, e) => ToggleEquip_Click(charEquip);
        buttonPanel.Children.Add(equipButton);

        // Add "Take Out" button if item is in a container
        if (parentContainer != null)
        {
            var takeOutButton = new Button
            {
                Content = "Take Out",
                Width = 70,
                Height = 30,
                FontSize = 11,
                Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 5, 0)
            };
            takeOutButton.Click += (s, e) => TakeOutOfContainer_Click(charEquip);
            buttonPanel.Children.Add(takeOutButton);
        }

        var editButton = new Button
        {
            Content = "Edit",
            Width = 60,
            Height = 30,
            FontSize = 11,
            Background = new SolidColorBrush(Color.FromRgb(230, 126, 34)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 5, 0)
        };
        editButton.Click += (s, e) => EditEquipment_Click(charEquip);
        buttonPanel.Children.Add(editButton);

        var deleteButton = new Button
        {
            Content = "Remove",
            Width = 70,
            Height = 30,
            FontSize = 11,
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

    private async void TakeOutOfContainer_Click(CharacterEquipment charEquip)
    {
        try
        {
            charEquip.ContainerId = null;
            await _gameService.UpdateCharacterEquipmentAsync(charEquip);
            DisplayEquipment();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error removing item from container: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void EditEquipment_Click(CharacterEquipment charEquip)
    {
        try
        {
            var dialog = new CustomEquipmentDialog(charEquip.Equipment)
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true && dialog.Equipment != null)
            {
                await _gameService.UpdateEquipmentAsync(dialog.Equipment);
                // Reload all equipment data from database to get fresh data
                await LoadEquipmentAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error editing equipment: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private Border CreateCustomWeaponPanel(CustomWeapon weapon)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 2, 0, 2)
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
            FontSize = 11
        };
        leftPanel.Children.Add(nameText);

        // Weapon details
        var detailsText = new TextBlock
        {
            Text = $"{weapon.DamageDice} + {weapon.MagicBonus}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 2, 0, 0)
        };
        leftPanel.Children.Add(detailsText);

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Right side: Remove button
        var deleteButton = new Button
        {
            Content = "Remove",
            Width = 70,
            Height = 30,
            FontSize = 11,
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

    private void AddStandardEquipment_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.EquipmentPickerDialog(_gameService, _character)
        {
            Owner = this
        };

        // Subscribe to refresh event
        dialog.EquipmentAdded += (s, args) => LoadEquipmentAsync();

        dialog.ShowDialog();
    }

    private async void AddCustomEquipment_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Dialogs.CustomEquipmentDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.Equipment != null)
        {
            try
            {
                // Create the custom equipment in the database first
                var createdEquipment = await _gameService.CreateCustomEquipmentAsync(dialog.Equipment);

                // Add it to the character's inventory
                await _gameService.AddEquipmentToCharacterAsync(
                    _character.Id,
                    createdEquipment.Id,
                    quantity: 1,
                    isEquipped: false
                );

                MessageBox.Show($"Created and added {createdEquipment.Name} to inventory.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadEquipmentAsync(); // Refresh the display
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating custom equipment: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Container expansion/collapse
    private void ToggleContainerExpansion(int containerId)
    {
        if (_containerExpansionState.ContainsKey(containerId))
        {
            _containerExpansionState[containerId] = !_containerExpansionState[containerId];
        }
        else
        {
            _containerExpansionState[containerId] = false; // Collapse
        }
        DisplayEquipment();
    }

    // Drag and Drop functionality
    private void Equipment_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var border = sender as Border;
        if (border?.Tag is CharacterEquipment charEquip)
        {
            DragDrop.DoDragDrop(border, charEquip, DragDropEffects.Move);
        }
    }

    private void Container_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;

        if (e.Data.GetDataPresent(typeof(CharacterEquipment)))
        {
            var draggedItem = e.Data.GetData(typeof(CharacterEquipment)) as CharacterEquipment;
            var targetBorder = sender as Border;
            var targetContainer = targetBorder?.Tag as CharacterEquipment;

            // Prevent dropping a container into itself or into one of its children
            if (draggedItem != null && targetContainer != null && draggedItem.Id != targetContainer.Id)
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        e.Handled = true;
    }

    private async void Container_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(CharacterEquipment)))
        {
            var draggedItem = e.Data.GetData(typeof(CharacterEquipment)) as CharacterEquipment;
            var targetBorder = sender as Border;
            var targetContainer = targetBorder?.Tag as CharacterEquipment;

            if (draggedItem != null && targetContainer != null && draggedItem.Id != targetContainer.Id)
            {
                try
                {
                    // Debug: Show what we're about to do
                    var debugMessage = $"DEBUG:\n" +
                        $"Dragged Item: {draggedItem.Equipment.Name} (ID: {draggedItem.Id}, Current ContainerId: {draggedItem.ContainerId})\n" +
                        $"Target Container: {targetContainer.Equipment.Name} (ID: {targetContainer.Id})\n" +
                        $"Setting draggedItem.ContainerId = {targetContainer.Id}";

                    MessageBox.Show(debugMessage, "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Move the item into the container
                    draggedItem.ContainerId = targetContainer.Id;
                    await _gameService.UpdateCharacterEquipmentAsync(draggedItem);

                    // Expand the container to show the newly added item
                    _containerExpansionState[targetContainer.Id] = true;

                    // Reload equipment data from database to ensure fresh state
                    await LoadEquipmentAsync();

                    MessageBox.Show($"Moved {draggedItem.Equipment.Name} into {targetContainer.Equipment.Name}",
                        "Item Moved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error moving item: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

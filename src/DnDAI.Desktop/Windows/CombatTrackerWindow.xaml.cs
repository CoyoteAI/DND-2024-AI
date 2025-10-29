using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Services;

namespace DnDAI.Desktop.Windows;

public partial class CombatTrackerWindow : Window
{
    private readonly ICombatService _combatService;
    private readonly GameService _gameService;
    private readonly int _campaignId;
    private readonly int? _sessionId;
    private CombatEncounter? _currentEncounter;
    private readonly DispatcherTimer _refreshTimer;

    public CombatTrackerWindow(
        ICombatService combatService,
        GameService gameService,
        int campaignId,
        int? sessionId = null)
    {
        InitializeComponent();
        _combatService = combatService;
        _gameService = gameService;
        _campaignId = campaignId;
        _sessionId = sessionId;

        // Set up auto-refresh timer (every 2 seconds)
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _refreshTimer.Tick += async (s, e) => await AutoRefreshAsync();
        _refreshTimer.Start();

        LoadActiveCombatAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        _refreshTimer.Stop();
        base.OnClosed(e);
    }

    private async void LoadActiveCombatAsync()
    {
        _currentEncounter = await _combatService.GetActiveCombatAsync(_campaignId);

        if (_currentEncounter == null)
        {
            // Create new encounter
            var result = MessageBox.Show(
                "No active combat found. Start a new encounter?",
                "Start Combat",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var encounterName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter encounter name:",
                    "New Combat Encounter",
                    "Combat Encounter");

                if (!string.IsNullOrEmpty(encounterName))
                {
                    _currentEncounter = await _combatService.StartCombatAsync(
                        _campaignId,
                        _sessionId,
                        encounterName);
                }
                else
                {
                    Close();
                    return;
                }
            }
            else
            {
                Close();
                return;
            }
        }

        RefreshCombatDisplay();
    }

    private void RefreshCombatDisplay()
    {
        if (_currentEncounter == null) return;

        EncounterNameText.Text = _currentEncounter.Name;
        RoundInfoText.Text = $"Round {_currentEncounter.CurrentRound}";

        CombatantsPanel.Children.Clear();

        var initiativeOrder = _currentEncounter.Combatants
            .Where(c => !c.IsDead)
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.InitiativeModifier)
            .ToList();

        for (int i = 0; i < initiativeOrder.Count; i++)
        {
            var combatant = initiativeOrder[i];
            bool isCurrentTurn = i == _currentEncounter.CurrentTurnIndex;
            AddCombatantToPanel(combatant, isCurrentTurn);
        }

        UpdateCurrentTurnDisplay();
    }

    private void AddCombatantToPanel(Combatant combatant, bool isCurrentTurn)
    {
        var border = new Border
        {
            Background = isCurrentTurn
                ? new SolidColorBrush(Color.FromRgb(255, 243, 205))
                : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            BorderBrush = isCurrentTurn
                ? new SolidColorBrush(Color.FromRgb(255, 193, 7))
                : new SolidColorBrush(Color.FromRgb(222, 226, 230)),
            BorderThickness = new Thickness(isCurrentTurn ? 3 : 1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 0, 0, 10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Initiative
        var initiativeText = new TextBlock
        {
            Text = combatant.Initiative.ToString(),
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 15, 0)
        };
        Grid.SetColumn(initiativeText, 0);
        grid.Children.Add(initiativeText);

        // Name and HP
        var infoPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center
        };

        var nameText = new TextBlock
        {
            Text = combatant.Name,
            FontSize = 16,
            FontWeight = FontWeights.Bold
        };
        infoPanel.Children.Add(nameText);

        var hpText = new TextBlock
        {
            Text = $"HP: {combatant.CurrentHP}/{combatant.CurrentMaxHP}" +
                   (combatant.CurrentMaxHP < combatant.OriginalMaxHP ? $" (Max: {combatant.OriginalMaxHP})" : ""),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
        };
        infoPanel.Children.Add(hpText);

        if (combatant.CurrentMaxHP < combatant.OriginalMaxHP)
        {
            var reducedMaxText = new TextBlock
            {
                Text = "⚠️ Max HP Reduced",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
                FontWeight = FontWeights.Bold
            };
            infoPanel.Children.Add(reducedMaxText);
        }

        var statusText = new TextBlock
        {
            Text = $"AC: {combatant.ArmorClass}",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125))
        };
        infoPanel.Children.Add(statusText);

        Grid.SetColumn(infoPanel, 1);
        grid.Children.Add(infoPanel);

        // HP Buttons
        var hpButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(10, 0, 10, 0)
        };

        var damageButton = new Button
        {
            Content = "Damage",
            Width = 70,
            Margin = new Thickness(0, 0, 5, 0),
            Tag = combatant.Id
        };
        damageButton.Click += ApplyDamage_Click;
        hpButtonsPanel.Children.Add(damageButton);

        var healButton = new Button
        {
            Content = "Heal",
            Width = 70,
            Margin = new Thickness(0, 0, 5, 0),
            Tag = combatant.Id
        };
        healButton.Click += ApplyHealing_Click;
        hpButtonsPanel.Children.Add(healButton);

        var reduceMaxButton = new Button
        {
            Content = "⚠️ Max",
            Width = 60,
            Margin = new Thickness(0, 0, 5, 0),
            Tag = combatant.Id,
            ToolTip = "Reduce Max HP (Undead attacks)"
        };
        reduceMaxButton.Click += ReduceMaxHP_Click;
        hpButtonsPanel.Children.Add(reduceMaxButton);

        if (combatant.CurrentMaxHP < combatant.OriginalMaxHP)
        {
            var restoreButton = new Button
            {
                Content = "Restore",
                Width = 70,
                Tag = combatant.Id,
                ToolTip = "Restore Max HP"
            };
            restoreButton.Click += RestoreMaxHP_Click;
            hpButtonsPanel.Children.Add(restoreButton);
        }

        Grid.SetColumn(hpButtonsPanel, 2);
        grid.Children.Add(hpButtonsPanel);

        // Remove Button
        var removeButton = new Button
        {
            Content = "Remove",
            Width = 80,
            Tag = combatant.Id,
            Background = new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            Foreground = Brushes.White
        };
        removeButton.Click += RemoveCombatant_Click;
        Grid.SetColumn(removeButton, 3);
        grid.Children.Add(removeButton);

        border.Child = grid;
        CombatantsPanel.Children.Add(border);
    }

    private void UpdateCurrentTurnDisplay()
    {
        if (_currentEncounter == null) return;

        var initiativeOrder = _currentEncounter.Combatants
            .Where(c => !c.IsDead)
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.InitiativeModifier)
            .ToList();

        if (initiativeOrder.Any() && _currentEncounter.CurrentTurnIndex < initiativeOrder.Count)
        {
            var currentCombatant = initiativeOrder[_currentEncounter.CurrentTurnIndex];
            CurrentTurnText.Text = $"Current Turn: {currentCombatant.Name}";
        }
        else
        {
            CurrentTurnText.Text = "Current Turn: None";
        }
    }

    private async void AddCombatant_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;

        var dialog = new AddCombatantDialog(_combatService, _gameService, _campaignId, _currentEncounter.Id);
        if (dialog.ShowDialog() == true)
        {
            await ReloadCombatAsync();
        }
    }

    private async void RollInitiative_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;

        await _combatService.RollInitiativeForAllAsync(_currentEncounter.Id);
        await ReloadCombatAsync();
    }

    private async void NextTurn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;

        await _combatService.NextTurnAsync(_currentEncounter.Id);
        await ReloadCombatAsync();
    }

    private async void NextRound_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;

        await _combatService.NextRoundAsync(_currentEncounter.Id);
        await ReloadCombatAsync();
    }

    private async void ApplyDamage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int combatantId) return;

        var input = Microsoft.VisualBasic.Interaction.InputBox("Enter damage amount:", "Apply Damage", "0");
        if (int.TryParse(input, out int damage) && damage > 0)
        {
            await _combatService.ApplyDamageAsync(combatantId, damage);
            await ReloadCombatAsync();
        }
    }

    private async void ApplyHealing_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int combatantId) return;

        var input = Microsoft.VisualBasic.Interaction.InputBox("Enter healing amount:", "Apply Healing", "0");
        if (int.TryParse(input, out int healing) && healing > 0)
        {
            await _combatService.ApplyHealingAsync(combatantId, healing);
            await ReloadCombatAsync();
        }
    }

    private async void ReduceMaxHP_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int combatantId) return;

        var input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter Max HP reduction (e.g., from undead attacks):",
            "Reduce Max HP",
            "0");

        if (int.TryParse(input, out int reduction) && reduction > 0)
        {
            await _combatService.ReduceMaxHPAsync(combatantId, reduction);
            await ReloadCombatAsync();
        }
    }

    private async void RestoreMaxHP_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int combatantId) return;

        var result = MessageBox.Show(
            "Restore Max HP to original value?",
            "Restore Max HP",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await _combatService.RestoreMaxHPAsync(combatantId);
            await ReloadCombatAsync();
        }
    }

    private async void RemoveCombatant_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int combatantId) return;

        var result = MessageBox.Show(
            "Remove this combatant from the encounter?",
            "Remove Combatant",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await _combatService.RemoveCombatantAsync(combatantId);
            await ReloadCombatAsync();
        }
    }

    private async void EndCombat_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;

        var result = MessageBox.Show(
            "End this combat encounter?",
            "End Combat",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await _combatService.EndCombatAsync(_currentEncounter.Id, "Combat ended by user");
            Close();
        }
    }

    private async Task ReloadCombatAsync()
    {
        _currentEncounter = await _combatService.GetCombatEncounterAsync(_currentEncounter!.Id);
        RefreshCombatDisplay();
    }

    private async Task AutoRefreshAsync()
    {
        if (_currentEncounter == null) return;

        try
        {
            var updatedEncounter = await _combatService.GetCombatEncounterAsync(_currentEncounter.Id);

            // Check if combat has ended
            if (updatedEncounter == null || updatedEncounter.EndTime.HasValue)
            {
                _refreshTimer.Stop();
                MessageBox.Show("Combat has ended.", "Combat Ended", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            // Only update if something changed to avoid unnecessary UI updates
            if (HasCombatChanged(_currentEncounter, updatedEncounter))
            {
                _currentEncounter = updatedEncounter;
                RefreshCombatDisplay();
            }
        }
        catch (Exception ex)
        {
            // Silently fail on auto-refresh errors to avoid interrupting gameplay
            System.Diagnostics.Debug.WriteLine($"Auto-refresh error: {ex.Message}");
        }
    }

    private bool HasCombatChanged(CombatEncounter old, CombatEncounter updated)
    {
        if (old.CurrentRound != updated.CurrentRound) return true;
        if (old.CurrentTurnIndex != updated.CurrentTurnIndex) return true;
        if (old.Combatants.Count != updated.Combatants.Count) return true;

        // Check for HP changes
        foreach (var combatant in updated.Combatants)
        {
            var oldCombatant = old.Combatants.FirstOrDefault(c => c.Id == combatant.Id);
            if (oldCombatant == null) return true;
            if (oldCombatant.CurrentHP != combatant.CurrentHP) return true;
            if (oldCombatant.CurrentMaxHP != combatant.CurrentMaxHP) return true;
            if (oldCombatant.IsDead != combatant.IsDead) return true;
            if (oldCombatant.Initiative != combatant.Initiative) return true;
        }

        return false;
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (_currentEncounter == null) return;
        await ReloadCombatAsync();
    }
}

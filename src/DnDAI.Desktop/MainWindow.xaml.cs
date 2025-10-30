using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Services;
using DnDAI.Desktop.Windows;

namespace DnDAI.Desktop;

public partial class MainWindow : Window
{
    private readonly GameService _gameService;
    private readonly ICombatService _combatService;
    private Campaign? _currentCampaign;
    private Session? _currentSession;
    private CombatTrackerWindow? _combatTrackerWindow;

    private enum RollMode { Normal, Advantage, Disadvantage }
    private RollMode _currentRollMode = RollMode.Normal;

    public MainWindow(GameService gameService, ICombatService combatService)
    {
        InitializeComponent();
        _gameService = gameService;
        _combatService = combatService;
    }

    private async void NewCampaign_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CampaignDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                _currentCampaign = await _gameService.CreateCampaignAsync(
                    dialog.CampaignName,
                    dialog.CampaignDescription,
                    dialog.CampaignSetting);

                CampaignNameText.Text = _currentCampaign.Name;
                EnableCampaignButtons();
                AddSystemMessage($"Campaign '{_currentCampaign.Name}' created successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating campaign: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void LoadCampaign_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var campaigns = await _gameService.GetAllCampaignsAsync();
            var campaignList = campaigns.ToList();

            if (!campaignList.Any())
            {
                MessageBox.Show("No campaigns found. Please create a new campaign.", "No Campaigns", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new CampaignSelectionDialog(campaignList);
            if (dialog.ShowDialog() == true && dialog.SelectedCampaign != null)
            {
                _currentCampaign = await _gameService.GetCampaignAsync(dialog.SelectedCampaign.Id);
                CampaignNameText.Text = _currentCampaign?.Name ?? "Unknown Campaign";
                EnableCampaignButtons();
                AddSystemMessage($"Campaign '{_currentCampaign?.Name}' loaded successfully!");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading campaign: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void StartSession_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        try
        {
            _currentSession = await _gameService.StartSessionAsync(_currentCampaign.Id);
            SessionInfoText.Text = $"Session #{_currentSession.SessionNumber}\nStarted: {_currentSession.StartTime:g}";

            StartSessionButton.IsEnabled = false;
            EndSessionButton.IsEnabled = true;
            InputTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            EnableDiceButtons();

            AddSystemMessage($"Session #{_currentSession.SessionNumber} started. The adventure begins!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error starting session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void EndSession_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession == null) return;

        try
        {
            var summary = "Session ended."; // Could prompt user for summary
            await _gameService.EndSessionAsync(_currentSession.Id, summary);

            SessionInfoText.Text = "No active session";
            StartSessionButton.IsEnabled = true;
            EndSessionButton.IsEnabled = false;
            InputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            DisableDiceButtons();

            AddSystemMessage($"Session #{_currentSession.SessionNumber} ended.");
            _currentSession = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error ending session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
        {
            await SendMessageAsync();
            e.Handled = true;
        }
    }

    private async Task SendMessageAsync()
    {
        if (_currentCampaign == null || _currentSession == null) return;

        var input = InputTextBox.Text.Trim();
        if (string.IsNullOrEmpty(input)) return;

        InputTextBox.Text = string.Empty;
        InputTextBox.IsEnabled = false;
        SendButton.IsEnabled = false;

        try
        {
            AddPlayerMessage(input);

            var response = await _gameService.ProcessPlayerInputAsync(
                _currentCampaign.Id,
                _currentSession.Id,
                input);

            AddDMMessage(response);

            // Check if combat was started by the AI and auto-open tracker if needed
            await CheckAndOpenCombatTrackerAsync();
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error: {ex.Message}");
        }
        finally
        {
            InputTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            InputTextBox.Focus();
        }
    }

    private async Task CheckAndOpenCombatTrackerAsync()
    {
        if (_currentCampaign == null) return;

        try
        {
            var activeCombat = await _combatService.GetActiveCombatAsync(_currentCampaign.Id);

            // If combat is active and tracker isn't open, open it
            if (activeCombat != null && (_combatTrackerWindow == null || !_combatTrackerWindow.IsVisible))
            {
                _combatTrackerWindow = new CombatTrackerWindow(
                    _combatService,
                    _gameService,
                    _currentCampaign.Id,
                    _currentSession?.Id)
                {
                    Owner = this
                };

                _combatTrackerWindow.Closed += (s, e) => _combatTrackerWindow = null;
                _combatTrackerWindow.Show();

                AddSystemMessage("Combat tracker opened automatically.");
            }
        }
        catch
        {
            // Silently fail if combat check fails
        }
    }

    private void AddPlayerMessage(string message)
    {
        AddMessage("Player", message, (SolidColorBrush)FindResource("PlayerMessageBrush"));
    }

    private void AddDMMessage(string message)
    {
        AddMessage("DM", message, (SolidColorBrush)FindResource("DMMessageBrush"));
    }

    private void AddSystemMessage(string message)
    {
        AddMessage("System", message, new SolidColorBrush(Color.FromRgb(255, 243, 205)));
    }

    private void AddMessage(string speaker, string message, SolidColorBrush background)
    {
        var border = new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 5, 0, 5),
            MaxWidth = 800,
            HorizontalAlignment = speaker == "Player" ? HorizontalAlignment.Right : HorizontalAlignment.Left
        };

        var panel = new StackPanel();

        var speakerText = new TextBlock
        {
            Text = speaker,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13
        };

        panel.Children.Add(speakerText);
        panel.Children.Add(messageText);
        border.Child = panel;

        MessagesPanel.Children.Add(border);
        MessagesScrollViewer.ScrollToBottom();
    }

    private void EnableCampaignButtons()
    {
        StartSessionButton.IsEnabled = true;
        AddPlayerCharacterButton.IsEnabled = true;
        StartCombatButton.IsEnabled = true;
        CharacterSheetButton.IsEnabled = true;
        AddNPCButton.IsEnabled = true;
        AddLocationButton.IsEnabled = true;
        AddQuestButton.IsEnabled = true;
        ViewCampaignButton.IsEnabled = true;
    }

    private void StartCombat_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        try
        {
            // If combat tracker is already open, just bring it to front
            if (_combatTrackerWindow != null && _combatTrackerWindow.IsVisible)
            {
                _combatTrackerWindow.Activate();
                return;
            }

            _combatTrackerWindow = new CombatTrackerWindow(
                _combatService,
                _gameService,
                _currentCampaign.Id,
                _currentSession?.Id)
            {
                Owner = this
            };

            _combatTrackerWindow.Closed += (s, e) => _combatTrackerWindow = null;
            _combatTrackerWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening combat tracker: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void CharacterSheet_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        try
        {
            // Get the first player character from the campaign (or prompt to select)
            var campaign = await _gameService.GetCampaignAsync(_currentCampaign.Id);
            var playerCharacter = campaign?.PlayerCharacters.FirstOrDefault();

            if (playerCharacter == null)
            {
                MessageBox.Show("No player character found in this campaign. Please create one first.",
                    "No Character", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var characterSheet = new CharacterSheetWindow(
                playerCharacter,
                _gameService,
                _currentSession?.Id,
                (purpose, result) =>
                {
                    // Callback to display roll in main window
                    AddMessage("Dice Roll", $"{purpose}: {result}",
                        new SolidColorBrush(Color.FromRgb(232, 245, 233)));
                })
            {
                Owner = this
            };

            characterSheet.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening character sheet: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void AddPlayerCharacter_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        var dialog = new Dialogs.PlayerCharacterDialog
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true && dialog.Character != null)
        {
            try
            {
                await _gameService.AddPlayerCharacterAsync(_currentCampaign.Id, dialog.Character);
                AddSystemMessage($"Player Character '{dialog.Character.Name}' (played by {dialog.Character.PlayerName}) added to the campaign.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding player character: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void AddNPC_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        var dialog = new NPCDialog();
        if (dialog.ShowDialog() == true)
        {
            // NPC creation logic would go here
            AddSystemMessage($"NPC '{dialog.NPCName}' added to the campaign.");
        }
    }

    private void AddLocation_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        var dialog = new LocationDialog();
        if (dialog.ShowDialog() == true)
        {
            // Location creation logic would go here
            AddSystemMessage($"Location '{dialog.LocationName}' added to the campaign.");
        }
    }

    private void AddQuest_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        var dialog = new QuestDialog();
        if (dialog.ShowDialog() == true)
        {
            // Quest creation logic would go here
            AddSystemMessage($"Quest '{dialog.QuestTitle}' added to the campaign.");
        }
    }

    private async void ViewCampaign_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCampaign == null) return;

        try
        {
            var campaign = await _gameService.GetCampaignAsync(_currentCampaign.Id);
            var dialog = new CampaignViewDialog(campaign);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error viewing campaign: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Dice Rolling Methods
    private void EnableDiceButtons()
    {
        RollD4Button.IsEnabled = true;
        RollD6Button.IsEnabled = true;
        RollD8Button.IsEnabled = true;
        RollD10Button.IsEnabled = true;
        RollD12Button.IsEnabled = true;
        RollD20Button.IsEnabled = true;
        RollD100Button.IsEnabled = true;
        RollAdvButton.IsEnabled = true;
        RollDisButton.IsEnabled = true;
        CustomRollTextBox.IsEnabled = true;
        RollCustomButton.IsEnabled = true;
    }

    private void DisableDiceButtons()
    {
        RollD4Button.IsEnabled = false;
        RollD6Button.IsEnabled = false;
        RollD8Button.IsEnabled = false;
        RollD10Button.IsEnabled = false;
        RollD12Button.IsEnabled = false;
        RollD20Button.IsEnabled = false;
        RollD100Button.IsEnabled = false;
        RollAdvButton.IsEnabled = false;
        RollDisButton.IsEnabled = false;
        CustomRollTextBox.IsEnabled = false;
        RollCustomButton.IsEnabled = false;

        // Reset roll mode and button colors
        _currentRollMode = RollMode.Normal;
        RollAdvButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        RollDisButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
    }

    private async void RollD4_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d4");
    private async void RollD6_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d6");
    private async void RollD8_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d8");
    private async void RollD10_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d10");
    private async void RollD12_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d12");
    private async void RollD20_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d20");
    private async void RollD100_Click(object sender, RoutedEventArgs e) => await RollDiceAsync("1d100");

    private void RollAdvantage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRollMode == RollMode.Advantage)
        {
            // Toggle off
            _currentRollMode = RollMode.Normal;
            RollAdvButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
        else
        {
            // Toggle on
            _currentRollMode = RollMode.Advantage;
            RollAdvButton.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            // Turn off disadvantage if it was on
            RollDisButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
    }

    private void RollDisadvantage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentRollMode == RollMode.Disadvantage)
        {
            // Toggle off
            _currentRollMode = RollMode.Normal;
            RollDisButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
        else
        {
            // Toggle on
            _currentRollMode = RollMode.Disadvantage;
            RollDisButton.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
            // Turn off advantage if it was on
            RollAdvButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
    }

    private async void RollCustom_Click(object sender, RoutedEventArgs e)
    {
        var expression = CustomRollTextBox.Text.Trim();
        if (string.IsNullOrEmpty(expression)) return;

        await RollDiceAsync(expression);
        CustomRollTextBox.Clear();
    }

    private async Task RollDiceAsync(string expression)
    {
        if (_currentSession == null) return;

        try
        {
            Core.Models.DiceRoll roll;

            if (_currentRollMode == RollMode.Advantage)
            {
                roll = _gameService.RollWithAdvantage(0, "Player", $"Advantage - {expression}");
                await _gameService.LogDiceRollAsync(_currentSession.Id, roll);
            }
            else if (_currentRollMode == RollMode.Disadvantage)
            {
                roll = _gameService.RollWithDisadvantage(0, "Player", $"Disadvantage - {expression}");
                await _gameService.LogDiceRollAsync(_currentSession.Id, roll);
            }
            else
            {
                roll = await _gameService.RollAndLogAsync(_currentSession.Id, expression);
            }

            AddDiceRollMessage(roll);

            // Reset roll mode after rolling
            if (_currentRollMode != RollMode.Normal)
            {
                _currentRollMode = RollMode.Normal;
                RollAdvButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
                RollDisButton.Background = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"Error rolling dice: {ex.Message}");
        }
    }

    private void AddDiceRollMessage(Core.Models.DiceRoll roll)
    {
        var rollText = $"{roll.Expression}\n";
        rollText += $"Rolls: [{string.Join(", ", roll.IndividualRolls)}]";
        if (roll.Modifier != 0)
        {
            rollText += $" {(roll.Modifier >= 0 ? "+" : "")}{roll.Modifier}";
        }
        rollText += $"\nTotal: {roll.Total}";

        if (!string.IsNullOrEmpty(roll.Purpose))
        {
            rollText = $"{roll.Purpose}\n{rollText}";
        }

        AddMessage("Dice Roll", rollText, new SolidColorBrush(Color.FromRgb(232, 245, 233)));
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DnDAI.Core.Models;
using DnDAI.Services;

namespace DnDAI.Desktop;

public partial class MainWindow : Window
{
    private readonly GameService _gameService;
    private Campaign? _currentCampaign;
    private Session? _currentSession;

    public MainWindow(GameService gameService)
    {
        InitializeComponent();
        _gameService = gameService;
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
        AddNPCButton.IsEnabled = true;
        AddLocationButton.IsEnabled = true;
        AddQuestButton.IsEnabled = true;
        ViewCampaignButton.IsEnabled = true;
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
}

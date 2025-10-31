using System.Windows;
using System.Windows.Input;
using DnDAI.Core.Models;
using DnDAI.Services;

namespace DnDAI.Desktop;

public partial class CampaignSelectionDialog : Window
{
    private readonly GameService _gameService;
    public Campaign? SelectedCampaign { get; private set; }

    public CampaignSelectionDialog(IEnumerable<Campaign> campaigns, GameService gameService)
    {
        InitializeComponent();
        _gameService = gameService;
        CampaignsListBox.ItemsSource = campaigns;
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        if (CampaignsListBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a campaign.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedCampaign = CampaignsListBox.SelectedItem as Campaign;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void CampaignsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (CampaignsListBox.SelectedItem != null)
        {
            SelectedCampaign = CampaignsListBox.SelectedItem as Campaign;
            DialogResult = true;
            Close();
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (CampaignsListBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a campaign to delete.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var campaign = CampaignsListBox.SelectedItem as Campaign;
        if (campaign == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the campaign '{campaign.Name}'?\n\nThis will permanently delete:\n- All sessions and conversation history\n- All NPCs, locations, and quests\n- All player characters\n- All combat encounters\n\nThis action cannot be undone!",
            "Confirm Campaign Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var deleted = await _gameService.DeleteCampaignAsync(campaign.Id);
                if (deleted)
                {
                    MessageBox.Show($"Campaign '{campaign.Name}' has been deleted successfully.", "Campaign Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh the campaign list
                    var campaigns = await _gameService.GetAllCampaignsAsync();
                    CampaignsListBox.ItemsSource = campaigns;
                }
                else
                {
                    MessageBox.Show("Failed to delete the campaign. It may have already been deleted.", "Deletion Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting campaign: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

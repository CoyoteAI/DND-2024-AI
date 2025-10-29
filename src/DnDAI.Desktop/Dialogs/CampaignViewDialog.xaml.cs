using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Models;

namespace DnDAI.Desktop;

public partial class CampaignViewDialog : Window
{
    public CampaignViewDialog(Campaign? campaign)
    {
        InitializeComponent();

        if (campaign == null)
        {
            AddSection("Error", "Campaign not found.");
            return;
        }

        AddSection("Campaign Name", campaign.Name);
        AddSection("Setting", campaign.Setting);
        AddSection("Description", campaign.Description);
        AddSection("Started", campaign.StartDate.ToString("g"));
        AddSection("Status", campaign.IsActive ? "Active" : "Inactive");

        if (campaign.CurrentLocation != null)
        {
            AddSection("Current Location", campaign.CurrentLocation.Name);
        }

        AddSection("Player Characters", $"{campaign.PlayerCharacters.Count} characters");
        AddSection("NPCs", $"{campaign.NPCs.Count} NPCs");
        AddSection("Locations", $"{campaign.Locations.Count} locations");
        AddSection("Events", $"{campaign.Events.Count} events");
        AddSection("Quests", $"{campaign.Quests.Count} quests");
        AddSection("Sessions", $"{campaign.Sessions.Count} sessions");
    }

    private void AddSection(string title, string content)
    {
        var titleBlock = new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Margin = new Thickness(0, 10, 0, 5)
        };

        var contentBlock = new TextBlock
        {
            Text = content,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };

        DetailsPanel.Children.Add(titleBlock);
        DetailsPanel.Children.Add(contentBlock);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

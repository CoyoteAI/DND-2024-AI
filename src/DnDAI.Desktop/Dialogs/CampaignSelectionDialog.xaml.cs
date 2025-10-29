using System.Windows;
using System.Windows.Input;
using DnDAI.Core.Models;

namespace DnDAI.Desktop;

public partial class CampaignSelectionDialog : Window
{
    public Campaign? SelectedCampaign { get; private set; }

    public CampaignSelectionDialog(IEnumerable<Campaign> campaigns)
    {
        InitializeComponent();
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
}

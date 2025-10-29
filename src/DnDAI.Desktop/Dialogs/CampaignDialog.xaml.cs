using System.Windows;

namespace DnDAI.Desktop;

public partial class CampaignDialog : Window
{
    public string CampaignName => NameTextBox.Text;
    public string CampaignSetting => SettingTextBox.Text;
    public string CampaignDescription => DescriptionTextBox.Text;

    public CampaignDialog()
    {
        InitializeComponent();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Please enter a campaign name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
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

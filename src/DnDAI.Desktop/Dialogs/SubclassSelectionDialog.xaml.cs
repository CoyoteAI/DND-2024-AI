using System.Windows;
using DnDAI.Core.Enums;
using DnDAI.Core.Helpers;

namespace DnDAI.Desktop.Dialogs;

public partial class SubclassSelectionDialog : Window
{
    public CharacterSubclass SelectedSubclass { get; private set; }

    public SubclassSelectionDialog(CharacterClass characterClass, CharacterSubclass currentSubclass)
    {
        InitializeComponent();

        // Display class info
        ClassInfoText.Text = $"Select a subclass for your {characterClass}";

        // Populate subclass options
        var subclasses = SubclassHelper.GetSubclassesForClass(characterClass);
        var subclassOptions = new List<CharacterSubclass> { CharacterSubclass.None };
        subclassOptions.AddRange(subclasses);

        SubclassComboBox.ItemsSource = subclassOptions;

        // Set current selection
        if (subclassOptions.Contains(currentSubclass))
        {
            SubclassComboBox.SelectedItem = currentSubclass;
        }
        else
        {
            SubclassComboBox.SelectedIndex = 0; // Default to None
        }

        SelectedSubclass = currentSubclass;
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        if (SubclassComboBox.SelectedItem is CharacterSubclass subclass)
        {
            SelectedSubclass = subclass;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Please select a subclass.", "Validation Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

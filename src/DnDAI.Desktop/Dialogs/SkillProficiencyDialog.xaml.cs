using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DnDAI.Desktop.Dialogs;

public partial class SkillProficiencyDialog : Window
{
    private readonly Dictionary<string, string> _skills = new()
    {
        { "Acrobatics", "DEX" },
        { "Animal Handling", "WIS" },
        { "Arcana", "INT" },
        { "Athletics", "STR" },
        { "Deception", "CHA" },
        { "History", "INT" },
        { "Insight", "WIS" },
        { "Intimidation", "CHA" },
        { "Investigation", "INT" },
        { "Medicine", "WIS" },
        { "Nature", "INT" },
        { "Perception", "WIS" },
        { "Performance", "CHA" },
        { "Persuasion", "CHA" },
        { "Religion", "INT" },
        { "Sleight of Hand", "DEX" },
        { "Stealth", "DEX" },
        { "Survival", "WIS" }
    };

    private readonly Dictionary<string, CheckBox> _proficiencyCheckBoxes = new();
    private readonly Dictionary<string, CheckBox> _expertiseCheckBoxes = new();

    public string SkillProficiencies { get; private set; } = string.Empty;
    public string SkillExpertise { get; private set; } = string.Empty;

    public SkillProficiencyDialog(string currentProficiencies, string currentExpertise)
    {
        InitializeComponent();

        var profSet = currentProficiencies?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();
        var expSet = currentExpertise?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();

        foreach (var skill in _skills.OrderBy(s => s.Key))
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Skill name
            var nameText = new TextBlock
            {
                Text = $"{skill.Key} ({skill.Value})",
                Width = 250,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 13
            };
            panel.Children.Add(nameText);

            // Proficiency checkbox
            var profCheckBox = new CheckBox
            {
                Content = "Proficient",
                Width = 90,
                IsChecked = profSet.Contains(skill.Key),
                Margin = new Thickness(10, 0, 0, 0)
            };
            profCheckBox.Checked += (s, e) => ExpertiseCheckBox_UpdateState(skill.Key);
            profCheckBox.Unchecked += (s, e) => ExpertiseCheckBox_UpdateState(skill.Key);
            _proficiencyCheckBoxes[skill.Key] = profCheckBox;
            panel.Children.Add(profCheckBox);

            // Expertise checkbox
            var expCheckBox = new CheckBox
            {
                Content = "Expertise",
                Width = 80,
                IsChecked = expSet.Contains(skill.Key),
                Margin = new Thickness(5, 0, 0, 0),
                IsEnabled = profSet.Contains(skill.Key)
            };
            _expertiseCheckBoxes[skill.Key] = expCheckBox;
            panel.Children.Add(expCheckBox);

            SkillsPanel.Children.Add(panel);
        }
    }

    private void ExpertiseCheckBox_UpdateState(string skillName)
    {
        // Expertise requires proficiency
        _expertiseCheckBoxes[skillName].IsEnabled = _proficiencyCheckBoxes[skillName].IsChecked == true;
        if (!_expertiseCheckBoxes[skillName].IsEnabled)
        {
            _expertiseCheckBoxes[skillName].IsChecked = false;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var proficiencies = _proficiencyCheckBoxes
            .Where(kvp => kvp.Value.IsChecked == true)
            .Select(kvp => kvp.Key);

        var expertise = _expertiseCheckBoxes
            .Where(kvp => kvp.Value.IsChecked == true)
            .Select(kvp => kvp.Key);

        SkillProficiencies = string.Join(", ", proficiencies);
        SkillExpertise = string.Join(", ", expertise);

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

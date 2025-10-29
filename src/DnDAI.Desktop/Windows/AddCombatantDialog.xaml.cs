using System.Windows;
using DnDAI.Core.Interfaces;
using DnDAI.Services;

namespace DnDAI.Desktop.Windows;

public partial class AddCombatantDialog : Window
{
    private readonly ICombatService _combatService;
    private readonly GameService _gameService;
    private readonly int _campaignId;
    private readonly int _encounterId;

    public AddCombatantDialog(
        ICombatService combatService,
        GameService gameService,
        int campaignId,
        int encounterId)
    {
        InitializeComponent();
        _combatService = combatService;
        _gameService = gameService;
        _campaignId = campaignId;
        _encounterId = encounterId;

        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        try
        {
            // Load PCs
            var campaign = await _gameService.GetCampaignAsync(_campaignId);
            if (campaign != null)
            {
                PCListBox.ItemsSource = campaign.PlayerCharacters.Where(pc => pc.IsAlive).ToList();
            }

            // Load NPCs
            var npcs = await _gameService.GetCampaignNPCsAsync(_campaignId);
            NPCListBox.ItemsSource = npcs.Where(npc => npc.IsAlive).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Check which tab is selected
            if (PCListBox.SelectedItem != null)
            {
                var pc = (DnDAI.Core.Models.PlayerCharacter)PCListBox.SelectedItem;
                await _combatService.AddPlayerCharacterToCombatAsync(_encounterId, pc.Id);
                DialogResult = true;
                Close();
            }
            else if (NPCListBox.SelectedItem != null)
            {
                var npc = (DnDAI.Core.Models.NPC)NPCListBox.SelectedItem;
                await _combatService.AddNPCToCombatAsync(_encounterId, npc.Id);
                DialogResult = true;
                Close();
            }
            else if (!string.IsNullOrWhiteSpace(MonsterNameTextBox.Text))
            {
                if (!int.TryParse(MonsterHPTextBox.Text, out int hp) || hp <= 0)
                {
                    MessageBox.Show("Please enter a valid HP value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(MonsterACTextBox.Text, out int ac) || ac < 0)
                {
                    MessageBox.Show("Please enter a valid AC value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int.TryParse(MonsterInitTextBox.Text, out int initMod);

                await _combatService.AddMonsterToCombatAsync(
                    _encounterId,
                    MonsterNameTextBox.Text,
                    hp,
                    ac,
                    initMod);

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select a combatant or enter monster details.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding combatant: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

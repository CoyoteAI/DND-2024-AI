using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DnDAI.Core.Models;
using DnDAI.Services;
using System.Text.RegularExpressions;

namespace DnDAI.Desktop.Windows;

public partial class CharacterSheetWindow : Window
{
    private readonly PlayerCharacter _character;
    private readonly GameService _gameService;
    private readonly int? _sessionId;
    private readonly Action<string, string> _onRollCallback;

    private int _proficiencyBonus;
    private Dictionary<string, int> _abilityModifiers = new();

    public CharacterSheetWindow(
        PlayerCharacter character,
        GameService gameService,
        int? sessionId = null,
        Action<string, string>? onRollCallback = null)
    {
        InitializeComponent();
        _character = character;
        _gameService = gameService;
        _sessionId = sessionId;
        _onRollCallback = onRollCallback ?? ((purpose, result) => { });

        LoadCharacterData();
    }

    private void LoadCharacterData()
    {
        // Header
        CharacterNameText.Text = _character.Name;
        CharacterInfoText.Text = $"Level {_character.Level} {_character.Race} {_character.Class}";
        HPText.Text = $"{_character.CurrentHitPoints}/{_character.MaxHitPoints}";

        // Calculate proficiency bonus
        _proficiencyBonus = CalculateProficiencyBonus(_character.Level);
        ProfText.Text = $"+{_proficiencyBonus}";

        // Calculate AC (basic calculation - 10 + DEX modifier)
        int dexMod = CalculateAbilityModifier(_character.Dexterity);
        int ac = 10 + dexMod; // Basic calculation, could be enhanced with armor
        ACText.Text = ac.ToString();

        // Load ability scores and modifiers
        LoadAbilityScores();

        // Load saving throws
        LoadSavingThrows();

        // Load skills
        LoadSkills();

        // Load weapons from equipment
        LoadWeapons();

        // Update initiative
        InitiativeText.Text = $"Roll Initiative: {FormatModifier(dexMod)}";
    }

    private void LoadAbilityScores()
    {
        // STR
        StrScore.Text = _character.Strength.ToString();
        int strMod = CalculateAbilityModifier(_character.Strength);
        StrMod.Text = FormatModifier(strMod);
        _abilityModifiers["STR"] = strMod;

        // DEX
        DexScore.Text = _character.Dexterity.ToString();
        int dexMod = CalculateAbilityModifier(_character.Dexterity);
        DexMod.Text = FormatModifier(dexMod);
        _abilityModifiers["DEX"] = dexMod;

        // CON
        ConScore.Text = _character.Constitution.ToString();
        int conMod = CalculateAbilityModifier(_character.Constitution);
        ConMod.Text = FormatModifier(conMod);
        _abilityModifiers["CON"] = conMod;

        // INT
        IntScore.Text = _character.Intelligence.ToString();
        int intMod = CalculateAbilityModifier(_character.Intelligence);
        IntMod.Text = FormatModifier(intMod);
        _abilityModifiers["INT"] = intMod;

        // WIS
        WisScore.Text = _character.Wisdom.ToString();
        int wisMod = CalculateAbilityModifier(_character.Wisdom);
        WisMod.Text = FormatModifier(wisMod);
        _abilityModifiers["WIS"] = wisMod;

        // CHA
        ChaScore.Text = _character.Charisma.ToString();
        int chaMod = CalculateAbilityModifier(_character.Charisma);
        ChaMod.Text = FormatModifier(chaMod);
        _abilityModifiers["CHA"] = chaMod;
    }

    private void LoadSavingThrows()
    {
        // For now, assume no proficiencies (can be enhanced later)
        // In D&D 2024, each class gets 2 saving throw proficiencies
        var saveButtons = SavingThrowsPanel.Children.OfType<Button>().ToList();

        foreach (var button in saveButtons)
        {
            string ability = button.Tag.ToString() ?? "";
            int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);
            // TODO: Add proficiency if character is proficient in this save
            button.Content = $"{ability} Save: {FormatModifier(modifier)}";
        }
    }

    private void LoadSkills()
    {
        // Update all skill buttons with their modifiers
        var skillButtons = FindVisualChildren<Button>(this)
            .Where(b => b.Tag?.ToString()?.Contains('|') == true)
            .ToList();

        foreach (var button in skillButtons)
        {
            string tag = button.Tag.ToString() ?? "";
            var parts = tag.Split('|');
            if (parts.Length == 2)
            {
                string skillName = parts[0];
                string ability = parts[1];
                int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);
                // TODO: Add proficiency bonus if character is proficient in this skill
                button.Content = $"{skillName} ({ability}): {FormatModifier(modifier)}";
            }
        }
    }

    private void LoadWeapons()
    {
        WeaponsPanel.Children.Clear();

        if (string.IsNullOrWhiteSpace(_character.Equipment))
        {
            WeaponsPanel.Children.Add(new TextBlock
            {
                Text = "No weapons equipped",
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(5)
            });
            return;
        }

        // Parse equipment for weapons
        var weapons = ParseWeaponsFromEquipment(_character.Equipment);

        if (!weapons.Any())
        {
            WeaponsPanel.Children.Add(new TextBlock
            {
                Text = "No weapons found in equipment",
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(5)
            });
            return;
        }

        foreach (var weapon in weapons)
        {
            var weaponPanel = new StackPanel
            {
                Margin = new Thickness(0, 5, 0, 5)
            };

            var weaponHeader = new TextBlock
            {
                Text = weapon.Name,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 0, 0, 3)
            };
            weaponPanel.Children.Add(weaponHeader);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // Attack button
            var attackButton = new Button
            {
                Content = $"Attack: {FormatModifier(weapon.AttackBonus)}",
                Width = 120,
                Height = 30,
                Margin = new Thickness(5, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = weapon
            };
            attackButton.Click += WeaponAttack_Click;
            buttonPanel.Children.Add(attackButton);

            // Damage button
            var damageButton = new Button
            {
                Content = $"Damage: {weapon.Damage}",
                Width = 120,
                Height = 30,
                Margin = new Thickness(5, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = weapon
            };
            damageButton.Click += WeaponDamage_Click;
            buttonPanel.Children.Add(damageButton);

            weaponPanel.Children.Add(buttonPanel);
            WeaponsPanel.Children.Add(weaponPanel);
        }
    }

    private List<Weapon> ParseWeaponsFromEquipment(string equipment)
    {
        var weapons = new List<Weapon>();

        // Common weapon patterns
        var weaponPatterns = new Dictionary<string, (string damage, bool finesse)>
        {
            { "longsword", ("1d8", false) },
            { "shortsword", ("1d6", true) },
            { "greatsword", ("2d6", false) },
            { "rapier", ("1d8", true) },
            { "dagger", ("1d4", true) },
            { "handaxe", ("1d6", false) },
            { "battleaxe", ("1d8", false) },
            { "greataxe", ("1d12", false) },
            { "mace", ("1d6", false) },
            { "warhammer", ("1d8", false) },
            { "maul", ("2d6", false) },
            { "spear", ("1d6", false) },
            { "quarterstaff", ("1d6", false) },
            { "club", ("1d4", false) },
            { "greatclub", ("1d8", false) },
            { "shortbow", ("1d6", true) },
            { "longbow", ("1d8", true) },
            { "crossbow", ("1d8", true) },
            { "light crossbow", ("1d8", true) },
            { "heavy crossbow", ("1d10", true) }
        };

        string lowerEquip = equipment.ToLower();

        foreach (var kvp in weaponPatterns)
        {
            if (lowerEquip.Contains(kvp.Key))
            {
                string weaponName = char.ToUpper(kvp.Key[0]) + kvp.Key.Substring(1);
                string damage = kvp.Value.damage;
                bool isFinesse = kvp.Value.finesse;

                // Calculate attack bonus
                int strMod = _abilityModifiers.GetValueOrDefault("STR", 0);
                int dexMod = _abilityModifiers.GetValueOrDefault("DEX", 0);
                int attackMod = isFinesse ? Math.Max(strMod, dexMod) : strMod;
                int attackBonus = attackMod + _proficiencyBonus;

                // Calculate damage modifier
                int damageMod = isFinesse ? Math.Max(strMod, dexMod) : strMod;
                string damageString = damageMod >= 0 ? $"{damage}+{damageMod}" : $"{damage}{damageMod}";

                weapons.Add(new Weapon
                {
                    Name = weaponName,
                    AttackBonus = attackBonus,
                    Damage = damageString,
                    DamageDice = damage,
                    DamageModifier = damageMod
                });
            }
        }

        return weapons;
    }

    private int CalculateAbilityModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }

    private int CalculateProficiencyBonus(int level)
    {
        return 2 + (level - 1) / 4; // Standard D&D progression
    }

    private string FormatModifier(int modifier)
    {
        return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
    }

    private async void AbilityCheck_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string ability)
            return;

        int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);
        await RollD20WithModifier($"{ability} Check", modifier);
    }

    private async void SavingThrow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string ability)
            return;

        int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);
        // TODO: Add proficiency if proficient in this save
        await RollD20WithModifier($"{ability} Saving Throw", modifier);
    }

    private async void SkillCheck_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag)
            return;

        var parts = tag.Split('|');
        if (parts.Length != 2) return;

        string skillName = parts[0];
        string ability = parts[1];
        int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);
        // TODO: Add proficiency if proficient in this skill

        await RollD20WithModifier($"{skillName} Check", modifier);
    }

    private async void Initiative_Click(object sender, RoutedEventArgs e)
    {
        int dexMod = _abilityModifiers.GetValueOrDefault("DEX", 0);
        await RollD20WithModifier("Initiative", dexMod);
    }

    private async void WeaponAttack_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Weapon weapon)
            return;

        await RollD20WithModifier($"{weapon.Name} Attack", weapon.AttackBonus);
    }

    private async void WeaponDamage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Weapon weapon)
            return;

        string expression = weapon.DamageDice;
        if (weapon.DamageModifier != 0)
        {
            expression = weapon.DamageModifier > 0
                ? $"{weapon.DamageDice}+{weapon.DamageModifier}"
                : $"{weapon.DamageDice}{weapon.DamageModifier}";
        }

        await RollDice($"{weapon.Name} Damage", expression);
    }

    private async Task RollD20WithModifier(string purpose, int modifier)
    {
        if (_sessionId == null) return;

        try
        {
            var roll = _gameService.RollWithModifier(modifier, _character.Name, purpose);
            await _gameService.LogDiceRollAsync(_sessionId.Value, roll);

            string resultText = $"{roll.Expression}: {roll.Total}";
            _onRollCallback(purpose, resultText);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error rolling dice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RollDice(string purpose, string expression)
    {
        if (_sessionId == null) return;

        try
        {
            var roll = await _gameService.RollAndLogAsync(_sessionId.Value, expression, _character.Name, purpose);

            string resultText = $"{roll.Expression}: {roll.Total}";
            _onRollCallback(purpose, resultText);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error rolling dice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Helper method to find child controls
    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                yield return t;
            }

            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    private class Weapon
    {
        public string Name { get; set; } = string.Empty;
        public int AttackBonus { get; set; }
        public string Damage { get; set; } = string.Empty;
        public string DamageDice { get; set; } = string.Empty;
        public int DamageModifier { get; set; }
    }
}

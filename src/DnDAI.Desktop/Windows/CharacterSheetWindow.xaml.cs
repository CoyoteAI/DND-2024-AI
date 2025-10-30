using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DnDAI.Core.Models;
using DnDAI.Services;
using System.Text.RegularExpressions;
using DnDAI.Desktop.Dialogs;

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

        // Attach mouse wheel handler to all child controls
        Loaded += (s, e) =>
        {
            // Recursively attach handlers to all controls that might capture mouse wheel
            AttachMouseWheelToChildren(MainScrollViewer);

            // Also attach to the ScrollViewer itself
            MainScrollViewer.PreviewMouseWheel += InterceptMouseWheel;

            // Reload skills after window is fully loaded (visual tree is ready)
            LoadSkills();
        };

        LoadCharacterData();
    }

    private void AttachMouseWheelToChildren(DependencyObject parent)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // Attach handler to this child if it's a UIElement
            if (child is UIElement element)
            {
                element.PreviewMouseWheel += InterceptMouseWheel;
            }

            // Recursively process children
            AttachMouseWheelToChildren(child);
        }
    }

    private void InterceptMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Calculate new offset
        double offset = MainScrollViewer.VerticalOffset - (e.Delta / 3.0);
        MainScrollViewer.ScrollToVerticalOffset(Math.Max(0, offset));

        // Mark as handled to prevent child controls from processing it
        e.Handled = true;
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

        // Load spells
        LoadSpells();

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
        // Get saving throw proficiencies from character or class defaults
        var proficientSaves = GetSavingThrowProficiencies();
        var saveButtons = SavingThrowsPanel.Children.OfType<Button>().ToList();

        foreach (var button in saveButtons)
        {
            string ability = button.Tag.ToString() ?? "";
            int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);

            // Add proficiency bonus if proficient
            if (proficientSaves.Contains(ability))
            {
                modifier += _proficiencyBonus;
            }

            // Show proficiency indicator
            string indicator = proficientSaves.Contains(ability) ? "★ " : "";
            button.Content = $"{indicator}{ability} Save: {FormatModifier(modifier)}";
        }
    }

    private HashSet<string> GetSavingThrowProficiencies()
    {
        // If character has explicit proficiencies set, use those
        if (!string.IsNullOrWhiteSpace(_character.SavingThrowProficiencies))
        {
            return _character.SavingThrowProficiencies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToHashSet();
        }

        // Otherwise, use class defaults (D&D 2024)
        return _character.Class switch
        {
            Core.Enums.CharacterClass.Barbarian => new HashSet<string> { "STR", "CON" },
            Core.Enums.CharacterClass.Bard => new HashSet<string> { "DEX", "CHA" },
            Core.Enums.CharacterClass.Cleric => new HashSet<string> { "WIS", "CHA" },
            Core.Enums.CharacterClass.Druid => new HashSet<string> { "INT", "WIS" },
            Core.Enums.CharacterClass.Fighter => new HashSet<string> { "STR", "CON" },
            Core.Enums.CharacterClass.Monk => new HashSet<string> { "STR", "DEX" },
            Core.Enums.CharacterClass.Paladin => new HashSet<string> { "WIS", "CHA" },
            Core.Enums.CharacterClass.Ranger => new HashSet<string> { "STR", "DEX" },
            Core.Enums.CharacterClass.Rogue => new HashSet<string> { "DEX", "INT" },
            Core.Enums.CharacterClass.Sorcerer => new HashSet<string> { "CON", "CHA" },
            Core.Enums.CharacterClass.Warlock => new HashSet<string> { "WIS", "CHA" },
            Core.Enums.CharacterClass.Wizard => new HashSet<string> { "INT", "WIS" },
            Core.Enums.CharacterClass.Artificer => new HashSet<string> { "CON", "INT" },
            _ => new HashSet<string>()
        };
    }

    private void LoadSkills()
    {
        // Update all skill buttons with their modifiers
        var skillButtons = FindVisualChildren<Button>(this)
            .Where(b => b.Tag?.ToString()?.Contains('|') == true)
            .ToList();

        // Parse skill proficiencies and expertise
        var proficientSkills = _character.SkillProficiencies?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();
        var expertiseSkills = _character.SkillExpertise?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();

        foreach (var button in skillButtons)
        {
            string tag = button.Tag.ToString() ?? "";
            var parts = tag.Split('|');
            if (parts.Length == 2)
            {
                string skillName = parts[0];
                string ability = parts[1];
                int modifier = _abilityModifiers.GetValueOrDefault(ability, 0);

                // Add proficiency bonus if proficient
                if (proficientSkills.Contains(skillName))
                {
                    modifier += _proficiencyBonus;
                }

                // Add proficiency bonus again if expertise
                if (expertiseSkills.Contains(skillName))
                {
                    modifier += _proficiencyBonus;
                }

                // Show proficiency indicator
                string indicator = expertiseSkills.Contains(skillName) ? "★★ " :
                                   proficientSkills.Contains(skillName) ? "★ " : "";

                button.Content = $"{indicator}{skillName} ({ability}): {FormatModifier(modifier)}";
            }
        }
    }

    private void LoadWeapons()
    {
        WeaponsPanel.Children.Clear();

        // Add "Add Custom Weapon" button at top
        var addWeaponButton = new Button
        {
            Content = "+ Add Custom Weapon",
            Height = 35,
            Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Margin = new Thickness(0, 0, 0, 10)
        };
        addWeaponButton.Click += AddCustomWeapon_Click;
        WeaponsPanel.Children.Add(addWeaponButton);

        // Load custom weapons first
        var hasCustomWeapons = _character.CustomWeapons?.Any() == true;
        if (hasCustomWeapons)
        {
            foreach (var customWeapon in _character.CustomWeapons!)
            {
                AddCustomWeaponDisplay(customWeapon);
            }
        }

        // Parse equipment for weapons
        var parsedWeapons = ParseWeaponsFromEquipment(_character.Equipment ?? "");

        if (!hasCustomWeapons && !parsedWeapons.Any())
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

        // Display parsed weapons
        foreach (var weapon in parsedWeapons)
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

            // Damage button(s)
            if (weapon.IsVersatile)
            {
                // One-handed damage button
                var damageOneHandButton = new Button
                {
                    Content = $"Dmg (1H): {weapon.Damage}",
                    Width = 120,
                    Height = 30,
                    Margin = new Thickness(5, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = weapon
                };
                damageOneHandButton.Click += WeaponDamage_Click;
                buttonPanel.Children.Add(damageOneHandButton);

                // Two-handed damage button (versatile)
                var damageTwoHandButton = new Button
                {
                    Content = $"Dmg (2H): {weapon.VersatileDamage}",
                    Width = 120,
                    Height = 30,
                    Margin = new Thickness(5, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(155, 39, 29)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = (weapon, true) // tuple to indicate two-handed
                };
                damageTwoHandButton.Click += WeaponDamageTwoHanded_Click;
                buttonPanel.Children.Add(damageTwoHandButton);
            }
            else
            {
                // Standard single damage button
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
            }

            weaponPanel.Children.Add(buttonPanel);
            WeaponsPanel.Children.Add(weaponPanel);
        }
    }

    private List<Weapon> ParseWeaponsFromEquipment(string equipment)
    {
        var weapons = new List<Weapon>();

        // Common weapon patterns: (one-handed damage, finesse, versatile two-handed damage)
        var weaponPatterns = new Dictionary<string, (string damage, bool finesse, string? versatile)>
        {
            { "longsword", ("1d8", false, "1d10") },
            { "shortsword", ("1d6", true, null) },
            { "greatsword", ("2d6", false, null) },
            { "rapier", ("1d8", true, null) },
            { "dagger", ("1d4", true, null) },
            { "handaxe", ("1d6", false, null) },
            { "battleaxe", ("1d8", false, "1d10") },
            { "greataxe", ("1d12", false, null) },
            { "mace", ("1d6", false, null) },
            { "warhammer", ("1d8", false, "1d10") },
            { "maul", ("2d6", false, null) },
            { "spear", ("1d6", false, "1d8") },
            { "trident", ("1d6", false, "1d8") },
            { "quarterstaff", ("1d6", false, "1d8") },
            { "club", ("1d4", false, null) },
            { "greatclub", ("1d8", false, null) },
            { "shortbow", ("1d6", true, null) },
            { "longbow", ("1d8", true, null) },
            { "crossbow", ("1d8", true, null) },
            { "light crossbow", ("1d8", true, null) },
            { "heavy crossbow", ("1d10", true, null) }
        };

        string lowerEquip = equipment.ToLower();

        foreach (var kvp in weaponPatterns)
        {
            if (lowerEquip.Contains(kvp.Key))
            {
                string weaponName = char.ToUpper(kvp.Key[0]) + kvp.Key.Substring(1);
                string damage = kvp.Value.damage;
                bool isFinesse = kvp.Value.finesse;
                string? versatileDamage = kvp.Value.versatile;

                // Calculate attack bonus
                int strMod = _abilityModifiers.GetValueOrDefault("STR", 0);
                int dexMod = _abilityModifiers.GetValueOrDefault("DEX", 0);
                int attackMod = isFinesse ? Math.Max(strMod, dexMod) : strMod;
                int attackBonus = attackMod + _proficiencyBonus;

                // Calculate damage modifier
                int damageMod = isFinesse ? Math.Max(strMod, dexMod) : strMod;
                string damageString = damageMod >= 0 ? $"{damage}+{damageMod}" : $"{damage}{damageMod}";

                var weapon = new Weapon
                {
                    Name = weaponName,
                    AttackBonus = attackBonus,
                    Damage = damageString,
                    DamageDice = damage,
                    DamageModifier = damageMod,
                    IsVersatile = versatileDamage != null
                };

                // Add versatile damage if applicable
                if (versatileDamage != null)
                {
                    string versatileDamageString = damageMod >= 0
                        ? $"{versatileDamage}+{damageMod}"
                        : $"{versatileDamage}{damageMod}";
                    weapon.VersatileDamage = versatileDamageString;
                    weapon.VersatileDamageDice = versatileDamage;
                }

                weapons.Add(weapon);
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

        // Add proficiency bonus if proficient in this save
        var proficientSaves = GetSavingThrowProficiencies();
        if (proficientSaves.Contains(ability))
        {
            modifier += _proficiencyBonus;
        }

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

        // Parse skill proficiencies and expertise
        var proficientSkills = _character.SkillProficiencies?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();
        var expertiseSkills = _character.SkillExpertise?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).ToHashSet() ?? new HashSet<string>();

        // Add proficiency bonus if proficient
        if (proficientSkills.Contains(skillName))
        {
            modifier += _proficiencyBonus;
        }

        // Add proficiency bonus again if expertise
        if (expertiseSkills.Contains(skillName))
        {
            modifier += _proficiencyBonus;
        }

        await RollD20WithModifier($"{skillName} Check", modifier);
    }

    private async void ManageSkills_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SkillProficiencyDialog(_character.SkillProficiencies, _character.SkillExpertise)
        {
            Owner = this
        };

        if (dialog.ShowDialog() == true)
        {
            // Update character's skill proficiencies
            _character.SkillProficiencies = dialog.SkillProficiencies;
            _character.SkillExpertise = dialog.SkillExpertise;

            try
            {
                // Save to database
                await _gameService.UpdatePlayerCharacterAsync(_character);

                // Refresh skill display
                LoadSkills();

                MessageBox.Show("Skill proficiencies updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating skill proficiencies: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
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

        await RollDice($"{weapon.Name} Damage (1H)", expression);
    }

    private async void WeaponDamageTwoHanded_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ValueTuple<Weapon, bool> tag)
            return;

        var weapon = tag.Item1;

        string expression = weapon.VersatileDamageDice ?? weapon.DamageDice;
        if (weapon.DamageModifier != 0)
        {
            expression = weapon.DamageModifier > 0
                ? $"{expression}+{weapon.DamageModifier}"
                : $"{expression}{weapon.DamageModifier}";
        }

        await RollDice($"{weapon.Name} Damage (2H)", expression);
    }

    // Custom Weapon Display and Handling
    private void AddCustomWeaponDisplay(CustomWeapon customWeapon)
    {
        var weaponPanel = new StackPanel
        {
            Margin = new Thickness(0, 5, 0, 10),
            Background = new SolidColorBrush(Color.FromRgb(255, 250, 240))
        };

        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5, 5, 5, 3)
        };

        var weaponHeader = new TextBlock
        {
            Text = customWeapon.Name,
            FontSize = 14,
            FontWeight = FontWeights.Bold
        };
        headerPanel.Children.Add(weaponHeader);

        if (customWeapon.MagicBonus > 0)
        {
            var bonusText = new TextBlock
            {
                Text = $" +{customWeapon.MagicBonus}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                Margin = new Thickness(5, 0, 0, 0)
            };
            headerPanel.Children.Add(bonusText);
        }

        if (!string.IsNullOrWhiteSpace(customWeapon.BaseWeaponType))
        {
            var baseText = new TextBlock
            {
                Text = $" ({customWeapon.BaseWeaponType})",
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(3, 2, 0, 0)
            };
            headerPanel.Children.Add(baseText);
        }

        weaponPanel.Children.Add(headerPanel);

        // Attack and Damage buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5, 0, 5, 5)
        };

        // Calculate modifiers
        int strMod = _abilityModifiers.GetValueOrDefault("STR", 0);
        int dexMod = _abilityModifiers.GetValueOrDefault("DEX", 0);
        int attackMod = customWeapon.IsFinesse ? Math.Max(strMod, dexMod) : strMod;
        int attackBonus = attackMod + _proficiencyBonus + customWeapon.MagicBonus + customWeapon.AttackBonusOverride;
        int damageMod = customWeapon.IsFinesse ? Math.Max(strMod, dexMod) : strMod;
        int totalDamageBonus = damageMod + customWeapon.MagicBonus + customWeapon.DamageBonusOverride;

        // Attack button
        var attackButton = new Button
        {
            Content = $"Attack: {FormatModifier(attackBonus)}",
            Width = 120,
            Height = 30,
            Margin = new Thickness(0, 0, 5, 0),
            Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Tag = (customWeapon, attackBonus)
        };
        attackButton.Click += CustomWeaponAttack_Click;
        buttonPanel.Children.Add(attackButton);

        // Damage buttons
        if (customWeapon.IsVersatile)
        {
            string oneHandDamage = totalDamageBonus >= 0
                ? $"{customWeapon.DamageDice}+{totalDamageBonus}"
                : $"{customWeapon.DamageDice}{totalDamageBonus}";

            var dmgOneHand = new Button
            {
                Content = $"Dmg (1H): {oneHandDamage}",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = (customWeapon, customWeapon.DamageDice, totalDamageBonus, false)
            };
            dmgOneHand.Click += CustomWeaponDamage_Click;
            buttonPanel.Children.Add(dmgOneHand);

            string twoHandDamage = totalDamageBonus >= 0
                ? $"{customWeapon.VersatileDamageDice}+{totalDamageBonus}"
                : $"{customWeapon.VersatileDamageDice}{totalDamageBonus}";

            var dmgTwoHand = new Button
            {
                Content = $"Dmg (2H): {twoHandDamage}",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(155, 39, 29)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = (customWeapon, customWeapon.VersatileDamageDice, totalDamageBonus, true)
            };
            dmgTwoHand.Click += CustomWeaponDamage_Click;
            buttonPanel.Children.Add(dmgTwoHand);
        }
        else
        {
            string damage = totalDamageBonus >= 0
                ? $"{customWeapon.DamageDice}+{totalDamageBonus}"
                : $"{customWeapon.DamageDice}{totalDamageBonus}";

            var dmgButton = new Button
            {
                Content = $"Damage: {damage}",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 5, 0),
                Background = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = (customWeapon, customWeapon.DamageDice, totalDamageBonus, false)
            };
            dmgButton.Click += CustomWeaponDamage_Click;
            buttonPanel.Children.Add(dmgButton);
        }

        weaponPanel.Children.Add(buttonPanel);

        // Special Abilities
        if (customWeapon.SpecialAbilities?.Any() == true)
        {
            var abilitiesPanel = new WrapPanel
            {
                Margin = new Thickness(5, 5, 5, 5)
            };

            foreach (var ability in customWeapon.SpecialAbilities)
            {
                var abilityButton = new Button
                {
                    Height = 28,
                    Margin = new Thickness(0, 0, 5, 3),
                    Background = new SolidColorBrush(Color.FromRgb(142, 68, 173)),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = ability
                };

                string content = ability.Name;
                if (!string.IsNullOrWhiteSpace(ability.DiceRoll))
                {
                    content += $": {ability.DiceRoll}";
                    if (!string.IsNullOrWhiteSpace(ability.DamageType))
                    {
                        content += $" {ability.DamageType}";
                    }
                }
                abilityButton.Content = content;
                abilityButton.Click += SpecialAbility_Click;

                abilitiesPanel.Children.Add(abilityButton);
            }

            weaponPanel.Children.Add(abilitiesPanel);
        }

        WeaponsPanel.Children.Add(weaponPanel);
    }

    private async void AddCustomWeapon_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CustomWeaponDialog
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.Weapon != null)
        {
            try
            {
                await _gameService.AddCustomWeaponAsync(_character.Id, dialog.Weapon);
                MessageBox.Show($"Custom weapon '{dialog.Weapon.Name}' added successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload character and weapons
                var updatedCharacter = await _gameService.GetCampaignAsync(_character.CampaignId);
                var updatedPC = updatedCharacter?.PlayerCharacters.FirstOrDefault(pc => pc.Id == _character.Id);
                if (updatedPC != null)
                {
                    // Update the character reference
                    _character.CustomWeapons.Clear();
                    foreach (var weapon in updatedPC.CustomWeapons)
                    {
                        _character.CustomWeapons.Add(weapon);
                    }
                    LoadWeapons();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding custom weapon: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void CustomWeaponAttack_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ValueTuple<CustomWeapon, int> tag)
            return;

        var weapon = tag.Item1;
        var attackBonus = tag.Item2;

        await RollD20WithModifier($"{weapon.Name} Attack", attackBonus);
    }

    private async void CustomWeaponDamage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not ValueTuple<CustomWeapon, string, int, bool> tag)
            return;

        var weapon = tag.Item1;
        var damageDice = tag.Item2;
        var damageBonus = tag.Item3;
        var isTwoHanded = tag.Item4;

        string expression = damageBonus >= 0
            ? $"{damageDice}+{damageBonus}"
            : $"{damageDice}{damageBonus}";

        string purpose = isTwoHanded
            ? $"{weapon.Name} Damage (2H)"
            : weapon.IsVersatile
                ? $"{weapon.Name} Damage (1H)"
                : $"{weapon.Name} Damage";

        await RollDice(purpose, expression);
    }

    private async void SpecialAbility_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not WeaponAbility ability)
            return;

        if (!string.IsNullOrWhiteSpace(ability.DiceRoll))
        {
            // Has a dice roll component
            await RollDice(ability.Name, ability.DiceRoll);
        }
        else
        {
            // No dice roll, just show description
            string message = $"{ability.Name}\n\n{ability.Description}";
            if (!string.IsNullOrWhiteSpace(ability.UsageLimit))
            {
                message += $"\n\nUsage: {ability.UsageLimit}";
            }
            MessageBox.Show(message, ability.Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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

    // Spell System Methods
    private void LoadSpells()
    {
        // Check if character has any spell slots or known spells
        bool hasSpellSlots = HasAnySpellSlots();
        bool hasSpells = _character.PlayerCharacterSpells?.Any() == true;

        if (!hasSpellSlots && !hasSpells)
        {
            SpellcastingSection.Visibility = Visibility.Collapsed;
            return;
        }

        SpellcastingSection.Visibility = Visibility.Visible;

        // Load spell slots
        LoadSpellSlots();

        // Load learned spells
        LoadLearnedSpells();
    }

    private bool HasAnySpellSlots()
    {
        return _character.SpellSlots1Max > 0 ||
               _character.SpellSlots2Max > 0 ||
               _character.SpellSlots3Max > 0 ||
               _character.SpellSlots4Max > 0 ||
               _character.SpellSlots5Max > 0 ||
               _character.SpellSlots6Max > 0 ||
               _character.SpellSlots7Max > 0 ||
               _character.SpellSlots8Max > 0 ||
               _character.SpellSlots9Max > 0;
    }

    private void LoadSpellSlots()
    {
        SpellSlotsPanel.Children.Clear();

        for (int level = 1; level <= 9; level++)
        {
            var currentProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Current");
            var maxProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Max");

            if (currentProp != null && maxProp != null)
            {
                int current = (int)(currentProp.GetValue(_character) ?? 0);
                int max = (int)(maxProp.GetValue(_character) ?? 0);

                if (max > 0)
                {
                    var slotPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    var levelText = new TextBlock
                    {
                        Text = $"Level {level}:",
                        Width = 60,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 11
                    };
                    slotPanel.Children.Add(levelText);

                    // Slot circles
                    for (int i = 0; i < max; i++)
                    {
                        var circle = new System.Windows.Shapes.Ellipse
                        {
                            Width = 16,
                            Height = 16,
                            Margin = new Thickness(2, 0, 2, 0),
                            Stroke = Brushes.Gray,
                            StrokeThickness = 2,
                            Fill = i < current
                                ? new SolidColorBrush(Color.FromRgb(155, 89, 182))
                                : Brushes.Transparent
                        };
                        slotPanel.Children.Add(circle);
                    }

                    var countText = new TextBlock
                    {
                        Text = $" {current}/{max}",
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 11,
                        Margin = new Thickness(5, 0, 0, 0),
                        Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141))
                    };
                    slotPanel.Children.Add(countText);

                    SpellSlotsPanel.Children.Add(slotPanel);
                }
            }
        }
    }

    private void LoadLearnedSpells()
    {
        SpellsPanel.Children.Clear();

        if (_character.PlayerCharacterSpells == null || !_character.PlayerCharacterSpells.Any())
        {
            SpellsPanel.Children.Add(new TextBlock
            {
                Text = "No spells learned",
                FontStyle = FontStyles.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                Margin = new Thickness(5)
            });
            return;
        }

        // Group spells by level
        var spellsByLevel = _character.PlayerCharacterSpells
            .OrderBy(pcs => pcs.Spell.Level)
            .ThenBy(pcs => pcs.Spell.Name)
            .GroupBy(pcs => pcs.Spell.Level);

        foreach (var levelGroup in spellsByLevel)
        {
            int level = levelGroup.Key;
            string levelHeader = level == 0 ? "Cantrips" : $"Level {level} Spells";

            var headerText = new TextBlock
            {
                Text = levelHeader,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
                Margin = new Thickness(0, 10, 0, 5)
            };
            SpellsPanel.Children.Add(headerText);

            foreach (var pcSpell in levelGroup)
            {
                AddSpellDisplay(pcSpell);
            }
        }
    }

    private void AddSpellDisplay(PlayerCharacterSpell pcSpell)
    {
        var spell = pcSpell.Spell;

        var spellPanel = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(250, 250, 255)),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(8),
            Margin = new Thickness(0, 2, 0, 2)
        };

        var content = new StackPanel();

        // Spell name and components
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var nameText = new TextBlock
        {
            Text = spell.Name,
            FontWeight = FontWeights.Bold,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };
        headerPanel.Children.Add(nameText);

        // Components indicator
        var components = new List<string>();
        if (spell.HasVerbal) components.Add("V");
        if (spell.HasSomatic) components.Add("S");
        if (spell.HasMaterial) components.Add("M");

        if (components.Any())
        {
            var compText = new TextBlock
            {
                Text = $" ({string.Join(", ", components)})",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0)
            };
            headerPanel.Children.Add(compText);
        }

        // Concentration and Ritual indicators
        if (spell.RequiresConcentration)
        {
            var concText = new TextBlock
            {
                Text = " [C]",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 0, 0)
            };
            headerPanel.Children.Add(concText);
        }

        if (spell.IsRitual)
        {
            var ritualText = new TextBlock
            {
                Text = " [R]",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(3, 0, 0, 0)
            };
            headerPanel.Children.Add(ritualText);
        }

        content.Children.Add(headerPanel);

        // Spell details (school, range, casting time)
        var detailsText = new TextBlock
        {
            Text = $"{spell.School} • {spell.Range} • {spell.CastingTime}",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141)),
            Margin = new Thickness(0, 2, 0, 5)
        };
        content.Children.Add(detailsText);

        // Action buttons
        var buttonPanel = new WrapPanel
        {
            Orientation = Orientation.Horizontal
        };

        // Cast button
        var castButton = new Button
        {
            Content = spell.Level == 0 ? "Cast Cantrip" : $"Cast (Level {spell.Level})",
            Height = 26,
            Margin = new Thickness(0, 0, 5, 0),
            Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Padding = new Thickness(10, 3, 10, 3),
            Tag = pcSpell
        };
        castButton.Click += CastSpell_Click;
        buttonPanel.Children.Add(castButton);

        // Info button
        var infoButton = new Button
        {
            Content = "ℹ️ Info",
            Height = 26,
            Margin = new Thickness(0, 0, 5, 0),
            Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Padding = new Thickness(8, 3, 8, 3),
            Tag = spell
        };
        infoButton.Click += SpellInfo_Click;
        buttonPanel.Children.Add(infoButton);

        content.Children.Add(buttonPanel);

        spellPanel.Child = content;
        SpellsPanel.Children.Add(spellPanel);
    }

    private async void CastSpell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not PlayerCharacterSpell pcSpell)
            return;

        var spell = pcSpell.Spell;

        // Check and consume spell slot (if not a cantrip)
        if (spell.Level > 0)
        {
            var currentProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{spell.Level}Current");
            int current = (int)(currentProp?.GetValue(_character) ?? 0);

            if (current <= 0)
            {
                MessageBox.Show($"No level {spell.Level} spell slots remaining!",
                    "Cannot Cast Spell", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Consume spell slot
            bool success = await _gameService.UseSpellSlotAsync(_character.Id, spell.Level);
            if (success)
            {
                currentProp?.SetValue(_character, current - 1);
                LoadSpellSlots(); // Refresh display
            }
        }

        // Handle different spell types
        if (!string.IsNullOrWhiteSpace(spell.AttackType))
        {
            // Spell attack roll
            int spellcastingMod = GetSpellcastingModifier();
            int attackBonus = spellcastingMod + _proficiencyBonus;
            await RollD20WithModifier($"{spell.Name} Spell Attack", attackBonus);
        }

        // Roll damage if applicable
        if (!string.IsNullOrWhiteSpace(spell.DamageDice))
        {
            int spellcastingMod = GetSpellcastingModifier();
            string expression = spell.DamageDice;

            // Some spells add spellcasting modifier to damage
            if (spell.Level == 0 || spell.Name.Contains("Magic Missile"))
            {
                expression = spellcastingMod > 0
                    ? $"{spell.DamageDice}+{spellcastingMod}"
                    : spell.DamageDice;
            }

            string damageType = !string.IsNullOrWhiteSpace(spell.DamageType)
                ? $" {spell.DamageType}"
                : "";
            await RollDice($"{spell.Name} Damage{damageType}", expression);
        }

        // Show save DC if applicable
        if (!string.IsNullOrWhiteSpace(spell.SaveType))
        {
            int spellcastingMod = GetSpellcastingModifier();
            int saveDC = 8 + _proficiencyBonus + spellcastingMod;
            MessageBox.Show($"{spell.Name}\n\nTarget must make a DC {saveDC} {spell.SaveType} saving throw.",
                "Spell Cast", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // If spell has no attack, damage, or save, just show it was cast
        if (string.IsNullOrWhiteSpace(spell.AttackType) &&
            string.IsNullOrWhiteSpace(spell.DamageDice) &&
            string.IsNullOrWhiteSpace(spell.SaveType))
        {
            MessageBox.Show($"You cast {spell.Name}!\n\n{spell.Description}",
                "Spell Cast", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SpellInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not Spell spell)
            return;

        var components = new List<string>();
        if (spell.HasVerbal) components.Add("V");
        if (spell.HasSomatic) components.Add("S");
        if (spell.HasMaterial)
        {
            string matComp = !string.IsNullOrWhiteSpace(spell.MaterialComponents)
                ? $"M ({spell.MaterialComponents})"
                : "M";
            components.Add(matComp);
        }

        string info = $"{spell.Name}\n";
        info += $"Level {spell.Level} {spell.School}\n\n";
        info += $"Casting Time: {spell.CastingTime}\n";
        info += $"Range: {spell.Range}\n";
        info += $"Components: {string.Join(", ", components)}\n";
        info += $"Duration: {spell.Duration}\n";
        if (spell.RequiresConcentration) info += "Concentration: Yes\n";
        if (spell.IsRitual) info += "Ritual: Yes\n";
        info += $"\n{spell.Description}";

        if (!string.IsNullOrWhiteSpace(spell.AtHigherLevels))
        {
            info += $"\n\nAt Higher Levels: {spell.AtHigherLevels}";
        }

        if (!string.IsNullOrWhiteSpace(spell.DamageDice))
        {
            info += $"\n\nDamage: {spell.DamageDice}";
            if (!string.IsNullOrWhiteSpace(spell.DamageType))
                info += $" {spell.DamageType}";
        }

        if (!string.IsNullOrWhiteSpace(spell.SaveType))
        {
            int spellcastingMod = GetSpellcastingModifier();
            int saveDC = 8 + _proficiencyBonus + spellcastingMod;
            info += $"\n\nSave: DC {saveDC} {spell.SaveType}";
        }

        MessageBox.Show(info, spell.Name, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void LearnSpell_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SpellDialog
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && dialog.Spell != null)
        {
            try
            {
                // Save spell and link to character
                var createdSpell = await _gameService.CreateSpellAsync(dialog.Spell);
                await _gameService.LearnSpellAsync(_character.Id, createdSpell.Id, isPrepared: true);

                MessageBox.Show($"Spell '{createdSpell.Name}' learned successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reload character and spells
                var updatedCharacter = await _gameService.GetCampaignAsync(_character.CampaignId);
                var updatedPC = updatedCharacter?.PlayerCharacters.FirstOrDefault(pc => pc.Id == _character.Id);
                if (updatedPC != null)
                {
                    _character.PlayerCharacterSpells.Clear();
                    foreach (var pcSpell in updatedPC.PlayerCharacterSpells)
                    {
                        _character.PlayerCharacterSpells.Add(pcSpell);
                    }
                    LoadSpells();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error learning spell: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void RestoreSpellSlots_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _gameService.RestoreSpellSlotsAsync(_character.Id);

            // Update local character object
            for (int level = 1; level <= 9; level++)
            {
                var currentProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Current");
                var maxProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Max");

                if (currentProp != null && maxProp != null)
                {
                    int max = (int)(maxProp.GetValue(_character) ?? 0);
                    currentProp.SetValue(_character, max);
                }
            }

            LoadSpellSlots();

            MessageBox.Show("All spell slots restored!",
                "Long Rest", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error restoring spell slots: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private int GetSpellcastingModifier()
    {
        // Determine spellcasting ability based on class
        return _character.Class switch
        {
            Core.Enums.CharacterClass.Wizard => _abilityModifiers.GetValueOrDefault("INT", 0),
            Core.Enums.CharacterClass.Artificer => _abilityModifiers.GetValueOrDefault("INT", 0),
            Core.Enums.CharacterClass.Cleric => _abilityModifiers.GetValueOrDefault("WIS", 0),
            Core.Enums.CharacterClass.Druid => _abilityModifiers.GetValueOrDefault("WIS", 0),
            Core.Enums.CharacterClass.Ranger => _abilityModifiers.GetValueOrDefault("WIS", 0),
            Core.Enums.CharacterClass.Bard => _abilityModifiers.GetValueOrDefault("CHA", 0),
            Core.Enums.CharacterClass.Sorcerer => _abilityModifiers.GetValueOrDefault("CHA", 0),
            Core.Enums.CharacterClass.Warlock => _abilityModifiers.GetValueOrDefault("CHA", 0),
            Core.Enums.CharacterClass.Paladin => _abilityModifiers.GetValueOrDefault("CHA", 0),
            _ => _abilityModifiers.GetValueOrDefault("INT", 0) // Default to INT
        };
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
        public bool IsVersatile { get; set; }
        public string? VersatileDamage { get; set; }
        public string? VersatileDamageDice { get; set; }
    }
}

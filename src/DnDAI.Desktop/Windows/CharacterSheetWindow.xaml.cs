using System.Windows;
using System.Windows.Controls;
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

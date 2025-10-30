namespace DnDAI.Core.Models;

public class CustomWeapon : BaseEntity
{
    public int PlayerCharacterId { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? BaseWeaponType { get; set; } // e.g., "Longsword", "Dagger"
    public int MagicBonus { get; set; } = 0;

    // Attack and Damage (can be customized beyond base weapon)
    public int AttackBonusOverride { get; set; } = 0; // Additional bonus beyond normal calculation
    public string DamageDice { get; set; } = "1d8"; // One-handed or only damage
    public int DamageBonusOverride { get; set; } = 0; // Additional bonus beyond ability modifier

    // Versatile
    public bool IsVersatile { get; set; }
    public string? VersatileDamageDice { get; set; } // Two-handed damage

    // Finesse
    public bool IsFinesse { get; set; }

    // Special Abilities - stored as collection
    public ICollection<WeaponAbility> SpecialAbilities { get; set; } = new List<WeaponAbility>();

    public string Notes { get; set; } = string.Empty;
}

public class WeaponAbility : BaseEntity
{
    public int CustomWeaponId { get; set; }
    public CustomWeapon CustomWeapon { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // For abilities that roll dice (like Green Flame Blade)
    public string? DiceRoll { get; set; } // e.g., "1d8", "2d6"
    public string? DamageType { get; set; } // e.g., "fire", "cold", "force"

    public string UsageLimit { get; set; } = "at will"; // "at will", "1/day", "3/day", etc.
    public string ActionType { get; set; } = "action"; // "action", "bonus action", "reaction"
}

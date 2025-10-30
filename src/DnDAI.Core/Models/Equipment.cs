using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class Equipment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public EquipmentType Type { get; set; }
    public bool IsStandard { get; set; } = true; // True for seeded items, false for custom

    // Basic properties
    public decimal CostInGold { get; set; }
    public decimal Weight { get; set; }

    // Weapon properties
    public WeaponCategory? WeaponCategory { get; set; }
    public string? Damage { get; set; } // e.g., "1d8"
    public string? DamageType { get; set; } // Slashing, Piercing, Bludgeoning
    public bool IsFinesse { get; set; }
    public bool IsVersatile { get; set; }
    public string? VersatileDamage { get; set; } // e.g., "1d10" for versatile
    public string? Range { get; set; } // e.g., "80/320" for ranged weapons

    // Armor properties
    public ArmorCategory? ArmorCategory { get; set; }
    public int? ArmorClass { get; set; } // Base AC
    public bool? AddDexModifier { get; set; } // Can add DEX mod?
    public int? MaxDexModifier { get; set; } // Max DEX mod (for medium armor)
    public bool StealthDisadvantage { get; set; }
    public int? StrengthRequirement { get; set; }

    // Magic item properties
    public ItemRarity? Rarity { get; set; }
    public bool RequiresAttunement { get; set; }
    public int? MaxCharges { get; set; }
    public string? ChargeRegeneration { get; set; } // e.g., "1d6+1 at dawn"

    // Additional properties (JSON for flexibility)
    public string? PropertiesJson { get; set; }

    // Relationships
    public ICollection<CharacterEquipment> CharacterEquipment { get; set; } = new List<CharacterEquipment>();
}

namespace DnDAI.Core.Models;

public class Spell : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } // 0 = Cantrip, 1-9 = Spell levels
    public string School { get; set; } = string.Empty; // Evocation, Abjuration, etc.
    public string CastingTime { get; set; } = "1 action";
    public string Range { get; set; } = "Self";
    public string Duration { get; set; } = "Instantaneous";

    // Components
    public bool HasVerbal { get; set; }
    public bool HasSomatic { get; set; }
    public bool HasMaterial { get; set; }
    public string MaterialComponents { get; set; } = string.Empty;

    public bool RequiresConcentration { get; set; }
    public bool IsRitual { get; set; }

    public string Description { get; set; } = string.Empty;
    public string AtHigherLevels { get; set; } = string.Empty;

    // Combat/Effect properties
    public string? DamageDice { get; set; } // e.g., "1d8", "3d6"
    public string? DamageType { get; set; } // fire, cold, force, etc.
    public string? SaveType { get; set; } // DEX, CON, WIS, etc.
    public string? AttackType { get; set; } // "melee spell attack", "ranged spell attack", null if no attack

    // Scaling - how damage increases per level
    public string? ScalingType { get; set; } // "character_level" for cantrips, "spell_level" for leveled spells
    public string? ScalingDice { get; set; } // Additional dice per level, e.g., "1d8"

    public bool IsCustom { get; set; } // True for homebrew spells
    public string Source { get; set; } = "Homebrew"; // PHB, XGE, Homebrew, etc.

    public ICollection<PlayerCharacterSpell> PlayerCharacterSpells { get; set; } = new List<PlayerCharacterSpell>();
}

public class PlayerCharacterSpell : BaseEntity
{
    public int PlayerCharacterId { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; } = null!;

    public int SpellId { get; set; }
    public Spell Spell { get; set; } = null!;

    public bool IsPrepared { get; set; } // For classes that prepare spells daily
    public bool IsAlwaysPrepared { get; set; } // Domain spells, etc.

    public string Notes { get; set; } = string.Empty;
}

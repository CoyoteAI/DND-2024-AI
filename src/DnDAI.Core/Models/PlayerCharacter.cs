using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class PlayerCharacter : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public CharacterRace Race { get; set; }
    public CharacterClass Class { get; set; }
    public Alignment Alignment { get; set; }

    public string Background { get; set; } = string.Empty;
    public string Backstory { get; set; } = string.Empty;

    // Stats
    public int Level { get; set; } = 1;
    public int ExperiencePoints { get; set; } = 0;
    public int MaxHitPoints { get; set; } = 10;
    public int CurrentHitPoints { get; set; } = 10;

    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    public string Equipment { get; set; } = string.Empty;
    public string Inventory { get; set; } = string.Empty;
    public int Gold { get; set; } = 0;

    // Skills (comma-separated lists of skill names)
    public string SkillProficiencies { get; set; } = string.Empty;
    public string SkillExpertise { get; set; } = string.Empty;

    // Spell Slots (for spellcasters)
    public int SpellSlots1Current { get; set; } = 0;
    public int SpellSlots1Max { get; set; } = 0;
    public int SpellSlots2Current { get; set; } = 0;
    public int SpellSlots2Max { get; set; } = 0;
    public int SpellSlots3Current { get; set; } = 0;
    public int SpellSlots3Max { get; set; } = 0;
    public int SpellSlots4Current { get; set; } = 0;
    public int SpellSlots4Max { get; set; } = 0;
    public int SpellSlots5Current { get; set; } = 0;
    public int SpellSlots5Max { get; set; } = 0;
    public int SpellSlots6Current { get; set; } = 0;
    public int SpellSlots6Max { get; set; } = 0;
    public int SpellSlots7Current { get; set; } = 0;
    public int SpellSlots7Max { get; set; } = 0;
    public int SpellSlots8Current { get; set; } = 0;
    public int SpellSlots8Max { get; set; } = 0;
    public int SpellSlots9Current { get; set; } = 0;
    public int SpellSlots9Max { get; set; } = 0;

    public bool IsAlive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<CustomWeapon> CustomWeapons { get; set; } = new List<CustomWeapon>();
    public ICollection<PlayerCharacterSpell> PlayerCharacterSpells { get; set; } = new List<PlayerCharacterSpell>();
}

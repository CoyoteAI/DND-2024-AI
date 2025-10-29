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

    public bool IsAlive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}

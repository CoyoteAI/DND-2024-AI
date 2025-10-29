using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class NPC : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public CharacterRace Race { get; set; }
    public CharacterClass? Class { get; set; }
    public Alignment Alignment { get; set; }

    public string Appearance { get; set; } = string.Empty;
    public string Personality { get; set; } = string.Empty;
    public string Backstory { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;

    // Stats
    public int Level { get; set; } = 1;
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    public int? CurrentLocationId { get; set; }
    public Location? CurrentLocation { get; set; }

    public bool IsAlive { get; set; } = true;
    public bool IsHostile { get; set; } = false;

    public string Notes { get; set; } = string.Empty;

    // Relationships
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

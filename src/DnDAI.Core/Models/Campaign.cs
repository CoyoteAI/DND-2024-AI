namespace DnDAI.Core.Models;

public class Campaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Setting { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<PlayerCharacter> PlayerCharacters { get; set; } = new List<PlayerCharacter>();
    public ICollection<NPC> NPCs { get; set; } = new List<NPC>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Quest> Quests { get; set; } = new List<Quest>();
    public ICollection<CombatEncounter> CombatEncounters { get; set; } = new List<CombatEncounter>();

    public int? CurrentLocationId { get; set; }
    public Location? CurrentLocation { get; set; }
}

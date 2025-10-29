using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class Event : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public int? SessionId { get; set; }
    public Session? Session { get; set; }

    public EventType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;

    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    public string InGameDate { get; set; } = string.Empty;

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public string ParticipantNPCIds { get; set; } = string.Empty; // JSON array
    public string ParticipantPCIds { get; set; } = string.Empty; // JSON array

    public string Consequences { get; set; } = string.Empty;
    public bool IsSignificant { get; set; } = false;

    // Navigation properties for many-to-many
    public ICollection<NPC> NPCs { get; set; } = new List<NPC>();
    public ICollection<PlayerCharacter> PlayerCharacters { get; set; } = new List<PlayerCharacter>();
}

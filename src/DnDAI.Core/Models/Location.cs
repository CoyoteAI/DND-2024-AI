using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class Location : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Geography { get; set; } = string.Empty;
    public string Climate { get; set; } = string.Empty;

    public string Notable Features { get; set; } = string.Empty;
    public string Inhabitants { get; set; } = string.Empty;
    public string Dangers { get; set; } = string.Empty;

    public int? ParentLocationId { get; set; }
    public Location? ParentLocation { get; set; }

    public string ConnectedLocations { get; set; } = string.Empty; // JSON array of location IDs
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<NPC> NPCs { get; set; } = new List<NPC>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Location> SubLocations { get; set; } = new List<Location>();
}

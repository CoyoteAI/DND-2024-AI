using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class Feature : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public FeatureSource Source { get; set; }
    public string SourceName { get; set; } = string.Empty; // e.g., "Fighter", "Human", "Acolyte"

    public int LevelRequirement { get; set; } = 1;

    // Uses tracking
    public int? MaxUsesPerShortRest { get; set; }
    public int? MaxUsesPerLongRest { get; set; }
    public RechargeType RechargeType { get; set; } = RechargeType.None;

    public ICollection<CharacterFeature> CharacterFeatures { get; set; } = new List<CharacterFeature>();
}

namespace DnDAI.Core.Models;

public class CharacterFeature : BaseEntity
{
    public int PlayerCharacterId { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; } = null!;

    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;

    public int? CurrentUses { get; set; } // Null if feature has unlimited uses
}

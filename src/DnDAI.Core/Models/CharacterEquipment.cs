namespace DnDAI.Core.Models;

public class CharacterEquipment : BaseEntity
{
    public int PlayerCharacterId { get; set; }
    public PlayerCharacter PlayerCharacter { get; set; } = null!;

    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public bool IsEquipped { get; set; } // Is it currently equipped/worn?
    public bool IsAttuned { get; set; } // For magic items requiring attunement

    // For items with charges/uses
    public int? CurrentCharges { get; set; }

    // Custom notes for this specific instance
    public string? Notes { get; set; }
}

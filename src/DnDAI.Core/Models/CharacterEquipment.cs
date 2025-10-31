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

    // Container relationship - if this item is stored inside a container
    public int? ContainerId { get; set; }
    public CharacterEquipment? Container { get; set; }

    // Items stored in this container (if this item is a container)
    public ICollection<CharacterEquipment> ContainedItems { get; set; } = new List<CharacterEquipment>();

    // Custom notes for this specific instance
    public string? Notes { get; set; }
}

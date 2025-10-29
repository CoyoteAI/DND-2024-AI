using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class StatusEffect : BaseEntity
{
    public int CombatantId { get; set; }
    public Combatant Combatant { get; set; } = null!;

    public StatusEffectType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? DurationRounds { get; set; } // null = indefinite
    public int RoundsRemaining { get; set; }

    public bool RequiresConcentration { get; set; } = false;
    public string Source { get; set; } = string.Empty; // Who/what applied this effect

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

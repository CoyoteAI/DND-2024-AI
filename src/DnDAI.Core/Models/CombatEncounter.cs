namespace DnDAI.Core.Models;

public class CombatEncounter : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public int? SessionId { get; set; }
    public Session? Session { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }

    public int CurrentRound { get; set; } = 1;
    public int CurrentTurnIndex { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; } = false;

    public string Notes { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;

    // Navigation
    public ICollection<Combatant> Combatants { get; set; } = new List<Combatant>();
}

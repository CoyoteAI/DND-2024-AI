namespace DnDAI.Core.Models;

public class Quest : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objectives { get; set; } = string.Empty;

    public string QuestGiver { get; set; } = string.Empty;
    public int? QuestGiverNPCId { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; } = false;
    public bool IsFailed { get; set; } = false;

    public string Rewards { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public DateTime? CompletedDate { get; set; }
}

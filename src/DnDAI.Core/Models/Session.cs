namespace DnDAI.Core.Models;

public class Session : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;

    public int SessionNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string InGameDateStart { get; set; } = string.Empty;
    public string InGameDateEnd { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<ConversationMessage> ConversationMessages { get; set; } = new List<ConversationMessage>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
}

namespace DnDAI.Core.Models;

public class ConversationMessage : BaseEntity
{
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Speaker { get; set; } = string.Empty; // "DM", "Player", or character name
    public string Message { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Normal"; // Normal, Action, Narration, Dice Roll
}

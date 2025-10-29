namespace DnDAI.Core.Models;

public class DiceRoll
{
    public string Expression { get; set; } = string.Empty; // e.g., "2d6+3"
    public int[] IndividualRolls { get; set; } = Array.Empty<int>();
    public int Modifier { get; set; }
    public int Total { get; set; }
    public string RolledBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Purpose { get; set; } = string.Empty; // e.g., "Attack Roll", "Saving Throw"
}

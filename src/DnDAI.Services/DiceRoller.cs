using System.Text.RegularExpressions;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;

namespace DnDAI.Services;

public class DiceRoller : IDiceRoller
{
    private readonly Random _random;

    public DiceRoller()
    {
        _random = new Random();
    }

    /// <summary>
    /// Rolls dice based on standard notation (e.g., "2d6+3", "1d20", "3d8-2")
    /// </summary>
    public DiceRoll Roll(string expression, string rolledBy = "Player", string purpose = "")
    {
        expression = expression.Trim().ToLower().Replace(" ", "");

        // Parse dice notation: XdY+Z or XdY-Z
        var match = Regex.Match(expression, @"^(\d+)?d(\d+)([+\-]\d+)?$");

        if (!match.Success)
        {
            throw new ArgumentException($"Invalid dice expression: {expression}. Use format like '2d6+3' or '1d20'");
        }

        int numberOfDice = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        int sides = int.Parse(match.Groups[2].Value);
        int modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

        return RollDice(numberOfDice, sides, modifier, rolledBy, purpose);
    }

    /// <summary>
    /// Rolls a single d20 with optional modifier
    /// </summary>
    public DiceRoll RollD20(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        return RollDice(1, 20, modifier, rolledBy, purpose);
    }

    /// <summary>
    /// Rolls specified number of dice with given sides and modifier
    /// </summary>
    public DiceRoll RollDice(int numberOfDice, int sides, int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        if (numberOfDice < 1)
            throw new ArgumentException("Number of dice must be at least 1");

        if (sides < 2)
            throw new ArgumentException("Dice must have at least 2 sides");

        var rolls = new int[numberOfDice];
        int sum = 0;

        for (int i = 0; i < numberOfDice; i++)
        {
            rolls[i] = _random.Next(1, sides + 1);
            sum += rolls[i];
        }

        var total = sum + modifier;

        return new DiceRoll
        {
            Expression = $"{numberOfDice}d{sides}{(modifier >= 0 ? "+" : "")}{modifier}",
            IndividualRolls = rolls,
            Modifier = modifier,
            Total = total,
            RolledBy = rolledBy,
            Purpose = purpose,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Rolls d20 with advantage (roll twice, take higher)
    /// </summary>
    public DiceRoll RollWithAdvantage(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        int roll1 = _random.Next(1, 21);
        int roll2 = _random.Next(1, 21);
        int higher = Math.Max(roll1, roll2);

        return new DiceRoll
        {
            Expression = $"1d20 (Advantage){(modifier >= 0 ? "+" : "")}{modifier}",
            IndividualRolls = new[] { roll1, roll2 },
            Modifier = modifier,
            Total = higher + modifier,
            RolledBy = rolledBy,
            Purpose = purpose,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Rolls d20 with disadvantage (roll twice, take lower)
    /// </summary>
    public DiceRoll RollWithDisadvantage(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        int roll1 = _random.Next(1, 21);
        int roll2 = _random.Next(1, 21);
        int lower = Math.Min(roll1, roll2);

        return new DiceRoll
        {
            Expression = $"1d20 (Disadvantage){(modifier >= 0 ? "+" : "")}{modifier}",
            IndividualRolls = new[] { roll1, roll2 },
            Modifier = modifier,
            Total = lower + modifier,
            RolledBy = rolledBy,
            Purpose = purpose,
            Timestamp = DateTime.UtcNow
        };
    }
}

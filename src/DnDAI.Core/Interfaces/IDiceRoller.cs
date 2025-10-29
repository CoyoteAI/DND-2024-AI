using DnDAI.Core.Models;

namespace DnDAI.Core.Interfaces;

public interface IDiceRoller
{
    DiceRoll Roll(string expression, string rolledBy = "Player", string purpose = "");
    DiceRoll RollD20(int modifier = 0, string rolledBy = "Player", string purpose = "");
    DiceRoll RollDice(int numberOfDice, int sides, int modifier = 0, string rolledBy = "Player", string purpose = "");
    DiceRoll RollWithAdvantage(int modifier = 0, string rolledBy = "Player", string purpose = "");
    DiceRoll RollWithDisadvantage(int modifier = 0, string rolledBy = "Player", string purpose = "");
}

using System.Text.RegularExpressions;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Enums;

namespace DnDAI.Services;

public class CombatCommandParser
{
    private readonly ICombatService _combatService;

    public CombatCommandParser(ICombatService combatService)
    {
        _combatService = combatService;
    }

    public async Task<List<string>> ParseAndExecuteCommandsAsync(string text, int campaignId, int? sessionId = null)
    {
        var executedCommands = new List<string>();

        // Pattern: [COMMAND: params]
        var commandPattern = @"\[([A-Z_]+):\s*([^\]]+)\]";
        var matches = Regex.Matches(text, commandPattern);

        foreach (Match match in matches)
        {
            var command = match.Groups[1].Value;
            var parameters = match.Groups[2].Value;

            try
            {
                var result = await ExecuteCommandAsync(command, parameters, campaignId, sessionId);
                if (!string.IsNullOrEmpty(result))
                {
                    executedCommands.Add(result);
                }
            }
            catch (Exception ex)
            {
                executedCommands.Add($"Error executing {command}: {ex.Message}");
            }
        }

        return executedCommands;
    }

    public string StripCommandsFromText(string text)
    {
        var commandPattern = @"\[([A-Z_]+):\s*([^\]]+)\]";
        return Regex.Replace(text, commandPattern, "").Trim();
    }

    private async Task<string> ExecuteCommandAsync(string command, string parameters, int campaignId, int? sessionId)
    {
        switch (command)
        {
            case "COMBAT_START":
                return await HandleCombatStart(parameters, campaignId, sessionId);

            case "COMBAT_END":
                return await HandleCombatEnd(campaignId);

            case "ADD_MONSTER":
                return await HandleAddMonster(parameters, campaignId);

            case "ADD_PC":
                return await HandleAddPC(parameters, campaignId);

            case "DAMAGE":
                return await HandleDamage(parameters, campaignId);

            case "HEAL":
                return await HandleHeal(parameters, campaignId);

            case "REDUCE_MAX_HP":
                return await HandleReduceMaxHP(parameters, campaignId);

            case "RESTORE_MAX_HP":
                return await HandleRestoreMaxHP(parameters, campaignId);

            case "ROLL_INITIATIVE":
                return await HandleRollInitiative(campaignId);

            case "NEXT_TURN":
                return await HandleNextTurn(campaignId);

            default:
                return $"Unknown command: {command}";
        }
    }

    private async Task<string> HandleCombatStart(string name, int campaignId, int? sessionId)
    {
        // Check if combat already active
        var existingCombat = await _combatService.GetActiveCombatAsync(campaignId);
        if (existingCombat != null)
        {
            return "Combat already active";
        }

        var encounter = await _combatService.StartCombatAsync(campaignId, sessionId, name);
        return $"Combat started: {name}";
    }

    private async Task<string> HandleCombatEnd(int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        await _combatService.EndCombatAsync(encounter.Id, "Combat ended by DM");
        return "Combat ended";
    }

    private async Task<string> HandleAddMonster(string parameters, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        // Parse: Name, HP:X, AC:Y, Init:Z
        var parts = parameters.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length < 2)
        {
            return "Invalid ADD_MONSTER format";
        }

        var name = parts[0];
        int hp = 10;  // defaults
        int ac = 10;
        int initMod = 0;

        foreach (var part in parts.Skip(1))
        {
            if (part.StartsWith("HP:", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(part.Substring(3).Trim(), out hp);
            }
            else if (part.StartsWith("AC:", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(part.Substring(3).Trim(), out ac);
            }
            else if (part.StartsWith("Init:", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(part.Substring(5).Trim(), out initMod);
            }
        }

        await _combatService.AddMonsterToCombatAsync(encounter.Id, name, hp, ac, initMod);
        return $"Added {name} to combat";
    }

    private async Task<string> HandleAddPC(string pcName, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        // Find PC by name
        var campaign = encounter.Campaign;
        var pc = campaign?.PlayerCharacters.FirstOrDefault(p =>
            p.Name.Equals(pcName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (pc == null)
        {
            return $"Player character '{pcName}' not found";
        }

        await _combatService.AddPlayerCharacterToCombatAsync(encounter.Id, pc.Id);
        return $"Added {pc.Name} to combat";
    }

    private async Task<string> HandleDamage(string parameters, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        // Parse: Name, Amount
        var parts = parameters.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length < 2)
        {
            return "Invalid DAMAGE format";
        }

        var name = parts[0];
        if (!int.TryParse(parts[1], out int damage))
        {
            return "Invalid damage amount";
        }

        var combatant = encounter.Combatants.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (combatant == null)
        {
            return $"Combatant '{name}' not found";
        }

        await _combatService.ApplyDamageAsync(combatant.Id, damage);
        return $"{name} takes {damage} damage";
    }

    private async Task<string> HandleHeal(string parameters, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        var parts = parameters.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length < 2)
        {
            return "Invalid HEAL format";
        }

        var name = parts[0];
        if (!int.TryParse(parts[1], out int healing))
        {
            return "Invalid healing amount";
        }

        var combatant = encounter.Combatants.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (combatant == null)
        {
            return $"Combatant '{name}' not found";
        }

        await _combatService.ApplyHealingAsync(combatant.Id, healing);
        return $"{name} heals {healing} HP";
    }

    private async Task<string> HandleReduceMaxHP(string parameters, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        var parts = parameters.Split(',').Select(p => p.Trim()).ToArray();
        if (parts.Length < 2)
        {
            return "Invalid REDUCE_MAX_HP format";
        }

        var name = parts[0];
        if (!int.TryParse(parts[1], out int reduction))
        {
            return "Invalid reduction amount";
        }

        var combatant = encounter.Combatants.FirstOrDefault(c =>
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (combatant == null)
        {
            return $"Combatant '{name}' not found";
        }

        await _combatService.ReduceMaxHPAsync(combatant.Id, reduction);
        return $"{name}'s max HP reduced by {reduction}";
    }

    private async Task<string> HandleRestoreMaxHP(string name, int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        var combatant = encounter.Combatants.FirstOrDefault(c =>
            c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (combatant == null)
        {
            return $"Combatant '{name}' not found";
        }

        await _combatService.RestoreMaxHPAsync(combatant.Id);
        return $"{name}'s max HP restored";
    }

    private async Task<string> HandleRollInitiative(int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        await _combatService.RollInitiativeForAllAsync(encounter.Id);
        return "Initiative rolled for all combatants";
    }

    private async Task<string> HandleNextTurn(int campaignId)
    {
        var encounter = await _combatService.GetActiveCombatAsync(campaignId);
        if (encounter == null)
        {
            return "No active combat";
        }

        await _combatService.NextTurnAsync(encounter.Id);
        return "Advanced to next turn";
    }
}

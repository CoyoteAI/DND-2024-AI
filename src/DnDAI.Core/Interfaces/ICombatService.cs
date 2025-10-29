using DnDAI.Core.Models;

namespace DnDAI.Core.Interfaces;

public interface ICombatService
{
    // Encounter management
    Task<CombatEncounter> StartCombatAsync(int campaignId, int? sessionId, string name, string description = "");
    Task<CombatEncounter> EndCombatAsync(int encounterId, string outcome = "");
    Task<CombatEncounter?> GetActiveCombatAsync(int campaignId);
    Task<CombatEncounter?> GetCombatEncounterAsync(int encounterId);

    // Combatant management
    Task<Combatant> AddCombatantAsync(int encounterId, Combatant combatant);
    Task<Combatant> AddPlayerCharacterToCombatAsync(int encounterId, int playerCharacterId, int? initiativeRoll = null);
    Task<Combatant> AddNPCToCombatAsync(int encounterId, int npcId, int? initiativeRoll = null);
    Task<Combatant> AddMonsterToCombatAsync(int encounterId, string name, int hp, int ac, int initiativeModifier = 0);
    Task<bool> RemoveCombatantAsync(int combatantId);

    // Initiative
    Task RollInitiativeForAllAsync(int encounterId);
    Task SetInitiativeAsync(int combatantId, int initiative);
    Task<List<Combatant>> GetInitiativeOrderAsync(int encounterId);

    // Turn management
    Task<Combatant?> GetCurrentTurnCombatantAsync(int encounterId);
    Task NextTurnAsync(int encounterId);
    Task NextRoundAsync(int encounterId);

    // HP management
    Task ApplyDamageAsync(int combatantId, int damage);
    Task ApplyHealingAsync(int combatantId, int healing);
    Task ReduceMaxHPAsync(int combatantId, int reduction);
    Task RestoreMaxHPAsync(int combatantId);

    // Status effects
    Task<StatusEffect> AddStatusEffectAsync(int combatantId, StatusEffect effect);
    Task RemoveStatusEffectAsync(int effectId);
    Task UpdateStatusEffectDurationsAsync(int encounterId);
}

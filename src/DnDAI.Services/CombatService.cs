using DnDAI.Core.Enums;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDAI.Services;

public class CombatService : ICombatService
{
    private readonly DnDContext _context;
    private readonly IDiceRoller _diceRoller;

    public CombatService(DnDContext context, IDiceRoller diceRoller)
    {
        _context = context;
        _diceRoller = diceRoller;
    }

    // Encounter Management
    public async Task<CombatEncounter> StartCombatAsync(int campaignId, int? sessionId, string name, string description = "")
    {
        var encounter = new CombatEncounter
        {
            CampaignId = campaignId,
            SessionId = sessionId,
            Name = name,
            Description = description,
            StartTime = DateTime.UtcNow,
            CurrentRound = 1,
            CurrentTurnIndex = 0,
            IsActive = true,
            IsCompleted = false
        };

        _context.CombatEncounters.Add(encounter);
        await _context.SaveChangesAsync();
        return encounter;
    }

    public async Task<CombatEncounter> EndCombatAsync(int encounterId, string outcome = "")
    {
        var encounter = await _context.CombatEncounters.FindAsync(encounterId);
        if (encounter == null)
            throw new ArgumentException("Combat encounter not found");

        encounter.IsActive = false;
        encounter.IsCompleted = true;
        encounter.EndTime = DateTime.UtcNow;
        encounter.Outcome = outcome;

        await _context.SaveChangesAsync();
        return encounter;
    }

    public async Task<CombatEncounter?> GetActiveCombatAsync(int campaignId)
    {
        return await _context.CombatEncounters
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.StatusEffects)
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.PlayerCharacter)
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.NPC)
            .FirstOrDefaultAsync(ce => ce.CampaignId == campaignId && ce.IsActive);
    }

    public async Task<CombatEncounter?> GetCombatEncounterAsync(int encounterId)
    {
        return await _context.CombatEncounters
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.StatusEffects)
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.PlayerCharacter)
            .Include(ce => ce.Combatants)
                .ThenInclude(c => c.NPC)
            .FirstOrDefaultAsync(ce => ce.Id == encounterId);
    }

    // Combatant Management
    public async Task<Combatant> AddCombatantAsync(int encounterId, Combatant combatant)
    {
        combatant.CombatEncounterId = encounterId;
        _context.Combatants.Add(combatant);
        await _context.SaveChangesAsync();
        return combatant;
    }

    public async Task<Combatant> AddPlayerCharacterToCombatAsync(int encounterId, int playerCharacterId, int? initiativeRoll = null)
    {
        var pc = await _context.PlayerCharacters.FindAsync(playerCharacterId);
        if (pc == null)
            throw new ArgumentException("Player character not found");

        var combatant = new Combatant
        {
            CombatEncounterId = encounterId,
            PlayerCharacterId = playerCharacterId,
            Type = CombatantType.PlayerCharacter,
            Name = pc.Name,
            CurrentHP = pc.CurrentHitPoints,
            CurrentMaxHP = pc.MaxHitPoints,
            OriginalMaxHP = pc.MaxHitPoints,
            InitiativeModifier = CalculateDexterityModifier(pc.Dexterity),
            Initiative = initiativeRoll ?? _diceRoller.RollD20(CalculateDexterityModifier(pc.Dexterity)).Total
        };

        return await AddCombatantAsync(encounterId, combatant);
    }

    public async Task<Combatant> AddNPCToCombatAsync(int encounterId, int npcId, int? initiativeRoll = null)
    {
        var npc = await _context.NPCs.FindAsync(npcId);
        if (npc == null)
            throw new ArgumentException("NPC not found");

        var maxHP = CalculateMaxHP(npc.Level, npc.Constitution);
        var combatant = new Combatant
        {
            CombatEncounterId = encounterId,
            NPCId = npcId,
            Type = CombatantType.NPC,
            Name = npc.Name,
            CurrentHP = maxHP,
            CurrentMaxHP = maxHP,
            OriginalMaxHP = maxHP,
            InitiativeModifier = CalculateDexterityModifier(npc.Dexterity),
            Initiative = initiativeRoll ?? _diceRoller.RollD20(CalculateDexterityModifier(npc.Dexterity)).Total
        };

        return await AddCombatantAsync(encounterId, combatant);
    }

    public async Task<Combatant> AddMonsterToCombatAsync(int encounterId, string name, int hp, int ac, int initiativeModifier = 0)
    {
        var combatant = new Combatant
        {
            CombatEncounterId = encounterId,
            Type = CombatantType.Monster,
            Name = name,
            CurrentHP = hp,
            CurrentMaxHP = hp,
            OriginalMaxHP = hp,
            ArmorClass = ac,
            InitiativeModifier = initiativeModifier,
            Initiative = _diceRoller.RollD20(initiativeModifier).Total
        };

        return await AddCombatantAsync(encounterId, combatant);
    }

    public async Task<bool> RemoveCombatantAsync(int combatantId)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            return false;

        _context.Combatants.Remove(combatant);
        await _context.SaveChangesAsync();
        return true;
    }

    // Initiative Management
    public async Task RollInitiativeForAllAsync(int encounterId)
    {
        var combatants = await _context.Combatants
            .Where(c => c.CombatEncounterId == encounterId)
            .ToListAsync();

        foreach (var combatant in combatants)
        {
            combatant.Initiative = _diceRoller.RollD20(combatant.InitiativeModifier).Total;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetInitiativeAsync(int combatantId, int initiative)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.Initiative = initiative;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Combatant>> GetInitiativeOrderAsync(int encounterId)
    {
        return await _context.Combatants
            .Where(c => c.CombatEncounterId == encounterId && !c.IsDead)
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.InitiativeModifier)
            .ToListAsync();
    }

    // Turn Management
    public async Task<Combatant?> GetCurrentTurnCombatantAsync(int encounterId)
    {
        var encounter = await GetCombatEncounterAsync(encounterId);
        if (encounter == null)
            return null;

        var initiativeOrder = encounter.Combatants
            .Where(c => !c.IsDead)
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.InitiativeModifier)
            .ToList();

        if (!initiativeOrder.Any() || encounter.CurrentTurnIndex >= initiativeOrder.Count)
            return null;

        return initiativeOrder[encounter.CurrentTurnIndex];
    }

    public async Task NextTurnAsync(int encounterId)
    {
        var encounter = await GetCombatEncounterAsync(encounterId);
        if (encounter == null)
            throw new ArgumentException("Combat encounter not found");

        var currentCombatant = await GetCurrentTurnCombatantAsync(encounterId);
        if (currentCombatant != null)
        {
            currentCombatant.ResetTurn();
        }

        var initiativeOrder = encounter.Combatants
            .Where(c => !c.IsDead)
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.InitiativeModifier)
            .ToList();

        encounter.CurrentTurnIndex++;

        if (encounter.CurrentTurnIndex >= initiativeOrder.Count)
        {
            await NextRoundAsync(encounterId);
        }
        else
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task NextRoundAsync(int encounterId)
    {
        var encounter = await GetCombatEncounterAsync(encounterId);
        if (encounter == null)
            throw new ArgumentException("Combat encounter not found");

        encounter.CurrentRound++;
        encounter.CurrentTurnIndex = 0;

        // Update status effect durations
        await UpdateStatusEffectDurationsAsync(encounterId);

        await _context.SaveChangesAsync();
    }

    // HP Management
    public async Task ApplyDamageAsync(int combatantId, int damage)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.TakeDamage(damage);

        if (combatant.CurrentHP == 0)
        {
            combatant.IsDead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ApplyHealingAsync(int combatantId, int healing)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.Heal(healing);
        await _context.SaveChangesAsync();
    }

    public async Task AddTempHPAsync(int combatantId, int tempHP)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.AddTempHP(tempHP);
        await _context.SaveChangesAsync();
    }

    public async Task ReduceMaxHPAsync(int combatantId, int reduction)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.ReduceMaxHP(reduction);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreMaxHPAsync(int combatantId)
    {
        var combatant = await _context.Combatants.FindAsync(combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.RestoreMaxHP();
        await _context.SaveChangesAsync();
    }

    // Status Effects
    public async Task<StatusEffect> AddStatusEffectAsync(int combatantId, StatusEffect effect)
    {
        effect.CombatantId = combatantId;
        effect.AppliedAt = DateTime.UtcNow;
        effect.RoundsRemaining = effect.DurationRounds ?? int.MaxValue;

        _context.StatusEffects.Add(effect);
        await _context.SaveChangesAsync();
        return effect;
    }

    public async Task RemoveStatusEffectAsync(int effectId)
    {
        var effect = await _context.StatusEffects.FindAsync(effectId);
        if (effect != null)
        {
            effect.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateStatusEffectDurationsAsync(int encounterId)
    {
        var combatants = await _context.Combatants
            .Include(c => c.StatusEffects)
            .Where(c => c.CombatEncounterId == encounterId)
            .ToListAsync();

        foreach (var combatant in combatants)
        {
            foreach (var effect in combatant.StatusEffects.Where(e => e.IsActive))
            {
                if (effect.DurationRounds.HasValue)
                {
                    effect.RoundsRemaining--;
                    if (effect.RoundsRemaining <= 0)
                    {
                        effect.IsActive = false;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    // Helper Methods
    private int CalculateDexterityModifier(int dexterity)
    {
        return (dexterity - 10) / 2;
    }

    private int CalculateMaxHP(int level, int constitution)
    {
        int conModifier = (constitution - 10) / 2;
        return (10 + conModifier) + ((level - 1) * (6 + conModifier)); // Simplified HP calculation
    }
}

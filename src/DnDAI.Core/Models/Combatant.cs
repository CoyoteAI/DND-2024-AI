using DnDAI.Core.Enums;

namespace DnDAI.Core.Models;

public class Combatant : BaseEntity
{
    public int CombatEncounterId { get; set; }
    public CombatEncounter CombatEncounter { get; set; } = null!;

    public CombatantType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    // Initiative
    public int Initiative { get; set; }
    public int InitiativeModifier { get; set; } = 0;

    // HP Tracking with support for max HP reduction (e.g., undead attacks)
    public int CurrentHP { get; set; }
    public int CurrentMaxHP { get; set; }  // Can be reduced by effects
    public int OriginalMaxHP { get; set; } // Original max, restored when effects end
    public int TempHP { get; set; } = 0;   // Temporary hit points (lost first, can't be healed)

    // Armor Class
    public int ArmorClass { get; set; } = 10;

    // References to existing entities
    public int? PlayerCharacterId { get; set; }
    public PlayerCharacter? PlayerCharacter { get; set; }

    public int? NPCId { get; set; }
    public NPC? NPC { get; set; }

    // Turn tracking
    public bool HasTakenAction { get; set; } = false;
    public bool HasTakenBonusAction { get; set; } = false;
    public bool HasTakenReaction { get; set; } = false;
    public int MovementRemaining { get; set; } = 30; // In feet

    // Status
    public bool IsConcentrating { get; set; } = false;
    public string ConcentrationSpell { get; set; } = string.Empty;
    public bool IsDead { get; set; } = false;
    public bool IsUnconscious { get; set; } = false;

    public string Notes { get; set; } = string.Empty;

    // Navigation
    public ICollection<StatusEffect> StatusEffects { get; set; } = new List<StatusEffect>();

    // Helper methods
    public void TakeDamage(int damage)
    {
        // Temp HP is lost first
        if (TempHP > 0)
        {
            if (damage <= TempHP)
            {
                TempHP -= damage;
                return; // All damage absorbed by temp HP
            }
            else
            {
                damage -= TempHP;
                TempHP = 0;
            }
        }

        // Apply remaining damage to regular HP
        CurrentHP = Math.Max(0, CurrentHP - damage);
        if (CurrentHP == 0)
        {
            IsUnconscious = true;
        }
    }

    public void AddTempHP(int tempHP)
    {
        // Temp HP doesn't stack - take the higher value
        TempHP = Math.Max(TempHP, tempHP);
    }

    public void Heal(int healing)
    {
        CurrentHP = Math.Min(CurrentMaxHP, CurrentHP + healing);
        if (CurrentHP > 0)
        {
            IsUnconscious = false;
        }
    }

    public void ReduceMaxHP(int reduction)
    {
        CurrentMaxHP = Math.Max(1, CurrentMaxHP - reduction);
        // Ensure current HP doesn't exceed new max
        CurrentHP = Math.Min(CurrentHP, CurrentMaxHP);
    }

    public void RestoreMaxHP()
    {
        CurrentMaxHP = OriginalMaxHP;
        // Current HP remains the same unless it exceeds the new max
        CurrentHP = Math.Min(CurrentHP, CurrentMaxHP);
    }

    public void ResetTurn()
    {
        HasTakenAction = false;
        HasTakenBonusAction = false;
        HasTakenReaction = false;
        MovementRemaining = 30; // Default movement
    }
}

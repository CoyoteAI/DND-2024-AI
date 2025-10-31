using DnDAI.Core.Enums;

namespace DnDAI.Core.Helpers;

public static class SpellSlotHelper
{
    // Spell slot progression tables for D&D 5e (2024)
    // Index 0 = Level 1, Index 19 = Level 20
    // Each inner array has 9 elements for spell levels 1-9

    private static readonly int[,] FullCasterSlots = new int[,]
    {
        // Lvl: 1st, 2nd, 3rd, 4th, 5th, 6th, 7th, 8th, 9th
        /*  1 */ { 2,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  2 */ { 3,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  3 */ { 4,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  4 */ { 4,  3,  0,  0,  0,  0,  0,  0,  0 },
        /*  5 */ { 4,  3,  2,  0,  0,  0,  0,  0,  0 },
        /*  6 */ { 4,  3,  3,  0,  0,  0,  0,  0,  0 },
        /*  7 */ { 4,  3,  3,  1,  0,  0,  0,  0,  0 },
        /*  8 */ { 4,  3,  3,  2,  0,  0,  0,  0,  0 },
        /*  9 */ { 4,  3,  3,  3,  1,  0,  0,  0,  0 },
        /* 10 */ { 4,  3,  3,  3,  2,  0,  0,  0,  0 },
        /* 11 */ { 4,  3,  3,  3,  2,  1,  0,  0,  0 },
        /* 12 */ { 4,  3,  3,  3,  2,  1,  0,  0,  0 },
        /* 13 */ { 4,  3,  3,  3,  2,  1,  1,  0,  0 },
        /* 14 */ { 4,  3,  3,  3,  2,  1,  1,  0,  0 },
        /* 15 */ { 4,  3,  3,  3,  2,  1,  1,  1,  0 },
        /* 16 */ { 4,  3,  3,  3,  2,  1,  1,  1,  0 },
        /* 17 */ { 4,  3,  3,  3,  2,  1,  1,  1,  1 },
        /* 18 */ { 4,  3,  3,  3,  3,  1,  1,  1,  1 },
        /* 19 */ { 4,  3,  3,  3,  3,  2,  1,  1,  1 },
        /* 20 */ { 4,  3,  3,  3,  3,  2,  2,  1,  1 }
    };

    private static readonly int[,] HalfCasterSlots = new int[,]
    {
        // Lvl: 1st, 2nd, 3rd, 4th, 5th, 6th, 7th, 8th, 9th
        /*  1 */ { 0,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  2 */ { 2,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  3 */ { 3,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  4 */ { 3,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  5 */ { 4,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  6 */ { 4,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  7 */ { 4,  3,  0,  0,  0,  0,  0,  0,  0 },
        /*  8 */ { 4,  3,  0,  0,  0,  0,  0,  0,  0 },
        /*  9 */ { 4,  3,  2,  0,  0,  0,  0,  0,  0 },
        /* 10 */ { 4,  3,  2,  0,  0,  0,  0,  0,  0 },
        /* 11 */ { 4,  3,  3,  0,  0,  0,  0,  0,  0 },
        /* 12 */ { 4,  3,  3,  0,  0,  0,  0,  0,  0 },
        /* 13 */ { 4,  3,  3,  1,  0,  0,  0,  0,  0 },
        /* 14 */ { 4,  3,  3,  1,  0,  0,  0,  0,  0 },
        /* 15 */ { 4,  3,  3,  2,  0,  0,  0,  0,  0 },
        /* 16 */ { 4,  3,  3,  2,  0,  0,  0,  0,  0 },
        /* 17 */ { 4,  3,  3,  3,  1,  0,  0,  0,  0 },
        /* 18 */ { 4,  3,  3,  3,  1,  0,  0,  0,  0 },
        /* 19 */ { 4,  3,  3,  3,  2,  0,  0,  0,  0 },
        /* 20 */ { 4,  3,  3,  3,  2,  0,  0,  0,  0 }
    };

    private static readonly int[,] ArtificerSlots = new int[,]
    {
        // Artificer uses its own progression
        // Lvl: 1st, 2nd, 3rd, 4th, 5th, 6th, 7th, 8th, 9th
        /*  1 */ { 0,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  2 */ { 2,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  3 */ { 3,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  4 */ { 3,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  5 */ { 4,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  6 */ { 4,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  7 */ { 4,  3,  0,  0,  0,  0,  0,  0,  0 },
        /*  8 */ { 4,  3,  0,  0,  0,  0,  0,  0,  0 },
        /*  9 */ { 4,  3,  2,  0,  0,  0,  0,  0,  0 },
        /* 10 */ { 4,  3,  2,  0,  0,  0,  0,  0,  0 },
        /* 11 */ { 4,  3,  3,  0,  0,  0,  0,  0,  0 },
        /* 12 */ { 4,  3,  3,  0,  0,  0,  0,  0,  0 },
        /* 13 */ { 4,  3,  3,  1,  0,  0,  0,  0,  0 },
        /* 14 */ { 4,  3,  3,  1,  0,  0,  0,  0,  0 },
        /* 15 */ { 4,  3,  3,  2,  0,  0,  0,  0,  0 },
        /* 16 */ { 4,  3,  3,  2,  0,  0,  0,  0,  0 },
        /* 17 */ { 4,  3,  3,  3,  1,  0,  0,  0,  0 },
        /* 18 */ { 4,  3,  3,  3,  1,  0,  0,  0,  0 },
        /* 19 */ { 4,  3,  3,  3,  2,  0,  0,  0,  0 },
        /* 20 */ { 4,  3,  3,  3,  2,  0,  0,  0,  0 }
    };

    private static readonly int[,] WarlockSlots = new int[,]
    {
        // Warlock uses Pact Magic (different progression)
        // Lvl: 1st, 2nd, 3rd, 4th, 5th, 6th, 7th, 8th, 9th
        /*  1 */ { 1,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  2 */ { 2,  0,  0,  0,  0,  0,  0,  0,  0 },
        /*  3 */ { 0,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  4 */ { 0,  2,  0,  0,  0,  0,  0,  0,  0 },
        /*  5 */ { 0,  0,  2,  0,  0,  0,  0,  0,  0 },
        /*  6 */ { 0,  0,  2,  0,  0,  0,  0,  0,  0 },
        /*  7 */ { 0,  0,  0,  2,  0,  0,  0,  0,  0 },
        /*  8 */ { 0,  0,  0,  2,  0,  0,  0,  0,  0 },
        /*  9 */ { 0,  0,  0,  0,  2,  0,  0,  0,  0 },
        /* 10 */ { 0,  0,  0,  0,  2,  0,  0,  0,  0 },
        /* 11 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 12 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 13 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 14 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 15 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 16 */ { 0,  0,  0,  0,  3,  0,  0,  0,  0 },
        /* 17 */ { 0,  0,  0,  0,  4,  0,  0,  0,  0 },
        /* 18 */ { 0,  0,  0,  0,  4,  0,  0,  0,  0 },
        /* 19 */ { 0,  0,  0,  0,  4,  0,  0,  0,  0 },
        /* 20 */ { 0,  0,  0,  0,  4,  0,  0,  0,  0 }
    };

    /// <summary>
    /// Gets the spell slots for a given class and level.
    /// Returns an array of 9 integers representing spell slots for levels 1-9.
    /// </summary>
    public static int[] GetSpellSlotsForClassAndLevel(CharacterClass characterClass, int level)
    {
        if (level < 1 || level > 20)
            return new int[9]; // Return empty array for invalid levels

        int[] slots = new int[9];
        int levelIndex = level - 1; // Convert to 0-based index

        switch (characterClass)
        {
            case CharacterClass.Bard:
            case CharacterClass.Cleric:
            case CharacterClass.Druid:
            case CharacterClass.Sorcerer:
            case CharacterClass.Wizard:
                // Full casters
                for (int i = 0; i < 9; i++)
                    slots[i] = FullCasterSlots[levelIndex, i];
                break;

            case CharacterClass.Paladin:
            case CharacterClass.Ranger:
                // Half casters
                for (int i = 0; i < 9; i++)
                    slots[i] = HalfCasterSlots[levelIndex, i];
                break;

            case CharacterClass.Artificer:
                // Artificer (half caster with unique progression)
                for (int i = 0; i < 9; i++)
                    slots[i] = ArtificerSlots[levelIndex, i];
                break;

            case CharacterClass.Warlock:
                // Warlock (Pact Magic)
                for (int i = 0; i < 9; i++)
                    slots[i] = WarlockSlots[levelIndex, i];
                break;

            case CharacterClass.Fighter:
            case CharacterClass.Rogue:
                // These classes can be spellcasters via subclass (Eldritch Knight, Arcane Trickster)
                // They use third-caster progression, but we'll handle them separately
                // For now, return empty (subclass-based spellcasting not auto-calculated)
                break;

            case CharacterClass.Barbarian:
            case CharacterClass.Monk:
                // Non-spellcasters
                break;
        }

        return slots;
    }

    /// <summary>
    /// Checks if a class is a spellcaster.
    /// </summary>
    public static bool IsSpellcaster(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Bard => true,
            CharacterClass.Cleric => true,
            CharacterClass.Druid => true,
            CharacterClass.Sorcerer => true,
            CharacterClass.Wizard => true,
            CharacterClass.Paladin => true,
            CharacterClass.Ranger => true,
            CharacterClass.Artificer => true,
            CharacterClass.Warlock => true,
            CharacterClass.Fighter => false, // Can be via subclass
            CharacterClass.Rogue => false,   // Can be via subclass
            CharacterClass.Barbarian => false,
            CharacterClass.Monk => false,
            _ => false
        };
    }

    /// <summary>
    /// Gets the spellcasting ability for a class.
    /// </summary>
    public static string GetSpellcastingAbility(CharacterClass characterClass)
    {
        return characterClass switch
        {
            CharacterClass.Bard => "Charisma",
            CharacterClass.Cleric => "Wisdom",
            CharacterClass.Druid => "Wisdom",
            CharacterClass.Sorcerer => "Charisma",
            CharacterClass.Wizard => "Intelligence",
            CharacterClass.Paladin => "Charisma",
            CharacterClass.Ranger => "Wisdom",
            CharacterClass.Artificer => "Intelligence",
            CharacterClass.Warlock => "Charisma",
            CharacterClass.Fighter => "Intelligence", // Eldritch Knight
            CharacterClass.Rogue => "Intelligence",   // Arcane Trickster
            _ => "None"
        };
    }
}

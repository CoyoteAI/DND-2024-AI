using DnDAI.Core.Enums;
using DnDAI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DnDAI.Data.Seeding;

public static class FeatureSeeder
{
    public static void SeedFeatures(DnDContext context)
    {
        // Check if features already exist
        if (context.Features.Any())
        {
            return; // Already seeded
        }

        var features = new List<Feature>();

        // Add all class features
        features.AddRange(GetBarbarianFeatures());
        features.AddRange(GetBardFeatures());
        features.AddRange(GetClericFeatures());
        features.AddRange(GetDruidFeatures());
        features.AddRange(GetFighterFeatures());
        features.AddRange(GetMonkFeatures());
        features.AddRange(GetPaladinFeatures());
        features.AddRange(GetRangerFeatures());
        features.AddRange(GetRogueFeatures());
        features.AddRange(GetSorcererFeatures());
        features.AddRange(GetWarlockFeatures());
        features.AddRange(GetWizardFeatures());
        features.AddRange(GetArtificerFeatures());

        // Add species features
        features.AddRange(GetSpeciesFeatures());

        // Add background features
        features.AddRange(GetBackgroundFeatures());

        context.Features.AddRange(features);
        context.SaveChanges();
    }

    private static List<Feature> GetBarbarianFeatures()
    {
        return new List<Feature>
        {
            new Feature
            {
                Name = "Rage",
                Description = "Enter a rage as a bonus action. While raging, you gain advantage on STR checks and saves, +2 bonus to melee damage with STR weapons, and resistance to physical damage. Lasts 1 minute or until you end it.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 1,
                MaxUsesPerLongRest = 2,
                RechargeType = RechargeType.LongRest
            },
            new Feature
            {
                Name = "Unarmored Defense",
                Description = "While not wearing armor, your AC equals 10 + DEX modifier + CON modifier.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 1,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Danger Sense",
                Description = "You have advantage on DEX saving throws against effects you can see, such as traps and spells.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 2,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Reckless Attack",
                Description = "When you make your first attack on your turn, you can decide to attack recklessly. Doing so gives you advantage on melee weapon attack rolls using STR during this turn, but attack rolls against you have advantage until your next turn.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 2,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Primal Path",
                Description = "Choose a Primal Path that shapes your rage.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 3,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Extra Attack",
                Description = "You can attack twice whenever you take the Attack action on your turn.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 5,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Fast Movement",
                Description = "Your speed increases by 10 feet while you aren't wearing heavy armor.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 5,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Feral Instinct",
                Description = "You have advantage on initiative rolls. Additionally, if you are surprised at the start of combat and aren't incapacitated, you can act normally on your first turn if you enter your rage.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 7,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Brutal Critical",
                Description = "You can roll one additional weapon damage die when determining the extra damage for a critical hit with a melee attack.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 9,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Relentless Rage",
                Description = "If you drop to 0 HP while raging and don't die outright, you can make a DC 10 CON save to drop to 1 HP instead. The DC increases by 5 for each use and resets after a short or long rest.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 11,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Persistent Rage",
                Description = "Your rage only ends early if you fall unconscious or if you choose to end it.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 15,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Indomitable Might",
                Description = "If your total for a STR check is less than your STR score, you can use your STR score in place of the total.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 18,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Primal Champion",
                Description = "Your STR and CON scores increase by 4. Your maximum for those scores is now 24.",
                Source = FeatureSource.Class,
                SourceName = "Barbarian",
                LevelRequirement = 20,
                RechargeType = RechargeType.None
            }
        };
    }

    private static List<Feature> GetBardFeatures()
    {
        return new List<Feature>
        {
            new Feature
            {
                Name = "Bardic Inspiration",
                Description = "As a bonus action, give an ally within 60 feet an inspiration die (d6). Within the next 10 minutes, they can add it to one ability check, attack roll, or saving throw.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 1,
                MaxUsesPerLongRest = null, // Uses CHA modifier
                RechargeType = RechargeType.LongRest
            },
            new Feature
            {
                Name = "Jack of All Trades",
                Description = "Add half your proficiency bonus (rounded down) to any ability check you make that doesn't already include your proficiency bonus.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 2,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Song of Rest",
                Description = "During a short rest, you and any allies who hear your performance regain extra hit points equal to your bardic inspiration die.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 2,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Bard College",
                Description = "Choose a Bard College that reflects your style of performance and magic.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 3,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Expertise",
                Description = "Choose two skills you're proficient in. Your proficiency bonus is doubled for any ability check you make with them.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 3,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Font of Inspiration",
                Description = "You regain all expended uses of Bardic Inspiration when you finish a short or long rest.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 5,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Countercharm",
                Description = "As an action, you can perform until the end of your next turn. Allies within 30 feet have advantage on saves against being frightened or charmed.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 6,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Magical Secrets",
                Description = "Choose two spells from any class. They count as bard spells for you.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 10,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Superior Inspiration",
                Description = "When you roll initiative and have no uses of Bardic Inspiration left, you regain one use.",
                Source = FeatureSource.Class,
                SourceName = "Bard",
                LevelRequirement = 20,
                RechargeType = RechargeType.None
            }
        };
    }

    private static List<Feature> GetClericFeatures()
    {
        return new List<Feature>
        {
            new Feature
            {
                Name = "Divine Domain",
                Description = "Choose a Divine Domain that represents your deity's sphere of influence.",
                Source = FeatureSource.Class,
                SourceName = "Cleric",
                LevelRequirement = 1,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Channel Divinity",
                Description = "You can channel divine energy to fuel magical effects. You start with Turn Undead and your domain grants additional effects.",
                Source = FeatureSource.Class,
                SourceName = "Cleric",
                LevelRequirement = 2,
                MaxUsesPerShortRest = 1,
                RechargeType = RechargeType.ShortRest
            },
            new Feature
            {
                Name = "Turn Undead",
                Description = "As an action, present your holy symbol and speak a prayer censuring the undead. Each undead within 30 feet that can see or hear you must make a WIS save or be turned for 1 minute.",
                Source = FeatureSource.Class,
                SourceName = "Cleric",
                LevelRequirement = 2,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Destroy Undead",
                Description = "When an undead fails its save against your Turn Undead, it is instantly destroyed if its CR is low enough (CR 1/2 at level 5, increases with level).",
                Source = FeatureSource.Class,
                SourceName = "Cleric",
                LevelRequirement = 5,
                RechargeType = RechargeType.None
            },
            new Feature
            {
                Name = "Divine Intervention",
                Description = "You can call on your deity to intervene. Roll d100; if you roll equal to or less than your cleric level, the intervention succeeds. If it fails, you can't use this again for 7 days. If it succeeds, you can't use it again for 7 days.",
                Source = FeatureSource.Class,
                SourceName = "Cleric",
                LevelRequirement = 10,
                RechargeType = RechargeType.Other
            }
        };
    }

    // I'll create a compressed version with key features for remaining classes to save space
    private static List<Feature> GetDruidFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Wild Shape", Description = "Transform into a beast as an action. You can use this feature twice and regain expended uses after a short or long rest.", Source = FeatureSource.Class, SourceName = "Druid", LevelRequirement = 2, MaxUsesPerShortRest = 2, RechargeType = RechargeType.ShortRest },
            new Feature { Name = "Druid Circle", Description = "Choose a Druid Circle that reflects your connection to nature.", Source = FeatureSource.Class, SourceName = "Druid", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Timeless Body", Description = "For every 10 years that pass, your body ages only 1 year.", Source = FeatureSource.Class, SourceName = "Druid", LevelRequirement = 18, RechargeType = RechargeType.None },
            new Feature { Name = "Beast Spells", Description = "You can cast spells in beast form.", Source = FeatureSource.Class, SourceName = "Druid", LevelRequirement = 18, RechargeType = RechargeType.None },
            new Feature { Name = "Archdruid", Description = "You can use Wild Shape an unlimited number of times. Additionally, you ignore verbal and somatic components of druid spells.", Source = FeatureSource.Class, SourceName = "Druid", LevelRequirement = 20, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetFighterFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Fighting Style", Description = "Choose a fighting style that reflects your combat training.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Second Wind", Description = "On your turn, you can use a bonus action to regain HP equal to 1d10 + your fighter level.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 1, MaxUsesPerShortRest = 1, RechargeType = RechargeType.ShortRest },
            new Feature { Name = "Action Surge", Description = "You can take one additional action on your turn. You can use this feature once and regain the ability after a short or long rest.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 2, MaxUsesPerShortRest = 1, RechargeType = RechargeType.ShortRest },
            new Feature { Name = "Martial Archetype", Description = "Choose a Martial Archetype that specializes your combat abilities.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Extra Attack", Description = "You can attack twice whenever you take the Attack action. At 11th level, you can attack three times. At 20th level, you can attack four times.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 5, RechargeType = RechargeType.None },
            new Feature { Name = "Indomitable", Description = "You can reroll a saving throw you fail. You must use the new roll. You can use this feature once, and regain the ability after a long rest.", Source = FeatureSource.Class, SourceName = "Fighter", LevelRequirement = 9, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest }
        };
    }

    private static List<Feature> GetMonkFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Unarmored Defense", Description = "While not wearing armor or wielding a shield, your AC equals 10 + DEX modifier + WIS modifier.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Martial Arts", Description = "You can use DEX instead of STR for attack and damage rolls of unarmed strikes and monk weapons. You can roll a d4 for damage instead of normal damage. When you use Attack action with unarmed strike or monk weapon, you can make one unarmed strike as a bonus action.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Ki", Description = "You have a pool of ki points equal to your monk level. You can spend ki points to fuel various ki features. You regain all spent ki points after a short or long rest.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 2, RechargeType = RechargeType.ShortRest },
            new Feature { Name = "Flurry of Blows", Description = "After taking the Attack action, spend 1 ki point to make two unarmed strikes as a bonus action.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Patient Defense", Description = "Spend 1 ki point to take the Dodge action as a bonus action.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Step of the Wind", Description = "Spend 1 ki point to take Disengage or Dash as a bonus action, and your jump distance is doubled.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Monastic Tradition", Description = "Choose a Monastic Tradition that shapes your martial arts practice.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Deflect Missiles", Description = "You can use your reaction to deflect or catch a missile when hit by a ranged weapon attack. Reduce damage by 1d10 + DEX modifier + monk level. If reduced to 0, you catch it if it's small enough.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Extra Attack", Description = "You can attack twice whenever you take the Attack action.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 5, RechargeType = RechargeType.None },
            new Feature { Name = "Stunning Strike", Description = "When you hit with a melee weapon attack, spend 1 ki point to force the target to make a CON save or be stunned until the end of your next turn.", Source = FeatureSource.Class, SourceName = "Monk", LevelRequirement = 5, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetPaladinFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Divine Sense", Description = "As an action, detect celestials, fiends, and undead within 60 feet. You can use this feature a number of times equal to 1 + CHA modifier and regain all uses after a long rest.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Lay on Hands", Description = "You have a pool of healing power that replenishes after a long rest. With it, you can restore HP equal to 5 × your paladin level.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Fighting Style", Description = "Choose a fighting style.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Divine Smite", Description = "When you hit with a melee weapon attack, expend a spell slot to deal extra radiant damage to the target (2d8 for 1st level, +1d8 per level, +1d8 vs undead/fiends, max 5d8).", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Sacred Oath", Description = "Choose a Sacred Oath that guides your actions and grants special powers.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Channel Divinity", Description = "Your oath allows you to channel divine energy to fuel magical effects.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 3, MaxUsesPerShortRest = 1, RechargeType = RechargeType.ShortRest },
            new Feature { Name = "Extra Attack", Description = "You can attack twice whenever you take the Attack action.", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 5, RechargeType = RechargeType.None },
            new Feature { Name = "Aura of Protection", Description = "You and friendly creatures within 10 feet gain a bonus to saving throws equal to your CHA modifier (minimum +1).", Source = FeatureSource.Class, SourceName = "Paladin", LevelRequirement = 6, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetRangerFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Favored Enemy", Description = "Choose a type of favored enemy. You have advantage on Survival checks to track them and on Intelligence checks to recall information about them.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Natural Explorer", Description = "You are a master of navigating the natural world. Choose a favored terrain type.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Fighting Style", Description = "Choose a fighting style.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Ranger Archetype", Description = "Choose a Ranger Archetype that reflects your wilderness specialization.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Extra Attack", Description = "You can attack twice whenever you take the Attack action.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 5, RechargeType = RechargeType.None },
            new Feature { Name = "Land's Stride", Description = "Moving through nonmagical difficult terrain costs you no extra movement. You can pass through nonmagical plants without slowing or taking damage.", Source = FeatureSource.Class, SourceName = "Ranger", LevelRequirement = 8, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetRogueFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Expertise", Description = "Choose two skills you're proficient in. Your proficiency bonus is doubled for ability checks with them.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Sneak Attack", Description = "Once per turn, deal an extra 1d6 damage to a creature you hit with an attack if you have advantage or an ally is within 5 feet of the target. Damage increases as you gain levels.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Thieves' Cant", Description = "You know thieves' cant, a secret mix of dialect, jargon, and code.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Cunning Action", Description = "You can take a bonus action to Dash, Disengage, or Hide.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Roguish Archetype", Description = "Choose a Roguish Archetype that reflects your criminal expertise.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Uncanny Dodge", Description = "When an attacker you can see hits you with an attack, use your reaction to halve the attack's damage.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 5, RechargeType = RechargeType.None },
            new Feature { Name = "Evasion", Description = "When subjected to an effect that allows a DEX save for half damage, you take no damage on a success and half on a failure.", Source = FeatureSource.Class, SourceName = "Rogue", LevelRequirement = 7, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetSorcererFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Sorcerous Origin", Description = "Choose a Sorcerous Origin that describes the source of your magic.", Source = FeatureSource.Class, SourceName = "Sorcerer", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Font of Magic", Description = "You have a pool of sorcery points equal to your sorcerer level. You can use them to create spell slots or fuel Metamagic.", Source = FeatureSource.Class, SourceName = "Sorcerer", LevelRequirement = 2, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Metamagic", Description = "You gain the ability to twist your spells to suit your needs. Choose two Metamagic options.", Source = FeatureSource.Class, SourceName = "Sorcerer", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Sorcerous Restoration", Description = "When you finish a short rest, you regain sorcery points equal to your CHA modifier (minimum 1).", Source = FeatureSource.Class, SourceName = "Sorcerer", LevelRequirement = 20, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetWarlockFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Otherworldly Patron", Description = "Choose an Otherworldly Patron that grants you power.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Pact Magic", Description = "You regain all expended spell slots when you finish a short or long rest.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Eldritch Invocations", Description = "You learn two Eldritch Invocations, which are magical tricks granted by your patron.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Pact Boon", Description = "Your patron bestows a gift upon you. Choose Pact of the Chain, Blade, or Tome.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Mystic Arcanum", Description = "Choose one 6th-level spell from the warlock spell list. You can cast it once without expending a spell slot and regain the ability after a long rest.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 11, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Eldritch Master", Description = "Spend 1 minute entreating your patron to regain all expended spell slots from Pact Magic. Once used, you must finish a long rest before using this again.", Source = FeatureSource.Class, SourceName = "Warlock", LevelRequirement = 20, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest }
        };
    }

    private static List<Feature> GetWizardFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Arcane Recovery", Description = "Once per day during a short rest, you can recover some expended spell slots. The spell slots can have a combined level equal to or less than half your wizard level (rounded up).", Source = FeatureSource.Class, SourceName = "Wizard", LevelRequirement = 1, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Arcane Tradition", Description = "Choose an Arcane Tradition that shapes your magical studies.", Source = FeatureSource.Class, SourceName = "Wizard", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Spell Mastery", Description = "Choose a 1st-level and 2nd-level wizard spell in your spellbook. You can cast them at their lowest level without expending a spell slot.", Source = FeatureSource.Class, SourceName = "Wizard", LevelRequirement = 18, RechargeType = RechargeType.None },
            new Feature { Name = "Signature Spells", Description = "Choose two 3rd-level wizard spells in your spellbook. They are always prepared and don't count against your prepared spells. You can cast each at 3rd level once per short rest without expending a spell slot.", Source = FeatureSource.Class, SourceName = "Wizard", LevelRequirement = 20, MaxUsesPerShortRest = 1, RechargeType = RechargeType.ShortRest }
        };
    }

    private static List<Feature> GetArtificerFeatures()
    {
        return new List<Feature>
        {
            new Feature { Name = "Magical Tinkering", Description = "You can imbue a Tiny nonmagical object with minor magical properties.", Source = FeatureSource.Class, SourceName = "Artificer", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Infuse Item", Description = "You can imbue mundane items with magical infusions. You learn a number of infusions and can have a limited number active at once.", Source = FeatureSource.Class, SourceName = "Artificer", LevelRequirement = 2, RechargeType = RechargeType.None },
            new Feature { Name = "Artificer Specialist", Description = "Choose an Artificer Specialist that reflects your area of expertise.", Source = FeatureSource.Class, SourceName = "Artificer", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "The Right Tool for the Job", Description = "Over 1 hour, you can magically create one set of artisan's tools.", Source = FeatureSource.Class, SourceName = "Artificer", LevelRequirement = 3, RechargeType = RechargeType.None },
            new Feature { Name = "Tool Expertise", Description = "Your proficiency bonus is doubled for any ability check you make that uses your proficiency with a tool.", Source = FeatureSource.Class, SourceName = "Artificer", LevelRequirement = 6, RechargeType = RechargeType.None }
        };
    }

    private static List<Feature> GetSpeciesFeatures()
    {
        return new List<Feature>
        {
            // Human
            new Feature { Name = "Resourceful", Description = "You gain Inspiration whenever you finish a long rest.", Source = FeatureSource.Species, SourceName = "Human", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Skillful", Description = "You gain proficiency in one skill of your choice.", Source = FeatureSource.Species, SourceName = "Human", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Versatile", Description = "You gain an Origin feat.", Source = FeatureSource.Species, SourceName = "Human", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Elf
            new Feature { Name = "Darkvision", Description = "You can see in dim light within 60 feet as if it were bright light, and in darkness as if it were dim light.", Source = FeatureSource.Species, SourceName = "Elf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Elven Lineage", Description = "You are in the Humanoid creature type, and you are also considered an elf for any prerequisite or effect that requires you to be an elf.", Source = FeatureSource.Species, SourceName = "Elf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Fey Ancestry", Description = "You have Advantage on saving throws you make to avoid or end the Charmed condition.", Source = FeatureSource.Species, SourceName = "Elf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Keen Senses", Description = "You have proficiency in the Perception skill.", Source = FeatureSource.Species, SourceName = "Elf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Trance", Description = "You don't need to sleep, and magic can't put you to sleep. You finish a Long Rest in 4 hours if you spend it in a trancelike meditation.", Source = FeatureSource.Species, SourceName = "Elf", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Dwarf
            new Feature { Name = "Darkvision", Description = "You can see in dim light within 120 feet as if it were bright light, and in darkness as if it were dim light.", Source = FeatureSource.Species, SourceName = "Dwarf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Dwarven Resilience", Description = "You have Advantage on saving throws against poison, and you have Resistance to Poison damage.", Source = FeatureSource.Species, SourceName = "Dwarf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Dwarven Toughness", Description = "Your Hit Point maximum increases by 1, and it increases by 1 again whenever you gain a level.", Source = FeatureSource.Species, SourceName = "Dwarf", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Stonecunning", Description = "As a Bonus Action, you gain Tremorsense with a range of 60 feet for 10 minutes. You must finish a Long Rest before you can use this feature again.", Source = FeatureSource.Species, SourceName = "Dwarf", LevelRequirement = 1, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest },

            // Halfling
            new Feature { Name = "Brave", Description = "You have Advantage on saving throws you make to avoid or end the Frightened condition.", Source = FeatureSource.Species, SourceName = "Halfling", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Halfling Nimbleness", Description = "You can move through the space of any creature that is a size larger than you, but you can't stop in the same space.", Source = FeatureSource.Species, SourceName = "Halfling", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Luck", Description = "When you roll a 1 on the d20 of a D20 Test, you can reroll the die, and you must use the new roll.", Source = FeatureSource.Species, SourceName = "Halfling", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Naturally Stealthy", Description = "You can take the Hide action even when you are obscured only by a creature that is at least one size larger than you.", Source = FeatureSource.Species, SourceName = "Halfling", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Dragonborn
            new Feature { Name = "Draconic Ancestry", Description = "You are a Humanoid descended from dragons. Choose a type of dragon; this determines your breath weapon and damage resistance.", Source = FeatureSource.Species, SourceName = "Dragonborn", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Breath Weapon", Description = "As an action, exhale destructive energy based on your Draconic Ancestry. Each creature in the area must make a saving throw. You can use this a number of times equal to your proficiency bonus.", Source = FeatureSource.Species, SourceName = "Dragonborn", LevelRequirement = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Damage Resistance", Description = "You have resistance to the damage type associated with your Draconic Ancestry.", Source = FeatureSource.Species, SourceName = "Dragonborn", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Gnome
            new Feature { Name = "Darkvision", Description = "You can see in dim light within 60 feet as if it were bright light, and in darkness as if it were dim light.", Source = FeatureSource.Species, SourceName = "Gnome", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Gnome Cunning", Description = "You have Advantage on INT, WIS, and CHA saving throws against spells.", Source = FeatureSource.Species, SourceName = "Gnome", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Tiefling
            new Feature { Name = "Darkvision", Description = "You can see in dim light within 60 feet as if it were bright light, and in darkness as if it were dim light.", Source = FeatureSource.Species, SourceName = "Tiefling", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Fiendish Legacy", Description = "You are the recipient of a legacy that grants you supernatural abilities. Choose a legacy: Abyssal, Chthonic, or Infernal.", Source = FeatureSource.Species, SourceName = "Tiefling", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Otherworldly Presence", Description = "You know the Thaumaturgy cantrip. Starting at 3rd level, you can cast a spell (depending on legacy) with this trait. Once you cast it, you can't do so again until you finish a Long Rest. CHA is your spellcasting ability.", Source = FeatureSource.Species, SourceName = "Tiefling", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Orc
            new Feature { Name = "Adrenaline Rush", Description = "You can take the Dash action as a Bonus Action. When you do, you gain Temporary Hit Points equal to your Proficiency Bonus. You can use this a number of times equal to your Proficiency Bonus.", Source = FeatureSource.Species, SourceName = "Orc", LevelRequirement = 1, RechargeType = RechargeType.LongRest },
            new Feature { Name = "Darkvision", Description = "You can see in dim light within 120 feet as if it were bright light, and in darkness as if it were dim light.", Source = FeatureSource.Species, SourceName = "Orc", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Relentless Endurance", Description = "When you are reduced to 0 HP but not killed outright, you can drop to 1 HP instead. Once you use this trait, you can't do so again until you finish a Long Rest.", Source = FeatureSource.Species, SourceName = "Orc", LevelRequirement = 1, MaxUsesPerLongRest = 1, RechargeType = RechargeType.LongRest }
        };
    }

    private static List<Feature> GetBackgroundFeatures()
    {
        return new List<Feature>
        {
            // Acolyte
            new Feature { Name = "Shelter of the Faithful", Description = "You can receive free healing and care at temples and shrines of your faith. Those who share your religion will support you at a modest lifestyle.", Source = FeatureSource.Background, SourceName = "Acolyte", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Acolyte Training", Description = "You have proficiency in Insight and Religion. You gain the Magic Initiate feat.", Source = FeatureSource.Background, SourceName = "Acolyte", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Artisan
            new Feature { Name = "Artisan's Business", Description = "You're familiar with the business side of crafting. You can identify the quality and value of crafted items, and you have contacts with merchants and artisans.", Source = FeatureSource.Background, SourceName = "Artisan", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Artisan Training", Description = "You have proficiency in Investigation and Persuasion. You gain proficiency with one type of artisan's tools.", Source = FeatureSource.Background, SourceName = "Artisan", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Charlatan
            new Feature { Name = "False Identity", Description = "You have created a second identity that includes documentation and acquaintances. You can adopt this identity or create similar identities.", Source = FeatureSource.Background, SourceName = "Charlatan", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Charlatan Training", Description = "You have proficiency in Deception and Sleight of Hand. You gain the Skilled feat.", Source = FeatureSource.Background, SourceName = "Charlatan", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Criminal
            new Feature { Name = "Criminal Contact", Description = "You have a reliable contact who acts as your liaison to a network of criminals. You know how to get messages to and from your contact even over great distances.", Source = FeatureSource.Background, SourceName = "Criminal", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Criminal Training", Description = "You have proficiency in Sleight of Hand and Stealth. You gain the Alert feat.", Source = FeatureSource.Background, SourceName = "Criminal", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Entertainer
            new Feature { Name = "By Popular Demand", Description = "You can always find a place to perform. You receive free lodging and food at inns and taverns where you perform, and local audiences love you.", Source = FeatureSource.Background, SourceName = "Entertainer", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Entertainer Training", Description = "You have proficiency in Acrobatics and Performance. You gain the Musician feat.", Source = FeatureSource.Background, SourceName = "Entertainer", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Farmer
            new Feature { Name = "Agriculturist", Description = "You have a deep knowledge of farming, animal husbandry, and the seasons. You can predict weather and recognize signs of blight or disease in crops and livestock.", Source = FeatureSource.Background, SourceName = "Farmer", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Farmer Training", Description = "You have proficiency in Animal Handling and Nature. You gain the Tough feat.", Source = FeatureSource.Background, SourceName = "Farmer", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Guard
            new Feature { Name = "Watcher's Eye", Description = "Your experience enforcing law makes you familiar with criminal behavior. You can spot criminals and recognize signs of illegal activity.", Source = FeatureSource.Background, SourceName = "Guard", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Guard Training", Description = "You have proficiency in Athletics and Perception. You gain the Alert feat.", Source = FeatureSource.Background, SourceName = "Guard", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Guide
            new Feature { Name = "Wanderer", Description = "You have an excellent memory for geography and maps. You can find food and fresh water for yourself and up to five others each day in the wilderness.", Source = FeatureSource.Background, SourceName = "Guide", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Guide Training", Description = "You have proficiency in Survival and Stealth. You gain the Magic Initiate feat.", Source = FeatureSource.Background, SourceName = "Guide", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Hermit
            new Feature { Name = "Discovery", Description = "The quiet seclusion of your extended hermitage gave you access to a unique and powerful discovery about nature, the cosmos, or the supernatural.", Source = FeatureSource.Background, SourceName = "Hermit", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Hermit Training", Description = "You have proficiency in Medicine and Religion. You gain the Magic Initiate feat.", Source = FeatureSource.Background, SourceName = "Hermit", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Merchant
            new Feature { Name = "Business Contact", Description = "You have contacts with merchants and trade guilds across the land. You can use these contacts to get information and fair prices on goods.", Source = FeatureSource.Background, SourceName = "Merchant", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Merchant Training", Description = "You have proficiency in Insight and Persuasion. You gain the Lucky feat.", Source = FeatureSource.Background, SourceName = "Merchant", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Noble
            new Feature { Name = "Position of Privilege", Description = "Thanks to your noble birth, people are inclined to think the best of you. You can secure an audience with local nobility and are welcome in high society.", Source = FeatureSource.Background, SourceName = "Noble", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Noble Training", Description = "You have proficiency in History and Persuasion. You gain the Skilled feat.", Source = FeatureSource.Background, SourceName = "Noble", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Sage
            new Feature { Name = "Researcher", Description = "When you attempt to learn something, you know where to find the information. This might be in a library, scriptorium, university, or from a sage or other learned person.", Source = FeatureSource.Background, SourceName = "Sage", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Sage Training", Description = "You have proficiency in Arcana and History. You gain the Magic Initiate feat.", Source = FeatureSource.Background, SourceName = "Sage", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Sailor
            new Feature { Name = "Ship's Passage", Description = "When you need to, you can secure free passage on a sailing ship. You're familiar with sailing vessels and the people who operate them.", Source = FeatureSource.Background, SourceName = "Sailor", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Sailor Training", Description = "You have proficiency in Athletics and Perception. You gain the Tavern Brawler feat.", Source = FeatureSource.Background, SourceName = "Sailor", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Scribe
            new Feature { Name = "Scribe's Insight", Description = "Your experience with documents and records gives you insight into bureaucracies and legal systems. You can identify forged documents and navigate complex organizations.", Source = FeatureSource.Background, SourceName = "Scribe", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Scribe Training", Description = "You have proficiency in Investigation and History. You gain the Skilled feat.", Source = FeatureSource.Background, SourceName = "Scribe", LevelRequirement = 1, RechargeType = RechargeType.None },

            // Soldier
            new Feature { Name = "Military Rank", Description = "You have a military rank from your career as a soldier. Soldiers loyal to your former organization still recognize your authority and influence.", Source = FeatureSource.Background, SourceName = "Soldier", LevelRequirement = 1, RechargeType = RechargeType.None },
            new Feature { Name = "Soldier Training", Description = "You have proficiency in Athletics and Intimidation. You gain the Savage Attacker feat.", Source = FeatureSource.Background, SourceName = "Soldier", LevelRequirement = 1, RechargeType = RechargeType.None }
        };
    }
}

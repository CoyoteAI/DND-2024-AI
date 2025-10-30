using DnDAI.Core.Enums;

namespace DnDAI.Core.Helpers;

public static class SubclassHelper
{
    private static readonly Dictionary<CharacterClass, List<CharacterSubclass>> _classToSubclasses = new()
    {
        {
            CharacterClass.Barbarian, new List<CharacterSubclass>
            {
                CharacterSubclass.PathOfTheBeserker,
                CharacterSubclass.PathOfTheTotemWarrior,
                CharacterSubclass.PathOfTheAncestralGuardian,
                CharacterSubclass.PathOfTheStormHerald,
                CharacterSubclass.PathOfTheZealot,
                CharacterSubclass.PathOfWildMagic,
                CharacterSubclass.PathOfTheBeast,
                CharacterSubclass.PathOfTheGiant,
                CharacterSubclass.WorldTree
            }
        },
        {
            CharacterClass.Bard, new List<CharacterSubclass>
            {
                CharacterSubclass.CollegeOfLore,
                CharacterSubclass.CollegeOfValor,
                CharacterSubclass.CollegeOfGlamour,
                CharacterSubclass.CollegeOfSwords,
                CharacterSubclass.CollegeOfWhispers,
                CharacterSubclass.CollegeOfCreation,
                CharacterSubclass.CollegeOfEloquence,
                CharacterSubclass.CollegeOfSpirits,
                CharacterSubclass.CollegeOfDance
            }
        },
        {
            CharacterClass.Cleric, new List<CharacterSubclass>
            {
                CharacterSubclass.LifeDomain,
                CharacterSubclass.LightDomain,
                CharacterSubclass.TrickeryDomain,
                CharacterSubclass.WarDomain,
                CharacterSubclass.TempestDomain,
                CharacterSubclass.NatureDomain,
                CharacterSubclass.KnowledgeDomain,
                CharacterSubclass.DeathDomain,
                CharacterSubclass.ForgeDomain,
                CharacterSubclass.GraveDomain,
                CharacterSubclass.OrderDomain,
                CharacterSubclass.PeaceDomain,
                CharacterSubclass.TwilightDomain,
                CharacterSubclass.ArcanaDomain
            }
        },
        {
            CharacterClass.Druid, new List<CharacterSubclass>
            {
                CharacterSubclass.CircleOfTheLand,
                CharacterSubclass.CircleOfTheMoon,
                CharacterSubclass.CircleOfTheShepherd,
                CharacterSubclass.CircleOfSpores,
                CharacterSubclass.CircleOfStars,
                CharacterSubclass.CircleOfWildfire,
                CharacterSubclass.CircleOfDreams,
                CharacterSubclass.CircleOfTheSea
            }
        },
        {
            CharacterClass.Fighter, new List<CharacterSubclass>
            {
                CharacterSubclass.Champion,
                CharacterSubclass.BattleMaster,
                CharacterSubclass.EldritchKnight,
                CharacterSubclass.ArcaneArcher,
                CharacterSubclass.Cavalier,
                CharacterSubclass.Samurai,
                CharacterSubclass.PsiWarrior,
                CharacterSubclass.RuneKnight,
                CharacterSubclass.EchoKnight,
                CharacterSubclass.PurpleDragonKnight
            }
        },
        {
            CharacterClass.Monk, new List<CharacterSubclass>
            {
                CharacterSubclass.WayOfTheOpenHand,
                CharacterSubclass.WayOfShadow,
                CharacterSubclass.WayOfTheFourElements,
                CharacterSubclass.WayOfTheDrunkenMaster,
                CharacterSubclass.WayOfTheSunSoul,
                CharacterSubclass.WayOfTheKensei,
                CharacterSubclass.WayOfMercy,
                CharacterSubclass.WayOfTheAstralSelf,
                CharacterSubclass.WayOfTheLongDeath,
                CharacterSubclass.WayOfTheAscendantDragon
            }
        },
        {
            CharacterClass.Paladin, new List<CharacterSubclass>
            {
                CharacterSubclass.OathOfDevotion,
                CharacterSubclass.OathOfTheAncients,
                CharacterSubclass.OathOfVengeance,
                CharacterSubclass.OathOfConquest,
                CharacterSubclass.OathOfRedemption,
                CharacterSubclass.OathOfGlory,
                CharacterSubclass.OathOfTheWatchers,
                CharacterSubclass.OathOfTheCrown,
                CharacterSubclass.Oathbreaker
            }
        },
        {
            CharacterClass.Ranger, new List<CharacterSubclass>
            {
                CharacterSubclass.Hunter,
                CharacterSubclass.BeastMaster,
                CharacterSubclass.GloomStalker,
                CharacterSubclass.HorizonWalker,
                CharacterSubclass.MonsterSlayer,
                CharacterSubclass.FeyWanderer,
                CharacterSubclass.SwarmKeeper,
                CharacterSubclass.DrakewWarden
            }
        },
        {
            CharacterClass.Rogue, new List<CharacterSubclass>
            {
                CharacterSubclass.Thief,
                CharacterSubclass.Assassin,
                CharacterSubclass.ArcaneTrickster,
                CharacterSubclass.Inquisitive,
                CharacterSubclass.Mastermind,
                CharacterSubclass.Scout,
                CharacterSubclass.Swashbuckler,
                CharacterSubclass.Phantom,
                CharacterSubclass.Soulknife
            }
        },
        {
            CharacterClass.Sorcerer, new List<CharacterSubclass>
            {
                CharacterSubclass.DraconicBloodline,
                CharacterSubclass.WildMagic,
                CharacterSubclass.DivineSoul,
                CharacterSubclass.ShadowMagic,
                CharacterSubclass.StormSorcery,
                CharacterSubclass.AberrantMind,
                CharacterSubclass.ClockworkSoul,
                CharacterSubclass.LunarSorcery
            }
        },
        {
            CharacterClass.Warlock, new List<CharacterSubclass>
            {
                CharacterSubclass.TheArchfey,
                CharacterSubclass.TheFiend,
                CharacterSubclass.TheGreatOldOne,
                CharacterSubclass.TheCelestial,
                CharacterSubclass.TheHexblade,
                CharacterSubclass.TheFathomless,
                CharacterSubclass.TheGenie,
                CharacterSubclass.TheUndead,
                CharacterSubclass.TheUndying
            }
        },
        {
            CharacterClass.Wizard, new List<CharacterSubclass>
            {
                CharacterSubclass.SchoolOfAbjuration,
                CharacterSubclass.SchoolOfConjuration,
                CharacterSubclass.SchoolOfDivination,
                CharacterSubclass.SchoolOfEnchantment,
                CharacterSubclass.SchoolOfEvocation,
                CharacterSubclass.SchoolOfIllusion,
                CharacterSubclass.SchoolOfNecromancy,
                CharacterSubclass.SchoolOfTransmutation,
                CharacterSubclass.Bladesinging,
                CharacterSubclass.OrderOfScribes,
                CharacterSubclass.Chronurgy,
                CharacterSubclass.Graviturgy
            }
        },
        {
            CharacterClass.Artificer, new List<CharacterSubclass>
            {
                CharacterSubclass.Alchemist,
                CharacterSubclass.Armorer,
                CharacterSubclass.Artillerist,
                CharacterSubclass.BattleSmith
            }
        }
    };

    public static List<CharacterSubclass> GetSubclassesForClass(CharacterClass characterClass)
    {
        if (_classToSubclasses.TryGetValue(characterClass, out var subclasses))
        {
            return subclasses;
        }
        return new List<CharacterSubclass>();
    }

    public static string GetFriendlyName(CharacterSubclass subclass)
    {
        if (subclass == CharacterSubclass.None)
            return "None";

        // Convert PascalCase to "Title Case with Spaces"
        var name = subclass.ToString();
        var result = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        return result;
    }
}

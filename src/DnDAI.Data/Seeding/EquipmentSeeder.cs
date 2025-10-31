using DnDAI.Core.Enums;
using DnDAI.Core.Models;

namespace DnDAI.Data.Seeding;

public static class EquipmentSeeder
{
    public static void SeedEquipment(DnDContext context)
    {
        // Check if equipment already exists
        if (context.Equipment.Any())
        {
            return; // Already seeded
        }

        var equipment = new List<Equipment>();

        // Add all weapons
        equipment.AddRange(GetSimpleMeleeWeapons());
        equipment.AddRange(GetSimpleRangedWeapons());
        equipment.AddRange(GetMartialMeleeWeapons());
        equipment.AddRange(GetMartialRangedWeapons());

        // Add armor
        equipment.AddRange(GetArmor());

        // Add adventuring gear
        equipment.AddRange(GetAdventuringGear());

        // Add containers
        equipment.AddRange(GetContainers());

        // Set timestamps for all equipment
        var now = DateTime.UtcNow;
        foreach (var item in equipment)
        {
            item.CreatedAt = now;
            item.UpdatedAt = now;
        }

        context.Equipment.AddRange(equipment);
        context.SaveChanges();
    }

    private static List<Equipment> GetSimpleMeleeWeapons()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Club",
                Description = "A simple bludgeoning weapon.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.1m,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d4",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Dagger",
                Description = "A light, finesse weapon suitable for close combat or throwing.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 2,
                Weight = 1,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d4",
                DamageType = "Piercing",
                IsFinesse = true,
                IsVersatile = false,
                Range = "20/60"
            },
            new Equipment
            {
                Name = "Greatclub",
                Description = "A large, two-handed club.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.2m,
                Weight = 10,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d8",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Handaxe",
                Description = "A light axe that can be thrown.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 5,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d6",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "20/60"
            },
            new Equipment
            {
                Name = "Javelin",
                Description = "A thrown weapon with good range.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.5m,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "30/120"
            },
            new Equipment
            {
                Name = "Light Hammer",
                Description = "A small hammer that can be thrown.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 2,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d4",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false,
                Range = "20/60"
            },
            new Equipment
            {
                Name = "Mace",
                Description = "A solid bludgeoning weapon.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 5,
                Weight = 4,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d6",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Quarterstaff",
                Description = "A versatile wooden staff.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.2m,
                Weight = 4,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d6",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d8"
            },
            new Equipment
            {
                Name = "Sickle",
                Description = "A curved blade used for reaping.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 1,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d4",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Spear",
                Description = "A versatile piercing weapon that can be thrown.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 1,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleMelee,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d8",
                Range = "20/60"
            }
        };
    }

    private static List<Equipment> GetSimpleRangedWeapons()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Light Crossbow",
                Description = "A simple ranged weapon that fires bolts.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 25,
                Weight = 5,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleRanged,
                Damage = "1d8",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "80/320"
            },
            new Equipment
            {
                Name = "Dart",
                Description = "A small thrown weapon.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.05m,
                Weight = 0.25m,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleRanged,
                Damage = "1d4",
                DamageType = "Piercing",
                IsFinesse = true,
                IsVersatile = false,
                Range = "20/60"
            },
            new Equipment
            {
                Name = "Shortbow",
                Description = "A simple bow suitable for close to medium range.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 25,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleRanged,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "80/320"
            },
            new Equipment
            {
                Name = "Sling",
                Description = "A simple ranged weapon using stones or bullets.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 0.1m,
                Weight = 0,
                WeaponCategory = Core.Enums.WeaponCategory.SimpleRanged,
                Damage = "1d4",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false,
                Range = "30/120"
            }
        };
    }

    private static List<Equipment> GetMartialMeleeWeapons()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Battleaxe",
                Description = "A versatile axe for combat.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 4,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d10"
            },
            new Equipment
            {
                Name = "Flail",
                Description = "A spiked ball on a chain.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Glaive",
                Description = "A pole weapon with a blade on the end.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 20,
                Weight = 6,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d10",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Greataxe",
                Description = "A massive two-handed axe.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 30,
                Weight = 7,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d12",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Greatsword",
                Description = "A large two-handed sword.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 50,
                Weight = 6,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "2d6",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Halberd",
                Description = "A pole weapon with an axe blade and spike.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 20,
                Weight = 6,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d10",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Lance",
                Description = "A long weapon designed for mounted combat.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 6,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d12",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Longsword",
                Description = "A versatile blade, iconic weapon of knights.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 15,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Slashing",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d10"
            },
            new Equipment
            {
                Name = "Maul",
                Description = "A heavy two-handed hammer.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 10,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "2d6",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Morningstar",
                Description = "A spiked mace.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 15,
                Weight = 4,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Pike",
                Description = "A very long spear.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 5,
                Weight = 18,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d10",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Rapier",
                Description = "An elegant finesse blade.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 25,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Piercing",
                IsFinesse = true,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Scimitar",
                Description = "A curved finesse blade.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 25,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d6",
                DamageType = "Slashing",
                IsFinesse = true,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Shortsword",
                Description = "A short finesse blade.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = true,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Trident",
                Description = "A three-pronged spear.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 5,
                Weight = 4,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d8",
                Range = "20/60"
            },
            new Equipment
            {
                Name = "War Pick",
                Description = "A pick designed for penetrating armor.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 5,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false
            },
            new Equipment
            {
                Name = "Warhammer",
                Description = "A versatile hammer for combat.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 15,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d8",
                DamageType = "Bludgeoning",
                IsFinesse = false,
                IsVersatile = true,
                VersatileDamage = "1d10"
            },
            new Equipment
            {
                Name = "Whip",
                Description = "A flexible finesse weapon with reach.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 2,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.MartialMelee,
                Damage = "1d4",
                DamageType = "Slashing",
                IsFinesse = true,
                IsVersatile = false
            }
        };
    }

    private static List<Equipment> GetMartialRangedWeapons()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Blowgun",
                Description = "A tube for firing darts silently.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 10,
                Weight = 1,
                WeaponCategory = Core.Enums.WeaponCategory.MartialRanged,
                Damage = "1",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "25/100"
            },
            new Equipment
            {
                Name = "Hand Crossbow",
                Description = "A small crossbow held in one hand.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 75,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.MartialRanged,
                Damage = "1d6",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "30/120"
            },
            new Equipment
            {
                Name = "Heavy Crossbow",
                Description = "A powerful crossbow requiring strength to load.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 50,
                Weight = 18,
                WeaponCategory = Core.Enums.WeaponCategory.MartialRanged,
                Damage = "1d10",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "100/400"
            },
            new Equipment
            {
                Name = "Longbow",
                Description = "A tall bow with excellent range.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 50,
                Weight = 2,
                WeaponCategory = Core.Enums.WeaponCategory.MartialRanged,
                Damage = "1d8",
                DamageType = "Piercing",
                IsFinesse = false,
                IsVersatile = false,
                Range = "150/600"
            },
            new Equipment
            {
                Name = "Net",
                Description = "A thrown weapon that restrains targets.",
                Type = EquipmentType.Weapon,
                IsStandard = true,
                CostInGold = 1,
                Weight = 3,
                WeaponCategory = Core.Enums.WeaponCategory.MartialRanged,
                Damage = "0",
                DamageType = "Special",
                IsFinesse = false,
                IsVersatile = false,
                Range = "5/15"
            }
        };
    }

    private static List<Equipment> GetArmor()
    {
        return new List<Equipment>
        {
            // Light Armor
            new Equipment
            {
                Name = "Padded Armor",
                Description = "Padded armor consists of quilted layers of cloth and batting.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 5,
                Weight = 8,
                ArmorCategory = ArmorCategory.LightArmor,
                ArmorClass = 11,
                AddDexModifier = true,
                StealthDisadvantage = true
            },
            new Equipment
            {
                Name = "Leather Armor",
                Description = "The breastplate and shoulder protectors of this armor are made of leather.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 10,
                Weight = 10,
                ArmorCategory = ArmorCategory.LightArmor,
                ArmorClass = 11,
                AddDexModifier = true
            },
            new Equipment
            {
                Name = "Studded Leather Armor",
                Description = "Made from tough but flexible leather, studded leather is reinforced with close-set rivets or spikes.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 45,
                Weight = 13,
                ArmorCategory = ArmorCategory.LightArmor,
                ArmorClass = 12,
                AddDexModifier = true
            },
            // Medium Armor
            new Equipment
            {
                Name = "Hide Armor",
                Description = "This crude armor consists of thick furs and pelts.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 10,
                Weight = 12,
                ArmorCategory = ArmorCategory.MediumArmor,
                ArmorClass = 12,
                AddDexModifier = true,
                MaxDexModifier = 2
            },
            new Equipment
            {
                Name = "Chain Shirt",
                Description = "Made of interlocking metal rings, a chain shirt is worn between layers of clothing or leather.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 50,
                Weight = 20,
                ArmorCategory = ArmorCategory.MediumArmor,
                ArmorClass = 13,
                AddDexModifier = true,
                MaxDexModifier = 2
            },
            new Equipment
            {
                Name = "Scale Mail",
                Description = "This armor consists of a coat and leggings of leather covered with overlapping pieces of metal.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 50,
                Weight = 45,
                ArmorCategory = ArmorCategory.MediumArmor,
                ArmorClass = 14,
                AddDexModifier = true,
                MaxDexModifier = 2,
                StealthDisadvantage = true
            },
            new Equipment
            {
                Name = "Breastplate",
                Description = "This armor consists of a fitted metal chest piece worn with supple leather.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 400,
                Weight = 20,
                ArmorCategory = ArmorCategory.MediumArmor,
                ArmorClass = 14,
                AddDexModifier = true,
                MaxDexModifier = 2
            },
            new Equipment
            {
                Name = "Half Plate",
                Description = "Half plate consists of shaped metal plates that cover most of the wearer's body.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 750,
                Weight = 40,
                ArmorCategory = ArmorCategory.MediumArmor,
                ArmorClass = 15,
                AddDexModifier = true,
                MaxDexModifier = 2,
                StealthDisadvantage = true
            },
            // Heavy Armor
            new Equipment
            {
                Name = "Ring Mail",
                Description = "This armor is leather armor with heavy rings sewn into it.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 30,
                Weight = 40,
                ArmorCategory = ArmorCategory.HeavyArmor,
                ArmorClass = 14,
                AddDexModifier = false,
                StealthDisadvantage = true
            },
            new Equipment
            {
                Name = "Chain Mail",
                Description = "Made of interlocking metal rings, chain mail includes a layer of quilted fabric worn underneath.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 75,
                Weight = 55,
                ArmorCategory = ArmorCategory.HeavyArmor,
                ArmorClass = 16,
                AddDexModifier = false,
                StealthDisadvantage = true,
                StrengthRequirement = 13
            },
            new Equipment
            {
                Name = "Splint Armor",
                Description = "This armor is made of narrow vertical strips of metal riveted to a backing of leather.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 200,
                Weight = 60,
                ArmorCategory = ArmorCategory.HeavyArmor,
                ArmorClass = 17,
                AddDexModifier = false,
                StealthDisadvantage = true,
                StrengthRequirement = 15
            },
            new Equipment
            {
                Name = "Plate Armor",
                Description = "Plate consists of shaped, interlocking metal plates to cover the entire body.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 1500,
                Weight = 65,
                ArmorCategory = ArmorCategory.HeavyArmor,
                ArmorClass = 18,
                AddDexModifier = false,
                StealthDisadvantage = true,
                StrengthRequirement = 15
            },
            // Shield
            new Equipment
            {
                Name = "Shield",
                Description = "A shield is made from wood or metal and is carried in one hand.",
                Type = EquipmentType.Armor,
                IsStandard = true,
                CostInGold = 10,
                Weight = 6,
                ArmorCategory = ArmorCategory.Shield,
                ArmorClass = 2
            }
        };
    }

    private static List<Equipment> GetAdventuringGear()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Rope, Hempen (50 feet)",
                Description = "Rope has 2 hit points and can be burst with a DC 17 Strength check.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 1,
                Weight = 10
            },
            new Equipment
            {
                Name = "Rope, Silk (50 feet)",
                Description = "Silk rope has 2 hit points and can be burst with a DC 17 Strength check.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 10,
                Weight = 5
            },
            new Equipment
            {
                Name = "Torch",
                Description = "A torch burns for 1 hour, providing bright light in a 20-foot radius.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 0.01m,
                Weight = 1
            },
            new Equipment
            {
                Name = "Rations (1 day)",
                Description = "Rations consist of dry foods suitable for extended travel.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 0.5m,
                Weight = 2
            },
            new Equipment
            {
                Name = "Waterskin",
                Description = "A waterskin can hold up to 4 pints of liquid.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 0.2m,
                Weight = 5
            },
            new Equipment
            {
                Name = "Bedroll",
                Description = "A sleeping bag for use on adventures.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 1,
                Weight = 7
            },
            new Equipment
            {
                Name = "Tinderbox",
                Description = "This small container holds flint, fire steel, and tinder for kindling a fire.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 0.5m,
                Weight = 1
            },
            new Equipment
            {
                Name = "Crowbar",
                Description = "Using a crowbar grants advantage to Strength checks where leverage can be applied.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 2,
                Weight = 5
            },
            new Equipment
            {
                Name = "Grappling Hook",
                Description = "A grappling hook can secure a rope, allowing you to climb.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 2,
                Weight = 4
            },
            new Equipment
            {
                Name = "Lantern, Hooded",
                Description = "A hooded lantern casts bright light in a 30-foot radius for 6 hours on a pint of oil.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 5,
                Weight = 2
            },
            new Equipment
            {
                Name = "Oil (flask)",
                Description = "Oil can fuel a lantern or be used as a splash weapon dealing 5 fire damage.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 0.1m,
                Weight = 1
            },
            new Equipment
            {
                Name = "Caltrops (bag of 20)",
                Description = "One bag covers a 5-foot-square area. Any creature that enters the area must succeed on a DC 15 Dexterity saving throw or take 1 piercing damage.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 1,
                Weight = 2
            },
            new Equipment
            {
                Name = "Ball Bearings (bag of 1,000)",
                Description = "A bag covers a 10-foot square. Any creature moving through the area must succeed on a DC 10 Dexterity saving throw or fall prone.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 1,
                Weight = 2
            },
            new Equipment
            {
                Name = "Healer's Kit",
                Description = "Has ten uses. As an action, you can expend one use to stabilize a creature at 0 hit points.",
                Type = EquipmentType.Adventuring,
                IsStandard = true,
                CostInGold = 5,
                Weight = 3
            },
            new Equipment
            {
                Name = "Potion of Healing",
                Description = "Drinking this potion restores 2d4+2 hit points.",
                Type = EquipmentType.Consumable,
                IsStandard = true,
                CostInGold = 50,
                Weight = 0.5m,
                Rarity = ItemRarity.Common
            }
        };
    }

    private static List<Equipment> GetContainers()
    {
        return new List<Equipment>
        {
            new Equipment
            {
                Name = "Backpack",
                Description = "A backpack can hold 1 cubic foot or 30 pounds of gear.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 2,
                Weight = 5,
                WeightCapacity = 30,
                VolumeCapacity = 1
            },
            new Equipment
            {
                Name = "Pouch",
                Description = "A cloth or leather pouch can hold 1/5 cubic foot or 6 pounds of gear.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 0.5m,
                Weight = 1,
                WeightCapacity = 6,
                VolumeCapacity = 0.2m
            },
            new Equipment
            {
                Name = "Sack",
                Description = "A sack can hold 1 cubic foot or 30 pounds of gear.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 0.01m,
                Weight = 0.5m,
                WeightCapacity = 30,
                VolumeCapacity = 1
            },
            new Equipment
            {
                Name = "Chest",
                Description = "A sturdy wooden chest can hold 12 cubic feet or 300 pounds of gear.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 5,
                Weight = 25,
                WeightCapacity = 300,
                VolumeCapacity = 12
            },
            new Equipment
            {
                Name = "Barrel",
                Description = "A barrel can hold 40 gallons (5.3 cubic feet) or 300 pounds of gear.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 2,
                Weight = 70,
                WeightCapacity = 300,
                VolumeCapacity = 5.3m
            },
            new Equipment
            {
                Name = "Bag of Holding",
                Description = "This bag has an interior space considerably larger than its outside dimensions. The bag can hold up to 500 pounds, not exceeding a volume of 64 cubic feet.",
                Type = EquipmentType.Container,
                IsStandard = true,
                CostInGold = 500,  // Uncommon magic item pricing (typically 101-500 gp)
                Weight = 15,
                WeightCapacity = 500,
                VolumeCapacity = 64,
                Rarity = ItemRarity.Uncommon,
                RequiresAttunement = false
            }
        };
    }
}

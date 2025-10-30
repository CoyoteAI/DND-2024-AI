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
}

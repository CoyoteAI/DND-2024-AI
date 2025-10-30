using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DnDAI.Data;

public static class DatabaseMigrationHelper
{
    public static void ApplyManualMigrations(DnDContext context)
    {
        // Ensure database exists
        context.Database.EnsureCreated();

        // Add SkillProficiencies and SkillExpertise columns if they don't exist
        AddSkillColumnsIfNotExist(context);

        // Create Features tables if they don't exist
        CreateFeatureTablesIfNotExist(context);

        // Create Equipment tables if they don't exist
        CreateEquipmentTablesIfNotExist(context);
    }

    private static void AddSkillColumnsIfNotExist(DnDContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();

            // Check if SkillProficiencies column exists
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'PlayerCharacters')
                    AND name = 'SkillProficiencies'
                )
                BEGIN
                    ALTER TABLE PlayerCharacters ADD SkillProficiencies nvarchar(max) NOT NULL DEFAULT ''
                END";
            command.ExecuteNonQuery();

            // Check if SkillExpertise column exists
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'PlayerCharacters')
                    AND name = 'SkillExpertise'
                )
                BEGIN
                    ALTER TABLE PlayerCharacters ADD SkillExpertise nvarchar(max) NOT NULL DEFAULT ''
                END";
            command.ExecuteNonQuery();

            // Check if SavingThrowProficiencies column exists
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'PlayerCharacters')
                    AND name = 'SavingThrowProficiencies'
                )
                BEGIN
                    ALTER TABLE PlayerCharacters ADD SavingThrowProficiencies nvarchar(max) NOT NULL DEFAULT ''
                END";
            command.ExecuteNonQuery();

            // Check if Subclass column exists
            command.CommandText = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'PlayerCharacters')
                    AND name = 'Subclass'
                )
                BEGIN
                    ALTER TABLE PlayerCharacters ADD Subclass int NOT NULL DEFAULT 0
                END";
            command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }

    private static void CreateFeatureTablesIfNotExist(DnDContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();

            // Create Features table if it doesn't exist
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Features')
                BEGIN
                    CREATE TABLE Features (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Name nvarchar(200) NOT NULL,
                        Description nvarchar(max) NOT NULL,
                        Source int NOT NULL,
                        SourceName nvarchar(100) NOT NULL,
                        LevelRequirement int NOT NULL DEFAULT 1,
                        MaxUsesPerShortRest int NULL,
                        MaxUsesPerLongRest int NULL,
                        RechargeType int NOT NULL DEFAULT 0,
                        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
                    )
                END";
            command.ExecuteNonQuery();

            // Create CharacterFeatures table if it doesn't exist
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CharacterFeatures')
                BEGIN
                    CREATE TABLE CharacterFeatures (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        PlayerCharacterId int NOT NULL,
                        FeatureId int NOT NULL,
                        CurrentUses int NULL,
                        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT FK_CharacterFeatures_PlayerCharacters FOREIGN KEY (PlayerCharacterId)
                            REFERENCES PlayerCharacters(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_CharacterFeatures_Features FOREIGN KEY (FeatureId)
                            REFERENCES Features(Id) ON DELETE CASCADE
                    )
                END";
            command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }

    private static void CreateEquipmentTablesIfNotExist(DnDContext context)
    {
        var connection = context.Database.GetDbConnection();
        connection.Open();

        try
        {
            using var command = connection.CreateCommand();

            // Create Equipment table if it doesn't exist
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Equipment')
                BEGIN
                    CREATE TABLE Equipment (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        Name nvarchar(200) NOT NULL,
                        Description nvarchar(max) NOT NULL,
                        Type int NOT NULL,
                        IsStandard bit NOT NULL DEFAULT 1,
                        CostInGold decimal(10,2) NOT NULL DEFAULT 0,
                        Weight decimal(10,2) NOT NULL DEFAULT 0,

                        -- Weapon properties
                        WeaponCategory int NULL,
                        Damage nvarchar(50) NULL,
                        DamageType nvarchar(50) NULL,
                        IsFinesse bit NOT NULL DEFAULT 0,
                        IsVersatile bit NOT NULL DEFAULT 0,
                        VersatileDamage nvarchar(50) NULL,
                        Range nvarchar(50) NULL,

                        -- Armor properties
                        ArmorCategory int NULL,
                        ArmorClass int NULL,
                        AddDexModifier bit NULL,
                        MaxDexModifier int NULL,
                        StealthDisadvantage bit NOT NULL DEFAULT 0,
                        StrengthRequirement int NULL,

                        -- Magic item properties
                        Rarity int NULL,
                        RequiresAttunement bit NOT NULL DEFAULT 0,
                        MaxCharges int NULL,
                        ChargeRegeneration nvarchar(200) NULL,

                        -- Additional properties
                        PropertiesJson nvarchar(max) NULL,

                        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
                    )
                END";
            command.ExecuteNonQuery();

            // Create CharacterEquipment table if it doesn't exist
            command.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CharacterEquipment')
                BEGIN
                    CREATE TABLE CharacterEquipment (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        PlayerCharacterId int NOT NULL,
                        EquipmentId int NOT NULL,
                        Quantity int NOT NULL DEFAULT 1,
                        IsEquipped bit NOT NULL DEFAULT 0,
                        IsAttuned bit NOT NULL DEFAULT 0,
                        CurrentCharges int NULL,
                        Notes nvarchar(max) NULL,
                        CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
                        CONSTRAINT FK_CharacterEquipment_PlayerCharacters FOREIGN KEY (PlayerCharacterId)
                            REFERENCES PlayerCharacters(Id) ON DELETE CASCADE,
                        CONSTRAINT FK_CharacterEquipment_Equipment FOREIGN KEY (EquipmentId)
                            REFERENCES Equipment(Id) ON DELETE CASCADE
                    )
                END";
            command.ExecuteNonQuery();
        }
        finally
        {
            connection.Close();
        }
    }
}

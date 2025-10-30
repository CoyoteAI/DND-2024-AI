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
        }
        finally
        {
            connection.Close();
        }
    }
}

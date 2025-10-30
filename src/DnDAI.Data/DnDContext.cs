using Microsoft.EntityFrameworkCore;
using DnDAI.Core.Models;

namespace DnDAI.Data;

public class DnDContext : DbContext
{
    public DnDContext(DbContextOptions<DnDContext> options) : base(options)
    {
    }

    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<NPC> NPCs { get; set; }
    public DbSet<PlayerCharacter> PlayerCharacters { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<ConversationMessage> ConversationMessages { get; set; }
    public DbSet<Quest> Quests { get; set; }
    public DbSet<CombatEncounter> CombatEncounters { get; set; }
    public DbSet<Combatant> Combatants { get; set; }
    public DbSet<StatusEffect> StatusEffects { get; set; }
    public DbSet<CustomWeapon> CustomWeapons { get; set; }
    public DbSet<WeaponAbility> WeaponAbilities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Campaign configuration
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Setting).HasMaxLength(1000);

            entity.HasOne(e => e.CurrentLocation)
                .WithMany()
                .HasForeignKey(e => e.CurrentLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // NPC configuration
        modelBuilder.Entity<NPC>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Appearance).HasMaxLength(1000);
            entity.Property(e => e.Personality).HasMaxLength(1000);
            entity.Property(e => e.Backstory).HasMaxLength(2000);
            entity.Property(e => e.Motivation).HasMaxLength(1000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.NPCs)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CurrentLocation)
                .WithMany(l => l.NPCs)
                .HasForeignKey(e => e.CurrentLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PlayerCharacter configuration
        modelBuilder.Entity<PlayerCharacter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PlayerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Background).HasMaxLength(500);
            entity.Property(e => e.Backstory).HasMaxLength(2000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.PlayerCharacters)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Location configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Geography).HasMaxLength(1000);
            entity.Property(e => e.Climate).HasMaxLength(500);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Locations)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentLocation)
                .WithMany(l => l.SubLocations)
                .HasForeignKey(e => e.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Outcome).HasMaxLength(2000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Summary).HasMaxLength(2000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Sessions)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ConversationMessage configuration
        modelBuilder.Entity<ConversationMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Speaker).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();

            entity.HasOne(e => e.Session)
                .WithMany(s => s.ConversationMessages)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Quest configuration
        modelBuilder.Entity<Quest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.Quests)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CombatEncounter configuration
        modelBuilder.Entity<CombatEncounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Campaign)
                .WithMany(c => c.CombatEncounters)
                .HasForeignKey(e => e.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Session)
                .WithMany()
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Combatant configuration
        modelBuilder.Entity<Combatant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.CombatEncounter)
                .WithMany(ce => ce.Combatants)
                .HasForeignKey(e => e.CombatEncounterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PlayerCharacter)
                .WithMany()
                .HasForeignKey(e => e.PlayerCharacterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.NPC)
                .WithMany()
                .HasForeignKey(e => e.NPCId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StatusEffect configuration
        modelBuilder.Entity<StatusEffect>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(e => e.Combatant)
                .WithMany(c => c.StatusEffects)
                .HasForeignKey(e => e.CombatantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CustomWeapon configuration
        modelBuilder.Entity<CustomWeapon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BaseWeaponType).HasMaxLength(100);
            entity.Property(e => e.DamageDice).HasMaxLength(50);
            entity.Property(e => e.VersatileDamageDice).HasMaxLength(50);

            entity.HasOne(e => e.PlayerCharacter)
                .WithMany()
                .HasForeignKey(e => e.PlayerCharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WeaponAbility configuration
        modelBuilder.Entity<WeaponAbility>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DiceRoll).HasMaxLength(50);
            entity.Property(e => e.DamageType).HasMaxLength(50);
            entity.Property(e => e.UsageLimit).HasMaxLength(100);
            entity.Property(e => e.ActionType).HasMaxLength(50);

            entity.HasOne(e => e.CustomWeapon)
                .WithMany(w => w.SpecialAbilities)
                .HasForeignKey(e => e.CustomWeaponId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

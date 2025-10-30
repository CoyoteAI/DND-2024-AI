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
    public DbSet<Spell> Spells { get; set; }
    public DbSet<PlayerCharacterSpell> PlayerCharacterSpells { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<CharacterFeature> CharacterFeatures { get; set; }

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
                .OnDelete(DeleteBehavior.Restrict);
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
                .OnDelete(DeleteBehavior.Restrict);
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
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.Events)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure many-to-many relationships to avoid cascade conflicts
            entity.HasMany(e => e.NPCs)
                .WithMany(n => n.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventNPC",
                    j => j.HasOne<NPC>()
                        .WithMany()
                        .HasForeignKey("NPCsId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Event>()
                        .WithMany()
                        .HasForeignKey("EventsId")
                        .OnDelete(DeleteBehavior.Cascade));

            entity.HasMany(e => e.PlayerCharacters)
                .WithMany(pc => pc.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventPlayerCharacter",
                    j => j.HasOne<PlayerCharacter>()
                        .WithMany()
                        .HasForeignKey("PlayerCharactersId")
                        .OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Event>()
                        .WithMany()
                        .HasForeignKey("EventsId")
                        .OnDelete(DeleteBehavior.Cascade));
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
                .OnDelete(DeleteBehavior.Restrict);
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
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NPC)
                .WithMany()
                .HasForeignKey(e => e.NPCId)
                .OnDelete(DeleteBehavior.Restrict);
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

        // Spell configuration
        modelBuilder.Entity<Spell>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.School).HasMaxLength(50);
            entity.Property(e => e.CastingTime).HasMaxLength(100);
            entity.Property(e => e.Range).HasMaxLength(100);
            entity.Property(e => e.Duration).HasMaxLength(100);
            entity.Property(e => e.MaterialComponents).HasMaxLength(500);
            entity.Property(e => e.DamageType).HasMaxLength(50);
            entity.Property(e => e.SaveType).HasMaxLength(50);
            entity.Property(e => e.AttackType).HasMaxLength(50);
            entity.Property(e => e.Source).HasMaxLength(100);
        });

        // PlayerCharacterSpell configuration
        modelBuilder.Entity<PlayerCharacterSpell>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.PlayerCharacter)
                .WithMany(pc => pc.PlayerCharacterSpells)
                .HasForeignKey(e => e.PlayerCharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Spell)
                .WithMany(s => s.PlayerCharacterSpells)
                .HasForeignKey(e => e.SpellId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevent duplicate spell entries for same character
            entity.HasIndex(e => new { e.PlayerCharacterId, e.SpellId }).IsUnique();
        });
    }
}

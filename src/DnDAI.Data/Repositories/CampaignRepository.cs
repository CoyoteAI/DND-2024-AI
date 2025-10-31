using Microsoft.EntityFrameworkCore;
using DnDAI.Core.Models;

namespace DnDAI.Data.Repositories;

public class CampaignRepository : Repository<Campaign>
{
    public CampaignRepository(DnDContext context) : base(context)
    {
    }

    public async Task<Campaign?> GetCampaignWithDetailsAsync(int campaignId)
    {
        return await _dbSet
            .Include(c => c.CurrentLocation)
            .Include(c => c.PlayerCharacters)
            .Include(c => c.NPCs)
            .Include(c => c.Locations)
            .Include(c => c.Quests)
            .FirstOrDefaultAsync(c => c.Id == campaignId);
    }

    public async Task<Campaign?> GetActiveCampaignAsync()
    {
        return await _dbSet
            .Include(c => c.CurrentLocation)
            .FirstOrDefaultAsync(c => c.IsActive);
    }

    public async Task<IEnumerable<Campaign>> GetAllCampaignsWithBasicInfoAsync()
    {
        return await _dbSet
            .Include(c => c.PlayerCharacters)
            .ToListAsync();
    }

    public async Task<bool> DeleteCampaignWithRelatedDataAsync(int campaignId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var campaign = await _dbSet.FindAsync(campaignId);
            if (campaign == null)
                return false;

            // Step 1: Delete all Combatants for this campaign's CombatEncounters
            // (removes Restrict references to PlayerCharacters and NPCs)
            var combatEncounterIds = await _context.CombatEncounters
                .Where(ce => ce.CampaignId == campaignId)
                .Select(ce => ce.Id)
                .ToListAsync();

            var combatants = await _context.Combatants
                .Where(c => combatEncounterIds.Contains(c.CombatEncounterId))
                .ToListAsync();
            _context.Combatants.RemoveRange(combatants);
            await _context.SaveChangesAsync();

            // Step 2: Delete all Events for this campaign
            // (removes Restrict references to Sessions and Locations)
            var events = await _context.Events
                .Where(e => e.CampaignId == campaignId)
                .ToListAsync();
            _context.Events.RemoveRange(events);
            await _context.SaveChangesAsync();

            // Step 3: Clear CombatEncounter.SessionId
            // (removes Restrict reference to Sessions)
            var combatEncounters = await _context.CombatEncounters
                .Where(ce => ce.CampaignId == campaignId)
                .ToListAsync();
            foreach (var encounter in combatEncounters)
            {
                encounter.SessionId = null;
            }
            await _context.SaveChangesAsync();

            // Step 4: Clear NPC.CurrentLocationId
            // (removes Restrict references to Locations)
            var npcs = await _context.NPCs
                .Where(n => n.CampaignId == campaignId)
                .ToListAsync();
            foreach (var npc in npcs)
            {
                npc.CurrentLocationId = null;
            }
            await _context.SaveChangesAsync();

            // Step 5: Clear Location.ParentLocationId
            // (removes self-referential Restrict constraints)
            var locations = await _context.Locations
                .Where(l => l.CampaignId == campaignId)
                .ToListAsync();
            foreach (var location in locations)
            {
                location.ParentLocationId = null;
            }
            await _context.SaveChangesAsync();

            // Step 6: Clear Campaign.CurrentLocationId
            campaign.CurrentLocationId = null;
            await _context.SaveChangesAsync();

            // Step 7: Delete the Campaign (cascade will handle the rest)
            _dbSet.Remove(campaign);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

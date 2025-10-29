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
}

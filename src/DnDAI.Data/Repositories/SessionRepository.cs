using Microsoft.EntityFrameworkCore;
using DnDAI.Core.Models;

namespace DnDAI.Data.Repositories;

public class SessionRepository : Repository<Session>
{
    public SessionRepository(DnDContext context) : base(context)
    {
    }

    public async Task<Session?> GetSessionWithMessagesAsync(int sessionId)
    {
        return await _dbSet
            .Include(s => s.ConversationMessages.OrderBy(m => m.Timestamp))
            .Include(s => s.Events)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<Session?> GetLatestSessionForCampaignAsync(int campaignId)
    {
        return await _dbSet
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Session>> GetSessionsForCampaignAsync(int campaignId)
    {
        return await _dbSet
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.SessionNumber)
            .ToListAsync();
    }
}

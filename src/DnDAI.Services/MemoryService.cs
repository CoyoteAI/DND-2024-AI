using System.Text;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Data.Repositories;

namespace DnDAI.Services;

public class MemoryService : IMemoryService
{
    private readonly CampaignRepository _campaignRepository;
    private readonly IRepository<NPC> _npcRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<PlayerCharacter> _pcRepository;
    private readonly IRepository<Quest> _questRepository;
    private readonly SessionRepository _sessionRepository;

    public MemoryService(
        CampaignRepository campaignRepository,
        IRepository<NPC> npcRepository,
        IRepository<Location> locationRepository,
        IRepository<Event> eventRepository,
        IRepository<PlayerCharacter> pcRepository,
        IRepository<Quest> questRepository,
        SessionRepository sessionRepository)
    {
        _campaignRepository = campaignRepository;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _eventRepository = eventRepository;
        _pcRepository = pcRepository;
        _questRepository = questRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<string> BuildContextAsync(int campaignId, int? sessionId = null)
    {
        var sb = new StringBuilder();

        // Get campaign info
        var campaign = await _campaignRepository.GetCampaignWithDetailsAsync(campaignId);
        if (campaign == null)
        {
            return "No campaign found.";
        }

        sb.AppendLine($"CAMPAIGN: {campaign.Name}");
        sb.AppendLine($"Setting: {campaign.Setting}");
        sb.AppendLine($"Description: {campaign.Description}");
        sb.AppendLine();

        // Current location
        if (campaign.CurrentLocation != null)
        {
            sb.AppendLine($"CURRENT LOCATION: {campaign.CurrentLocation.Name}");
            sb.AppendLine($"Type: {campaign.CurrentLocation.Type}");
            sb.AppendLine($"Description: {campaign.CurrentLocation.Description}");
            sb.AppendLine();
        }

        // Player characters
        var pcs = await _pcRepository.FindAsync(pc => pc.CampaignId == campaignId && pc.IsAlive);
        if (pcs.Any())
        {
            sb.AppendLine("PLAYER CHARACTERS:");
            foreach (var pc in pcs)
            {
                sb.AppendLine($"- {pc.Name} ({pc.Race} {pc.Class}, Level {pc.Level})");
                sb.AppendLine($"  HP: {pc.CurrentHitPoints}/{pc.MaxHitPoints}");
                if (!string.IsNullOrEmpty(pc.Backstory))
                {
                    sb.AppendLine($"  Background: {pc.Backstory}");
                }
            }
            sb.AppendLine();
        }

        // Active quests
        var quests = await _questRepository.FindAsync(q => q.CampaignId == campaignId && q.IsActive && !q.IsCompleted);
        if (quests.Any())
        {
            sb.AppendLine("ACTIVE QUESTS:");
            foreach (var quest in quests)
            {
                sb.AppendLine($"- {quest.Title}");
                sb.AppendLine($"  {quest.Description}");
            }
            sb.AppendLine();
        }

        // Recent significant events
        var recentEvents = (await _eventRepository.FindAsync(e => e.CampaignId == campaignId && e.IsSignificant))
            .OrderByDescending(e => e.EventDate)
            .Take(10);

        if (recentEvents.Any())
        {
            sb.AppendLine("SIGNIFICANT RECENT EVENTS:");
            foreach (var evt in recentEvents)
            {
                sb.AppendLine($"- {evt.Title} ({evt.Type})");
                sb.AppendLine($"  {evt.Description}");
                if (!string.IsNullOrEmpty(evt.Outcome))
                {
                    sb.AppendLine($"  Outcome: {evt.Outcome}");
                }
            }
            sb.AppendLine();
        }

        // Notable NPCs
        var npcs = (await _npcRepository.FindAsync(n => n.CampaignId == campaignId && n.IsAlive))
            .Take(20);

        if (npcs.Any())
        {
            sb.AppendLine("NOTABLE NPCs:");
            foreach (var npc in npcs)
            {
                sb.AppendLine($"- {npc.Name} ({npc.Race})");
                if (!string.IsNullOrEmpty(npc.Personality))
                {
                    sb.AppendLine($"  Personality: {npc.Personality}");
                }
                if (npc.CurrentLocation != null)
                {
                    sb.AppendLine($"  Location: {npc.CurrentLocation.Name}");
                }
            }
            sb.AppendLine();
        }

        // Recent conversation if session provided
        if (sessionId.HasValue)
        {
            var conversation = await GetRecentConversationAsync(sessionId.Value, 10);
            if (!string.IsNullOrEmpty(conversation))
            {
                sb.AppendLine("RECENT CONVERSATION:");
                sb.AppendLine(conversation);
            }
        }

        return sb.ToString();
    }

    public async Task<string> GetRelevantNPCsAsync(int campaignId, string query)
    {
        var npcs = await _npcRepository.FindAsync(n => n.CampaignId == campaignId && n.IsAlive);

        // Simple keyword matching - could be enhanced with better search
        var relevant = npcs.Where(n =>
            n.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            n.Personality.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            n.Backstory.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(5);

        var sb = new StringBuilder();
        foreach (var npc in relevant)
        {
            sb.AppendLine($"- {npc.Name}: {npc.Personality}");
        }

        return sb.ToString();
    }

    public async Task<string> GetRelevantLocationsAsync(int campaignId, string query)
    {
        var locations = await _locationRepository.FindAsync(l => l.CampaignId == campaignId);

        var relevant = locations.Where(l =>
            l.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            l.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(5);

        var sb = new StringBuilder();
        foreach (var location in relevant)
        {
            sb.AppendLine($"- {location.Name} ({location.Type}): {location.Description}");
        }

        return sb.ToString();
    }

    public async Task<string> GetRelevantEventsAsync(int campaignId, int limit = 10)
    {
        var events = (await _eventRepository.FindAsync(e => e.CampaignId == campaignId))
            .OrderByDescending(e => e.EventDate)
            .Take(limit);

        var sb = new StringBuilder();
        foreach (var evt in events)
        {
            sb.AppendLine($"- {evt.Title}: {evt.Description}");
        }

        return sb.ToString();
    }

    public async Task<string> GetRecentConversationAsync(int sessionId, int messageCount = 20)
    {
        var session = await _sessionRepository.GetSessionWithMessagesAsync(sessionId);
        if (session == null)
            return string.Empty;

        var messages = session.ConversationMessages
            .OrderByDescending(m => m.Timestamp)
            .Take(messageCount)
            .OrderBy(m => m.Timestamp);

        var sb = new StringBuilder();
        foreach (var msg in messages)
        {
            sb.AppendLine($"{msg.Speaker}: {msg.Message}");
        }

        return sb.ToString();
    }
}

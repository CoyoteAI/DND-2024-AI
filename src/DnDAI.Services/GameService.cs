using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Data.Repositories;

namespace DnDAI.Services;

public class GameService
{
    private readonly ILLMService _llmService;
    private readonly CampaignRepository _campaignRepository;
    private readonly SessionRepository _sessionRepository;
    private readonly IRepository<ConversationMessage> _messageRepository;
    private readonly IRepository<NPC> _npcRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Quest> _questRepository;

    public GameService(
        ILLMService llmService,
        CampaignRepository campaignRepository,
        SessionRepository sessionRepository,
        IRepository<ConversationMessage> messageRepository,
        IRepository<NPC> npcRepository,
        IRepository<Location> locationRepository,
        IRepository<Event> eventRepository,
        IRepository<Quest> questRepository)
    {
        _llmService = llmService;
        _campaignRepository = campaignRepository;
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _eventRepository = eventRepository;
        _questRepository = questRepository;
    }

    public async Task<Campaign> CreateCampaignAsync(string name, string description, string setting)
    {
        var campaign = new Campaign
        {
            Name = name,
            Description = description,
            Setting = setting,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };

        return await _campaignRepository.AddAsync(campaign);
    }

    public async Task<Session> StartSessionAsync(int campaignId)
    {
        var sessions = await _sessionRepository.GetSessionsForCampaignAsync(campaignId);
        var sessionNumber = sessions.Any() ? sessions.Max(s => s.SessionNumber) + 1 : 1;

        var session = new Session
        {
            CampaignId = campaignId,
            SessionNumber = sessionNumber,
            StartTime = DateTime.UtcNow
        };

        return await _sessionRepository.AddAsync(session);
    }

    public async Task<Session> EndSessionAsync(int sessionId, string summary)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Session not found");

        session.EndTime = DateTime.UtcNow;
        session.Summary = summary;

        return await _sessionRepository.UpdateAsync(session);
    }

    public async Task<string> ProcessPlayerInputAsync(int campaignId, int sessionId, string playerInput, string? playerName = null)
    {
        // Save player message
        await SaveMessageAsync(sessionId, playerName ?? "Player", playerInput);

        // Get DM response from LLM
        var dmResponse = await _llmService.GenerateWithMemoryAsync(campaignId, playerInput);

        // Save DM message
        await SaveMessageAsync(sessionId, "DM", dmResponse);

        return dmResponse;
    }

    public async Task<ConversationMessage> SaveMessageAsync(int sessionId, string speaker, string message, string messageType = "Normal")
    {
        var conversationMessage = new ConversationMessage
        {
            SessionId = sessionId,
            Speaker = speaker,
            Message = message,
            MessageType = messageType,
            Timestamp = DateTime.UtcNow
        };

        return await _messageRepository.AddAsync(conversationMessage);
    }

    public async Task<NPC> CreateNPCAsync(int campaignId, NPC npc)
    {
        npc.CampaignId = campaignId;
        return await _npcRepository.AddAsync(npc);
    }

    public async Task<Location> CreateLocationAsync(int campaignId, Location location)
    {
        location.CampaignId = campaignId;
        return await _locationRepository.AddAsync(location);
    }

    public async Task<Event> CreateEventAsync(int campaignId, Event gameEvent)
    {
        gameEvent.CampaignId = campaignId;
        return await _eventRepository.AddAsync(gameEvent);
    }

    public async Task<Quest> CreateQuestAsync(int campaignId, Quest quest)
    {
        quest.CampaignId = campaignId;
        return await _questRepository.AddAsync(quest);
    }

    public async Task<Campaign?> GetCampaignAsync(int campaignId)
    {
        return await _campaignRepository.GetCampaignWithDetailsAsync(campaignId);
    }

    public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync()
    {
        return await _campaignRepository.GetAllCampaignsWithBasicInfoAsync();
    }

    public async Task<Session?> GetSessionAsync(int sessionId)
    {
        return await _sessionRepository.GetSessionWithMessagesAsync(sessionId);
    }

    public async Task<IEnumerable<Session>> GetCampaignSessionsAsync(int campaignId)
    {
        return await _sessionRepository.GetSessionsForCampaignAsync(campaignId);
    }

    public async Task<IEnumerable<NPC>> GetCampaignNPCsAsync(int campaignId)
    {
        return await _npcRepository.FindAsync(n => n.CampaignId == campaignId);
    }

    public async Task<IEnumerable<Location>> GetCampaignLocationsAsync(int campaignId)
    {
        return await _locationRepository.FindAsync(l => l.CampaignId == campaignId);
    }

    public async Task<IEnumerable<Quest>> GetActiveQuestsAsync(int campaignId)
    {
        return await _questRepository.FindAsync(q => q.CampaignId == campaignId && q.IsActive && !q.IsCompleted);
    }
}

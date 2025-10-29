namespace DnDAI.Core.Interfaces;

public interface IMemoryService
{
    Task<string> BuildContextAsync(int campaignId, int? sessionId = null);
    Task<string> GetRelevantNPCsAsync(int campaignId, string query);
    Task<string> GetRelevantLocationsAsync(int campaignId, string query);
    Task<string> GetRelevantEventsAsync(int campaignId, int limit = 10);
    Task<string> GetRecentConversationAsync(int sessionId, int messageCount = 20);
}

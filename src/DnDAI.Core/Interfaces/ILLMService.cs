namespace DnDAI.Core.Interfaces;

public interface ILLMService
{
    Task<string> GenerateResponseAsync(string prompt, string context);
    Task<string> GenerateWithMemoryAsync(int campaignId, string userInput);
}

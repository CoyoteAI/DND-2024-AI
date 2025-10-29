using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using DnDAI.Core.Interfaces;
using DnDAI.Services.Configuration;
using DnDAI.Services.Models;

namespace DnDAI.Services;

public class QwenLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly QwenSettings _settings;
    private readonly IMemoryService _memoryService;

    public QwenLLMService(
        HttpClient httpClient,
        IOptions<QwenSettings> settings,
        IMemoryService memoryService)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _memoryService = memoryService;
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<string> GenerateResponseAsync(string prompt, string context)
    {
        try
        {
            var fullPrompt = BuildPrompt(prompt, context);

            var request = new QwenRequest
            {
                Model = _settings.ModelName,
                Prompt = fullPrompt,
                Stream = false,
                Options = new QwenOptions
                {
                    Temperature = _settings.Temperature,
                    NumPredict = _settings.MaxTokens
                }
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/api/generate", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var qwenResponse = JsonConvert.DeserializeObject<QwenResponse>(responseJson);

            return qwenResponse?.Response ?? "I apologize, but I couldn't generate a response.";
        }
        catch (Exception ex)
        {
            return $"Error communicating with Qwen: {ex.Message}";
        }
    }

    public async Task<string> GenerateWithMemoryAsync(int campaignId, string userInput)
    {
        // Build context from memory
        var context = await _memoryService.BuildContextAsync(campaignId);

        // Create DM prompt
        var prompt = $"Player: {userInput}";

        return await GenerateResponseAsync(prompt, context);
    }

    private string BuildPrompt(string userMessage, string context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert Dungeon Master running a Dungeons & Dragons 2024 campaign.");
        sb.AppendLine("You are creative, engaging, and maintain consistency with the game world and rules.");
        sb.AppendLine();
        sb.AppendLine("=== CAMPAIGN MEMORY ===");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine("=== CURRENT INTERACTION ===");
        sb.AppendLine(userMessage);
        sb.AppendLine();
        sb.AppendLine("Respond as the Dungeon Master. Be descriptive, immersive, and consistent with the established world.");
        sb.AppendLine("Update the game state as needed (create NPCs, locations, events) based on the narrative.");

        return sb.ToString();
    }
}

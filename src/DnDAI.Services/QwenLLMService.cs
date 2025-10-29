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
        sb.AppendLine("=== COMBAT COMMANDS ===");
        sb.AppendLine("You can control combat automatically using these commands (embed them in your narrative):");
        sb.AppendLine("[COMBAT_START: Encounter Name] - Start a combat encounter");
        sb.AppendLine("[ADD_MONSTER: Name, HP:X, AC:Y, Init:+Z] - Add a monster (e.g., [ADD_MONSTER: Goblin, HP:7, AC:15, Init:+2])");
        sb.AppendLine("[ADD_PC: CharacterName] - Add a player character to combat");
        sb.AppendLine("[ROLL_INITIATIVE] - Roll initiative for all combatants");
        sb.AppendLine("[DAMAGE: Name, Amount] - Apply damage (e.g., [DAMAGE: Goblin, 8])");
        sb.AppendLine("[HEAL: Name, Amount] - Heal a combatant");
        sb.AppendLine("[TEMP_HP: Name, Amount] - Grant temporary HP (e.g., [TEMP_HP: Fighter, 10])");
        sb.AppendLine("[REDUCE_MAX_HP: Name, Amount] - Reduce max HP (undead attacks, life drain)");
        sb.AppendLine("[RESTORE_MAX_HP: Name] - Restore max HP to original");
        sb.AppendLine("[NEXT_TURN] - Advance to next turn");
        sb.AppendLine("[COMBAT_END] - End combat");
        sb.AppendLine();
        sb.AppendLine("Example: \"Three goblins jump out! [COMBAT_START: Goblin Ambush] [ADD_MONSTER: Goblin 1, HP:7, AC:15, Init:+2] [ADD_MONSTER: Goblin 2, HP:7, AC:15, Init:+2] [ADD_MONSTER: Goblin 3, HP:7, AC:15, Init:+2] [ROLL_INITIATIVE] Roll for initiative!\"");
        sb.AppendLine();
        sb.AppendLine("When players describe attacks and roll damage, apply it: \"Your sword strikes true! [DAMAGE: Goblin 1, 8] The goblin staggers.\"");
        sb.AppendLine("For temporary HP buffs: \"The spell fills you with vigor! [TEMP_HP: Fighter, 10] You feel tougher.\"");
        sb.AppendLine("For undead attacks reducing max HP: \"The wraith drains your life force! [REDUCE_MAX_HP: PlayerName, 5] You feel weakened.\"");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT: Let players roll their own dice for attacks and damage. You handle the results and tracking.");
        sb.AppendLine();
        sb.AppendLine("=== CAMPAIGN MEMORY ===");
        sb.AppendLine(context);
        sb.AppendLine();
        sb.AppendLine("=== CURRENT INTERACTION ===");
        sb.AppendLine(userMessage);
        sb.AppendLine();
        sb.AppendLine("Respond as the Dungeon Master. Be descriptive, immersive, and consistent with the established world.");
        sb.AppendLine("Use combat commands to automatically manage combat encounters. The player will roll their own dice.");

        return sb.ToString();
    }
}

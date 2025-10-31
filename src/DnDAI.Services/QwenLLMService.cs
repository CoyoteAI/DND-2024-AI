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

    private void Log(string message)
    {
        var fullMsg = $"[Qwen] {message}";
        System.Diagnostics.Debug.WriteLine(fullMsg);
        Console.WriteLine(fullMsg);
    }

    private string CleanThinkingText(string thinkingText)
    {
        // The "thinking" field contains meta-planning and the actual narrative is buried within
        // Strategy: Look for quoted text or the last substantial narrative paragraph

        Log($"Raw thinking text length: {thinkingText.Length}");

        // Strategy 1: Extract text in quotes (e.g., "Good evening...")
        var quotePattern = @"""([^""]{50,})""";
        var quoteMatches = System.Text.RegularExpressions.Regex.Matches(thinkingText, quotePattern);

        if (quoteMatches.Count > 0)
        {
            // Prefer the LONGEST quoted section that looks like narrative (not meta-planning)
            // Meta-planning indicators (expanded list)
            var metaPhrases = new[] {
                "I'll ", "Let's ", "We can ", "Idea:", "Note:", "Campaign:", "Example narrative:",
                "The player has", "The player's", "campaign set in", "The character",
                "level ", "HP:", "/", "message is", "text is", "input is"
            };

            // First pass: Find longest quote WITHOUT meta phrases and >300 chars
            var validQuotes = quoteMatches.Cast<System.Text.RegularExpressions.Match>()
                .Select((m, i) => new { Quote = m.Groups[1].Value, Index = i })
                .Where(q => {
                    bool hasMeta = metaPhrases.Any(p => q.Quote.Contains(p, StringComparison.OrdinalIgnoreCase));
                    return !hasMeta && q.Quote.Length > 300;
                })
                .OrderByDescending(q => q.Quote.Length)
                .ToList();

            if (validQuotes.Any())
            {
                var best = validQuotes.First();
                Log($"Found clean quoted narrative (#{best.Index}): {best.Quote.Length} chars");
                return best.Quote.Trim();
            }

            // Second pass: If no long clean quotes, find longest quote without meta (>100 chars)
            validQuotes = quoteMatches.Cast<System.Text.RegularExpressions.Match>()
                .Select((m, i) => new { Quote = m.Groups[1].Value, Index = i })
                .Where(q => {
                    bool hasMeta = metaPhrases.Any(p => q.Quote.Contains(p, StringComparison.OrdinalIgnoreCase));
                    return !hasMeta && q.Quote.Length > 100;
                })
                .OrderByDescending(q => q.Quote.Length)
                .ToList();

            if (validQuotes.Any())
            {
                var best = validQuotes.First();
                Log($"Found quoted narrative (#{best.Index}): {best.Quote.Length} chars");
                return best.Quote.Trim();
            }

            // Fallback: use the longest quote regardless
            var longestQuote = quoteMatches.Cast<System.Text.RegularExpressions.Match>()
                .OrderByDescending(m => m.Groups[1].Value.Length)
                .First()
                .Groups[1].Value;
            Log($"Using longest quoted section as fallback: {longestQuote.Length} chars");
            return longestQuote.Trim();
        }

        // Strategy 2: Look for "Let me draft:" or similar markers and take everything after
        var draftMarkers = new[] { "Let me draft:", "I'll write:", "Example response:", "Draft:" };
        foreach (var marker in draftMarkers)
        {
            var markerIndex = thinkingText.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                var afterMarker = thinkingText.Substring(markerIndex + marker.Length).Trim();
                // Take first substantial paragraph after marker
                var firstParagraph = afterMarker.Split(new[] { "\n\n" }, StringSplitOptions.None)[0].Trim();
                if (firstParagraph.Length > 50)
                {
                    // Remove any leading quotes
                    firstParagraph = firstParagraph.Trim('"', ' ');
                    Log($"Found narrative after '{marker}': {firstParagraph.Length} chars");
                    return firstParagraph;
                }
            }
        }

        // Strategy 3: Find last paragraph that looks like narrative (starts with capital, >50 chars)
        var paragraphs = thinkingText.Split(new[] { "\n\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = paragraphs.Length - 1; i >= 0; i--)
        {
            var para = paragraphs[i].Trim();
            if (para.Length > 50 &&
                char.IsUpper(para[0]) &&
                !para.Contains("I'll") &&
                !para.Contains("Let me") &&
                !para.Contains("the player") &&
                !para.Contains("Example:"))
            {
                Log($"Found narrative paragraph at end: {para.Length} chars");
                return para;
            }
        }

        // Fallback: return everything (better to show too much than nothing)
        Log("No narrative found, returning full thinking text");
        return thinkingText;
    }

    public QwenLLMService(
        HttpClient httpClient,
        IOptions<QwenSettings> settings,
        IMemoryService memoryService)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _memoryService = memoryService;
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Startup diagnostic to verify new code is loaded
        Log($"QwenLLMService initialized - Model: {_settings.ModelName}, URL: {_settings.ApiUrl}");
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

            Log($"Sending request to: {_settings.ApiUrl}/api/generate");
            Log($"Model: {_settings.ModelName}");
            Log($"Prompt length: {fullPrompt.Length} characters");

            var response = await _httpClient.PostAsync($"{_settings.ApiUrl}/api/generate", content);

            Log($"Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Log($"Error response: {errorContent}");
                return $"Ollama API Error ({response.StatusCode}): {errorContent}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            Log($"Response length: {responseJson.Length} characters");
            Log($"Raw response: {responseJson.Substring(0, Math.Min(500, responseJson.Length))}...");

            var qwenResponse = JsonConvert.DeserializeObject<QwenResponse>(responseJson);

            if (qwenResponse == null)
            {
                Log("Failed to deserialize response");
                return "Error: Failed to parse Ollama response. Check the debug output.";
            }

            // Some models (like qwen3:4b) are inconsistent about where they put output:
            // - Sometimes everything in "thinking", response is empty
            // - Sometimes a SHORT snippet in "response", FULL text in "thinking"
            // Strategy: Prefer thinking field if it's substantial (>200 chars)
            string actualResponse;

            if (!string.IsNullOrEmpty(qwenResponse.Thinking) && qwenResponse.Thinking.Length > 200)
            {
                Log($"Using thinking field ({qwenResponse.Thinking.Length} chars) over response field ({qwenResponse.Response?.Length ?? 0} chars)");
                actualResponse = CleanThinkingText(qwenResponse.Thinking);
            }
            else if (!string.IsNullOrEmpty(qwenResponse.Response))
            {
                Log($"Using response field ({qwenResponse.Response.Length} chars)");
                actualResponse = qwenResponse.Response;
            }
            else
            {
                Log("Both response and thinking fields are empty or too short");
                return "Error: Ollama returned an empty response. This might mean:\n" +
                       "1. The model name is incorrect (check 'ollama list')\n" +
                       "2. The model needs to be pulled (run 'ollama pull " + _settings.ModelName + "')\n" +
                       "3. Ollama is having issues generating content";
            }

            Log($"Final response length: {actualResponse.Length} characters");
            return actualResponse;
        }
        catch (HttpRequestException ex)
        {
            Log($"HTTP Error: {ex.Message}");
            return $"Connection Error: Cannot reach Ollama at {_settings.ApiUrl}. Make sure Ollama is running.\n\nDetails: {ex.Message}";
        }
        catch (TaskCanceledException ex)
        {
            Log($"Timeout: {ex.Message}");
            return $"Timeout Error: Ollama took longer than {_settings.TimeoutSeconds} seconds to respond. The prompt might be too long or the model is slow.";
        }
        catch (Exception ex)
        {
            Log($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
            Log($"Stack trace: {ex.StackTrace}");
            return $"Unexpected Error: {ex.GetType().Name}\n{ex.Message}\n\nCheck the debug output window for details.";
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

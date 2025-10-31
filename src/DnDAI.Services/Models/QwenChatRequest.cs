using Newtonsoft.Json;

namespace DnDAI.Services.Models;

public class QwenChatRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("messages")]
    public List<QwenChatMessage> Messages { get; set; } = new();

    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;

    [JsonProperty("options")]
    public QwenOptions? Options { get; set; }
}

public class QwenChatMessage
{
    [JsonProperty("role")]
    public string Role { get; set; } = string.Empty; // "system", "user", or "assistant"

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}

public class QwenChatResponse
{
    [JsonProperty("model")]
    public string? Model { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("message")]
    public QwenChatMessage? Message { get; set; }

    [JsonProperty("done")]
    public bool Done { get; set; }

    [JsonProperty("done_reason")]
    public string? DoneReason { get; set; }

    [JsonProperty("total_duration")]
    public long TotalDuration { get; set; }

    [JsonProperty("load_duration")]
    public long LoadDuration { get; set; }

    [JsonProperty("prompt_eval_count")]
    public int PromptEvalCount { get; set; }

    [JsonProperty("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }

    [JsonProperty("eval_count")]
    public int EvalCount { get; set; }

    [JsonProperty("eval_duration")]
    public long EvalDuration { get; set; }
}

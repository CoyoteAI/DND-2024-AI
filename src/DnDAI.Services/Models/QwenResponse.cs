using Newtonsoft.Json;

namespace DnDAI.Services.Models;

public class QwenResponse
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonProperty("response")]
    public string Response { get; set; } = string.Empty;

    [JsonProperty("done")]
    public bool Done { get; set; }

    [JsonProperty("context")]
    public int[]? Context { get; set; }

    [JsonProperty("total_duration")]
    public long TotalDuration { get; set; }

    [JsonProperty("load_duration")]
    public long LoadDuration { get; set; }

    [JsonProperty("prompt_eval_duration")]
    public long PromptEvalDuration { get; set; }

    [JsonProperty("eval_duration")]
    public long EvalDuration { get; set; }
}

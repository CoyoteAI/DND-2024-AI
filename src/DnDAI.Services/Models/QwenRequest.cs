using Newtonsoft.Json;

namespace DnDAI.Services.Models;

public class QwenRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;

    [JsonProperty("options")]
    public QwenOptions? Options { get; set; }
}

public class QwenOptions
{
    [JsonProperty("temperature")]
    public double Temperature { get; set; } = 0.7;

    [JsonProperty("num_predict")]
    public int NumPredict { get; set; } = 2000;
}

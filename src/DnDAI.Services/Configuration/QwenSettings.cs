namespace DnDAI.Services.Configuration;

public class QwenSettings
{
    public string ApiUrl { get; set; } = "http://localhost:11434"; // Default Ollama URL
    public string ModelName { get; set; } = "qwen2.5:latest";
    public int MaxTokens { get; set; } = 2000;
    public double Temperature { get; set; } = 0.7;
    public int TimeoutSeconds { get; set; } = 120;
}

namespace DnDAI.Services.Configuration;

public class OllamaSettings
{
    public bool AutoStart { get; set; } = true;
    public bool AutoStop { get; set; } = true;
    public string OllamaPath { get; set; } = "ollama"; // Default assumes ollama is in PATH
    public int StartupTimeoutSeconds { get; set; } = 30;
}

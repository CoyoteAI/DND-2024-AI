namespace DnDAI.Core.Interfaces;

public interface IOllamaService
{
    Task<bool> IsRunningAsync();
    Task<bool> StartAsync();
    Task StopAsync();
    bool IsOllamaInstalled();
}

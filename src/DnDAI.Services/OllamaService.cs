using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DnDAI.Core.Interfaces;
using DnDAI.Services.Configuration;

namespace DnDAI.Services;

public class OllamaService : IOllamaService, IDisposable
{
    private readonly OllamaSettings _settings;
    private readonly QwenSettings _qwenSettings;
    private readonly ILogger<OllamaService>? _logger;
    private readonly HttpClient _httpClient;
    private Process? _ollamaProcess;
    private bool _isManaged = false;

    public OllamaService(
        IOptions<OllamaSettings> settings,
        IOptions<QwenSettings> qwenSettings,
        ILogger<OllamaService>? logger = null)
    {
        _settings = settings.Value;
        _qwenSettings = qwenSettings.Value;
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public bool IsOllamaInstalled()
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _settings.OllamaPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return false;

            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsRunningAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_qwenSettings.ApiUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> StartAsync()
    {
        _logger?.LogInformation("Starting Ollama service...");

        // Check if already running
        if (await IsRunningAsync())
        {
            _logger?.LogInformation("Ollama is already running.");
            return true;
        }

        // Check if Ollama is installed
        if (!IsOllamaInstalled())
        {
            _logger?.LogError("Ollama is not installed or not found in PATH.");
            return false;
        }

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _settings.OllamaPath,
                Arguments = "serve",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _ollamaProcess = Process.Start(processStartInfo);

            if (_ollamaProcess == null)
            {
                _logger?.LogError("Failed to start Ollama process.");
                return false;
            }

            _isManaged = true;

            // Wire up output handlers
            _ollamaProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger?.LogDebug($"Ollama: {e.Data}");
            };

            _ollamaProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger?.LogWarning($"Ollama Error: {e.Data}");
            };

            _ollamaProcess.BeginOutputReadLine();
            _ollamaProcess.BeginErrorReadLine();

            // Wait for Ollama to start and be ready
            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(_settings.StartupTimeoutSeconds);

            while (DateTime.UtcNow - startTime < timeout)
            {
                if (await IsRunningAsync())
                {
                    _logger?.LogInformation("Ollama service started successfully.");
                    return true;
                }

                await Task.Delay(500);
            }

            _logger?.LogError("Ollama service failed to start within timeout period.");
            await StopAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting Ollama service.");
            return false;
        }
    }

    public async Task StopAsync()
    {
        if (_ollamaProcess != null && _isManaged && !_ollamaProcess.HasExited)
        {
            _logger?.LogInformation("Stopping Ollama service...");

            try
            {
                _ollamaProcess.Kill(entireProcessTree: true);
                await Task.Run(() => _ollamaProcess.WaitForExit(5000));
                _logger?.LogInformation("Ollama service stopped.");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error stopping Ollama process.");
            }
            finally
            {
                _ollamaProcess?.Dispose();
                _ollamaProcess = null;
                _isManaged = false;
            }
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();

        // Stop Ollama if we started it
        if (_isManaged && _ollamaProcess != null && !_ollamaProcess.HasExited)
        {
            try
            {
                _ollamaProcess.Kill(entireProcessTree: true);
                _ollamaProcess.WaitForExit(5000);
            }
            catch
            {
                // Best effort cleanup
            }
        }

        _ollamaProcess?.Dispose();
    }
}

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Data;
using DnDAI.Data.Repositories;
using DnDAI.Services;
using DnDAI.Services.Configuration;

namespace DnDAI.Desktop;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.Configure<QwenSettings>(context.Configuration.GetSection("QwenSettings"));
                services.Configure<OllamaSettings>(context.Configuration.GetSection("OllamaSettings"));

                // Database
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=DnDAI;Trusted_Connection=True;MultipleActiveResultSets=true";

                services.AddDbContext<DnDContext>(options =>
                    options.UseSqlServer(connectionString));

                // Repositories
                services.AddScoped<CampaignRepository>();
                services.AddScoped<SessionRepository>();
                services.AddScoped<IRepository<NPC>, Repository<NPC>>();
                services.AddScoped<IRepository<PlayerCharacter>, Repository<PlayerCharacter>>();
                services.AddScoped<IRepository<Location>, Repository<Location>>();
                services.AddScoped<IRepository<Event>, Repository<Event>>();
                services.AddScoped<IRepository<Quest>, Repository<Quest>>();
                services.AddScoped<IRepository<CustomWeapon>, Repository<CustomWeapon>>();
                services.AddScoped<IRepository<Spell>, Repository<Spell>>();
                services.AddScoped<IRepository<PlayerCharacterSpell>, Repository<PlayerCharacterSpell>>();
                services.AddScoped<IRepository<ConversationMessage>, Repository<ConversationMessage>>();

                // Services
                services.AddSingleton<IOllamaService, OllamaService>();
                services.AddHttpClient<ILLMService, QwenLLMService>();
                services.AddScoped<IMemoryService, MemoryService>();
                services.AddScoped<IDiceRoller, DiceRoller>();
                services.AddScoped<ICombatService, CombatService>();
                services.AddScoped<GameService>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

        // Start Ollama service if configured
        StartOllamaServiceAsync().Wait();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // Ensure database is created
        using (var scope = _host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DnDContext>();
            dbContext.Database.EnsureCreated();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Stop Ollama service if we started it
        StopOllamaServiceAsync().Wait();

        _host?.Dispose();
        base.OnExit(e);
    }

    private async Task StartOllamaServiceAsync()
    {
        if (_host == null) return;

        var ollamaService = _host.Services.GetService<IOllamaService>();
        if (ollamaService == null) return;

        var ollamaSettings = _host.Services.GetService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>()?.Value;
        if (ollamaSettings?.AutoStart != true) return;

        try
        {
            // Check if Ollama is installed
            if (!ollamaService.IsOllamaInstalled())
            {
                MessageBox.Show(
                    "Ollama is not installed or not found.\n\n" +
                    "Please install Ollama from https://ollama.ai\n" +
                    "or ensure it's in your system PATH.\n\n" +
                    "The application will continue, but AI features will not work until Ollama is running.",
                    "Ollama Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Try to start Ollama
            var started = await ollamaService.StartAsync();

            if (!started)
            {
                MessageBox.Show(
                    "Failed to start Ollama service.\n\n" +
                    "You can try starting it manually by running 'ollama serve' in a terminal.\n\n" +
                    "The application will continue, but AI features may not work.",
                    "Ollama Startup Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error starting Ollama service: {ex.Message}\n\n" +
                "The application will continue, but AI features may not work.",
                "Ollama Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task StopOllamaServiceAsync()
    {
        if (_host == null) return;

        var ollamaService = _host.Services.GetService<IOllamaService>();
        if (ollamaService == null) return;

        var ollamaSettings = _host.Services.GetService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>()?.Value;
        if (ollamaSettings?.AutoStop != true) return;

        try
        {
            await ollamaService.StopAsync();
        }
        catch
        {
            // Best effort cleanup
        }
    }
}

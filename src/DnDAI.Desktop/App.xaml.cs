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
                services.AddScoped<IRepository<ConversationMessage>, Repository<ConversationMessage>>();

                // Services
                services.AddHttpClient<ILLMService, QwenLLMService>();
                services.AddScoped<IMemoryService, MemoryService>();
                services.AddScoped<IDiceRoller, DiceRoller>();
                services.AddScoped<GameService>();

                // Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();

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
        _host?.Dispose();
        base.OnExit(e);
    }
}

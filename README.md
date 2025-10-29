# D&D AI Dungeon Master

A C# WPF desktop application that uses an AI-powered Dungeon Master to run Dungeons & Dragons 2024 campaigns. The application maintains a SQL Server database to track NPCs, locations, events, quests, and all campaign details, ensuring continuity throughout your adventures.

## Features

- **AI-Powered Dungeon Master**: Uses Qwen LLM to generate immersive, contextual responses
- **Persistent Memory**: SQL Server database stores all campaign information
- **Campaign Management**: Create and manage multiple D&D campaigns
- **Session Tracking**: Start and end gaming sessions with conversation history
- **Dice Rolling System**: Built-in dice roller with support for all standard D&D dice (d4, d6, d8, d10, d12, d20, d100)
  - Quick-roll buttons for common dice
  - Custom dice expressions (e.g., "2d6+3", "1d20+5")
  - Advantage and Disadvantage rolls
  - Automatic logging to conversation history
- **NPC Management**: Track non-player characters with stats, personalities, and backstories
- **Location Tracking**: Maintain a detailed world with interconnected locations
- **Event History**: Record significant events and maintain narrative continuity
- **Quest System**: Track active and completed quests
- **Player Characters**: Manage player character stats, inventory, and progression

## Architecture

The application follows a clean architecture pattern with four main projects:

- **DnDAI.Core**: Domain models, enums, and interfaces
- **DnDAI.Data**: Entity Framework Core, database context, and repositories
- **DnDAI.Services**: Business logic, LLM integration, and memory management
- **DnDAI.Desktop**: WPF application with user interface

## Prerequisites

1. **.NET 8.0 SDK** or later
   - Download from: https://dotnet.microsoft.com/download

2. **SQL Server** (one of the following):
   - SQL Server LocalDB (comes with Visual Studio)
   - SQL Server Express (free)
   - SQL Server Developer Edition (free)
   - Full SQL Server

3. **Qwen LLM** (via Ollama or direct API):
   - **Option A - Ollama** (Recommended):
     - Install Ollama from: https://ollama.ai
     - Pull the Qwen model: `ollama pull qwen2.5:latest`
     - Start Ollama service (runs on http://localhost:11434 by default)

   - **Option B - Direct Qwen API**:
     - Configure your Qwen API endpoint in `appsettings.json`

4. **Visual Studio 2022** (Optional but recommended)
   - Community Edition is free
   - Or use VS Code with C# extension

## Installation & Setup

### 1. Clone or Download the Repository

```bash
git clone <repository-url>
cd DND-2024-AI
```

### 2. Configure the Database

Edit `src/DnDAI.Desktop/appsettings.json` and update the connection string if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DnDAI;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Connection String Options:**

- **LocalDB** (default): `Server=(localdb)\\mssqllocaldb;Database=DnDAI;Trusted_Connection=True;MultipleActiveResultSets=true`
- **SQL Express**: `Server=.\\SQLEXPRESS;Database=DnDAI;Trusted_Connection=True;MultipleActiveResultSets=true`
- **Full SQL Server**: `Server=localhost;Database=DnDAI;User Id=your_user;Password=your_password;MultipleActiveResultSets=true`

### 3. Configure Qwen LLM Settings

Edit `src/DnDAI.Desktop/appsettings.json` to configure your LLM:

```json
{
  "QwenSettings": {
    "ApiUrl": "http://localhost:11434",
    "ModelName": "qwen2.5:latest",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "TimeoutSeconds": 120
  }
}
```

### 4. Restore NuGet Packages

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run the Application

**Option A - Command Line:**
```bash
cd src/DnDAI.Desktop
dotnet run
```

**Option B - Visual Studio:**
1. Open `DnDAI.sln` in Visual Studio
2. Set `DnDAI.Desktop` as the startup project
3. Press F5 to run

## Usage

### Getting Started

1. **Ollama Setup**:
   - The application will **automatically start Ollama** if it's installed and not running
   - No manual startup required! Just launch the app.
   - If you prefer to manage Ollama manually, see the configuration section below

2. **Launch the Application**

3. **Create a New Campaign**:
   - Click "New Campaign"
   - Enter campaign name, setting, and description
   - Click "Create"

4. **Start a Session**:
   - Click "Start Session"
   - The chat interface will activate

5. **Interact with the DM**:
   - Type your actions and questions in the input box
   - Press Ctrl+Enter or click "Send"
   - The AI Dungeon Master will respond based on campaign context

6. **End the Session**:
   - Click "End Session" when you're done playing

### Managing Your Campaign

- **Add NPC**: Create new non-player characters
- **Add Location**: Define new places in your world
- **Add Quest**: Track objectives and missions
- **View Campaign**: See all campaign details and statistics

### Rolling Dice

The application includes a comprehensive dice rolling system:

**Quick Roll Buttons:**
- Click any dice button (d4, d6, d8, d10, d12, d20, d100) for an instant roll
- Results are displayed in the chat and logged to the session history

**Custom Rolls:**
- Enter custom expressions in the text box (e.g., "2d6+3", "1d20+5", "3d8-2")
- Click "Roll" to execute the custom roll
- Supports standard dice notation: XdY+Z or XdY-Z

**Advantage and Disadvantage:**
- **ADV** button: Rolls d20 twice, takes the higher result (D&D Advantage)
- **DIS** button: Rolls d20 twice, takes the lower result (D&D Disadvantage)

**Roll Display:**
All dice rolls show:
- The expression rolled
- Individual die results
- Any modifiers applied
- The total result

All rolls are automatically saved to the conversation history, making it easy to reference past rolls.

### Database Management

The database is automatically created when you first run the application. All data is stored in SQL Server and persists between sessions.

To reset the database:
1. Stop the application
2. Delete the database using SQL Server Management Studio or:
   ```sql
   DROP DATABASE DnDAI;
   ```
3. Restart the application (database will be recreated)

## Database Schema

The application uses the following main tables:

- **Campaigns**: Campaign information
- **Sessions**: Gaming sessions
- **ConversationMessages**: Chat history
- **PlayerCharacters**: Player character stats and details
- **NPCs**: Non-player character information
- **Locations**: World locations and geography
- **Events**: Significant campaign events
- **Quests**: Active and completed quests

## Configuration Options

### Qwen Settings

| Setting | Description | Default |
|---------|-------------|---------|
| ApiUrl | Qwen API endpoint | http://localhost:11434 |
| ModelName | Qwen model to use | qwen2.5:latest |
| MaxTokens | Maximum response length | 2000 |
| Temperature | Creativity level (0.0-1.0) | 0.7 |
| TimeoutSeconds | Request timeout | 120 |

### Database Settings

The application uses Entity Framework Core with SQL Server. You can:
- Use migrations for schema updates
- Change to a different database provider (PostgreSQL, MySQL, etc.) by updating packages and connection string

## Troubleshooting

### Database Connection Errors

- Ensure SQL Server is running
- Verify the connection string in `appsettings.json`
- Check that LocalDB is installed (comes with Visual Studio)

### Qwen/LLM Connection Errors

- Verify Ollama is running: `ollama list`
- Check the API URL in `appsettings.json`
- Ensure the Qwen model is downloaded: `ollama pull qwen2.5:latest`
- Test the endpoint: `curl http://localhost:11434/api/tags`

### Build Errors

- Ensure .NET 8.0 SDK is installed: `dotnet --version`
- Clean and rebuild: `dotnet clean && dotnet build`
- Restore packages: `dotnet restore`

## Advanced Configuration

### Ollama Auto-Start Configuration

The application can automatically start and stop the Ollama API server. Configure this in `appsettings.json`:

```json
{
  "OllamaSettings": {
    "AutoStart": true,           // Automatically start Ollama on app launch
    "AutoStop": true,            // Automatically stop Ollama on app exit
    "OllamaPath": "ollama",      // Path to ollama executable (or "ollama" if in PATH)
    "StartupTimeoutSeconds": 30  // How long to wait for Ollama to start
  }
}
```

**Options:**
- **AutoStart**: Set to `false` to manage Ollama manually
- **AutoStop**: Set to `false` to keep Ollama running after the app closes
- **OllamaPath**: Full path to `ollama.exe` if it's not in your system PATH

**How it works:**
1. On app startup, checks if Ollama is already running
2. If not running and `AutoStart` is true, launches `ollama serve`
3. Waits up to `StartupTimeoutSeconds` for the API to become available
4. On app exit, stops Ollama if `AutoStop` is true (only stops it if the app started it)

**Manual Management:**
If you prefer to manage Ollama yourself, set `AutoStart: false` and run:
```bash
ollama serve
```

### Using a Different LLM

To use a different model (e.g., Llama, Mistral):
1. Update the `ModelName` in `appsettings.json`
2. Adjust `MaxTokens` and `Temperature` as needed
3. Ensure the model is compatible with Ollama's API format

### Custom Database Provider

To use PostgreSQL instead of SQL Server:
1. Replace `Microsoft.EntityFrameworkCore.SqlServer` with `Npgsql.EntityFrameworkCore.PostgreSQL`
2. Update the connection string
3. Update `DnDContext` to use PostgreSQL

## Development

### Project Structure

```
DnDAI/
├── src/
│   ├── DnDAI.Core/         # Domain models and interfaces
│   ├── DnDAI.Data/         # EF Core and data access
│   ├── DnDAI.Services/     # Business logic and services
│   └── DnDAI.Desktop/      # WPF application
└── DnDAI.sln               # Solution file
```

### Adding New Features

1. **Domain Models**: Add to `DnDAI.Core/Models`
2. **Database Tables**: Update `DnDContext.cs` and add migrations
3. **Business Logic**: Add services to `DnDAI.Services`
4. **UI**: Add views to `DnDAI.Desktop`

## License

This project is provided as-is for educational and personal use.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Support

For issues, questions, or suggestions, please open an issue on the repository.

---

**Enjoy your AI-powered D&D adventures!** 🎲🐉

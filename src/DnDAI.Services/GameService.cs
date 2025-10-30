using DnDAI.Core.Interfaces;
using DnDAI.Core.Models;
using DnDAI.Data.Repositories;

namespace DnDAI.Services;

public class GameService
{
    private readonly ILLMService _llmService;
    private readonly IDiceRoller _diceRoller;
    private readonly ICombatService _combatService;
    private readonly CampaignRepository _campaignRepository;
    private readonly SessionRepository _sessionRepository;
    private readonly IRepository<ConversationMessage> _messageRepository;
    private readonly IRepository<NPC> _npcRepository;
    private readonly IRepository<PlayerCharacter> _playerCharacterRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<Event> _eventRepository;
    private readonly IRepository<Quest> _questRepository;
    private readonly IRepository<CustomWeapon> _customWeaponRepository;
    private readonly IRepository<Spell> _spellRepository;
    private readonly IRepository<PlayerCharacterSpell> _pcSpellRepository;
    private readonly IRepository<Feature> _featureRepository;
    private readonly IRepository<CharacterFeature> _characterFeatureRepository;
    private readonly IRepository<Equipment> _equipmentRepository;
    private readonly IRepository<CharacterEquipment> _characterEquipmentRepository;
    private readonly CombatCommandParser _combatCommandParser;

    public GameService(
        ILLMService llmService,
        IDiceRoller diceRoller,
        ICombatService combatService,
        CampaignRepository campaignRepository,
        SessionRepository sessionRepository,
        IRepository<ConversationMessage> messageRepository,
        IRepository<NPC> npcRepository,
        IRepository<PlayerCharacter> playerCharacterRepository,
        IRepository<Location> locationRepository,
        IRepository<Event> eventRepository,
        IRepository<Quest> questRepository,
        IRepository<CustomWeapon> customWeaponRepository,
        IRepository<Spell> spellRepository,
        IRepository<PlayerCharacterSpell> pcSpellRepository,
        IRepository<Feature> featureRepository,
        IRepository<CharacterFeature> characterFeatureRepository,
        IRepository<Equipment> equipmentRepository,
        IRepository<CharacterEquipment> characterEquipmentRepository)
    {
        _llmService = llmService;
        _diceRoller = diceRoller;
        _combatService = combatService;
        _campaignRepository = campaignRepository;
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _npcRepository = npcRepository;
        _playerCharacterRepository = playerCharacterRepository;
        _locationRepository = locationRepository;
        _eventRepository = eventRepository;
        _questRepository = questRepository;
        _customWeaponRepository = customWeaponRepository;
        _spellRepository = spellRepository;
        _pcSpellRepository = pcSpellRepository;
        _featureRepository = featureRepository;
        _characterFeatureRepository = characterFeatureRepository;
        _equipmentRepository = equipmentRepository;
        _characterEquipmentRepository = characterEquipmentRepository;
        _combatCommandParser = new CombatCommandParser(combatService);
    }

    public async Task<Campaign> CreateCampaignAsync(string name, string description, string setting)
    {
        var campaign = new Campaign
        {
            Name = name,
            Description = description,
            Setting = setting,
            StartDate = DateTime.UtcNow,
            IsActive = true
        };

        return await _campaignRepository.AddAsync(campaign);
    }

    public async Task<Session> StartSessionAsync(int campaignId)
    {
        var sessions = await _sessionRepository.GetSessionsForCampaignAsync(campaignId);
        var sessionNumber = sessions.Any() ? sessions.Max(s => s.SessionNumber) + 1 : 1;

        var session = new Session
        {
            CampaignId = campaignId,
            SessionNumber = sessionNumber,
            StartTime = DateTime.UtcNow
        };

        return await _sessionRepository.AddAsync(session);
    }

    public async Task<Session> EndSessionAsync(int sessionId, string summary)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null)
            throw new ArgumentException("Session not found");

        session.EndTime = DateTime.UtcNow;
        session.Summary = summary;

        return await _sessionRepository.UpdateAsync(session);
    }

    public async Task<string> ProcessPlayerInputAsync(int campaignId, int sessionId, string playerInput, string? playerName = null)
    {
        // Save player message
        await SaveMessageAsync(sessionId, playerName ?? "Player", playerInput);

        // Get DM response from LLM
        var dmResponse = await _llmService.GenerateWithMemoryAsync(campaignId, playerInput);

        System.Diagnostics.Debug.WriteLine($"[GameService] DM response length before command stripping: {dmResponse.Length} characters");
        System.Diagnostics.Debug.WriteLine($"[GameService] First 200 chars: {dmResponse.Substring(0, Math.Min(200, dmResponse.Length))}");
        Console.WriteLine($"[GameService] DM response length before command stripping: {dmResponse.Length} characters");

        // Parse and execute combat commands
        var commandResults = await _combatCommandParser.ParseAndExecuteCommandsAsync(dmResponse, campaignId, sessionId);

        // Strip commands from the response text
        var cleanedResponse = _combatCommandParser.StripCommandsFromText(dmResponse);

        System.Diagnostics.Debug.WriteLine($"[GameService] Cleaned response length: {cleanedResponse.Length} characters");
        System.Diagnostics.Debug.WriteLine($"[GameService] First 200 chars: {cleanedResponse.Substring(0, Math.Min(200, cleanedResponse.Length))}");
        Console.WriteLine($"[GameService] Cleaned response length: {cleanedResponse.Length} characters");

        // Save DM message (cleaned version)
        await SaveMessageAsync(sessionId, "DM", cleanedResponse);

        // Log command results if any
        if (commandResults.Any())
        {
            var commandLog = string.Join(", ", commandResults);
            await SaveMessageAsync(sessionId, "System", $"Combat: {commandLog}", "System");
        }

        return cleanedResponse;
    }

    public async Task<ConversationMessage> SaveMessageAsync(int sessionId, string speaker, string message, string messageType = "Normal")
    {
        var conversationMessage = new ConversationMessage
        {
            SessionId = sessionId,
            Speaker = speaker,
            Message = message,
            MessageType = messageType,
            Timestamp = DateTime.UtcNow
        };

        return await _messageRepository.AddAsync(conversationMessage);
    }

    public async Task<NPC> CreateNPCAsync(int campaignId, NPC npc)
    {
        npc.CampaignId = campaignId;
        return await _npcRepository.AddAsync(npc);
    }

    public async Task<Location> CreateLocationAsync(int campaignId, Location location)
    {
        location.CampaignId = campaignId;
        return await _locationRepository.AddAsync(location);
    }

    public async Task<Event> CreateEventAsync(int campaignId, Event gameEvent)
    {
        gameEvent.CampaignId = campaignId;
        return await _eventRepository.AddAsync(gameEvent);
    }

    public async Task<Quest> CreateQuestAsync(int campaignId, Quest quest)
    {
        quest.CampaignId = campaignId;
        return await _questRepository.AddAsync(quest);
    }

    public async Task<Campaign?> GetCampaignAsync(int campaignId)
    {
        return await _campaignRepository.GetCampaignWithDetailsAsync(campaignId);
    }

    public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync()
    {
        return await _campaignRepository.GetAllCampaignsWithBasicInfoAsync();
    }

    public async Task<Session?> GetSessionAsync(int sessionId)
    {
        return await _sessionRepository.GetSessionWithMessagesAsync(sessionId);
    }

    public async Task<IEnumerable<Session>> GetCampaignSessionsAsync(int campaignId)
    {
        return await _sessionRepository.GetSessionsForCampaignAsync(campaignId);
    }

    public async Task<IEnumerable<NPC>> GetCampaignNPCsAsync(int campaignId)
    {
        return await _npcRepository.FindAsync(n => n.CampaignId == campaignId);
    }

    public async Task<IEnumerable<Location>> GetCampaignLocationsAsync(int campaignId)
    {
        return await _locationRepository.FindAsync(l => l.CampaignId == campaignId);
    }

    public async Task<IEnumerable<Quest>> GetActiveQuestsAsync(int campaignId)
    {
        return await _questRepository.FindAsync(q => q.CampaignId == campaignId && q.IsActive && !q.IsCompleted);
    }

    // Dice Rolling Methods
    public DiceRoll RollDice(string expression, string rolledBy = "Player", string purpose = "")
    {
        return _diceRoller.Roll(expression, rolledBy, purpose);
    }

    public DiceRoll RollD20(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        return _diceRoller.RollD20(modifier, rolledBy, purpose);
    }

    public DiceRoll RollWithModifier(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        string expression = modifier >= 0 ? $"1d20+{modifier}" : $"1d20{modifier}";
        return _diceRoller.Roll(expression, rolledBy, purpose);
    }

    public DiceRoll RollWithAdvantage(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        return _diceRoller.RollWithAdvantage(modifier, rolledBy, purpose);
    }

    public DiceRoll RollWithDisadvantage(int modifier = 0, string rolledBy = "Player", string purpose = "")
    {
        return _diceRoller.RollWithDisadvantage(modifier, rolledBy, purpose);
    }

    public async Task<DiceRoll> RollAndLogAsync(int sessionId, string expression, string rolledBy = "Player", string purpose = "")
    {
        var roll = _diceRoller.Roll(expression, rolledBy, purpose);
        await LogDiceRollAsync(sessionId, roll);
        return roll;
    }

    public async Task LogDiceRollAsync(int sessionId, DiceRoll roll)
    {
        var rollDetails = $"[{roll.Expression}] = {string.Join(", ", roll.IndividualRolls)}";
        if (roll.Modifier != 0)
        {
            rollDetails += $" {(roll.Modifier >= 0 ? "+" : "")}{roll.Modifier}";
        }
        rollDetails += $" = **{roll.Total}**";

        if (!string.IsNullOrEmpty(roll.Purpose))
        {
            rollDetails = $"{roll.Purpose}: {rollDetails}";
        }

        await SaveMessageAsync(sessionId, roll.RolledBy, rollDetails, "Dice Roll");
    }

    // Player Character Management
    public async Task<PlayerCharacter> AddPlayerCharacterAsync(int campaignId, PlayerCharacter playerCharacter)
    {
        playerCharacter.CampaignId = campaignId;
        await _playerCharacterRepository.AddAsync(playerCharacter);
        return playerCharacter;
    }

    public async Task UpdatePlayerCharacterAsync(PlayerCharacter playerCharacter)
    {
        await _playerCharacterRepository.UpdateAsync(playerCharacter);
    }

    // Custom Weapon Management
    public async Task<CustomWeapon> AddCustomWeaponAsync(int playerCharacterId, CustomWeapon weapon)
    {
        weapon.PlayerCharacterId = playerCharacterId;
        await _customWeaponRepository.AddAsync(weapon);
        return weapon;
    }

    public async Task<List<CustomWeapon>> GetCustomWeaponsAsync(int playerCharacterId)
    {
        var allWeapons = await _customWeaponRepository.GetAllAsync();
        return allWeapons.Where(w => w.PlayerCharacterId == playerCharacterId).ToList();
    }

    public async Task<bool> DeleteCustomWeaponAsync(int weaponId)
    {
        var weapon = await _customWeaponRepository.GetByIdAsync(weaponId);
        if (weapon == null) return false;
        await _customWeaponRepository.DeleteAsync(weaponId);
        return true;
    }

    // Spell Management
    public async Task<Spell> CreateSpellAsync(Spell spell)
    {
        await _spellRepository.AddAsync(spell);
        return spell;
    }

    public async Task<List<Spell>> GetAllSpellsAsync()
    {
        return (await _spellRepository.GetAllAsync()).ToList();
    }

    public async Task<PlayerCharacterSpell> LearnSpellAsync(int playerCharacterId, int spellId, bool isPrepared = false)
    {
        var pcSpell = new PlayerCharacterSpell
        {
            PlayerCharacterId = playerCharacterId,
            SpellId = spellId,
            IsPrepared = isPrepared
        };
        await _pcSpellRepository.AddAsync(pcSpell);
        return pcSpell;
    }

    public async Task<bool> UseSpellSlotAsync(int playerCharacterId, int spellLevel)
    {
        var pc = await _campaignRepository.GetAllAsync();
        var character = pc.SelectMany(c => c.PlayerCharacters).FirstOrDefault(p => p.Id == playerCharacterId);
        if (character == null) return false;

        // Deduct spell slot based on level
        var slotProperty = typeof(PlayerCharacter).GetProperty($"SpellSlots{spellLevel}Current");
        if (slotProperty != null)
        {
            int current = (int)(slotProperty.GetValue(character) ?? 0);
            if (current > 0)
            {
                slotProperty.SetValue(character, current - 1);
                await _campaignRepository.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    public async Task RestoreSpellSlotsAsync(int playerCharacterId)
    {
        var pc = await _campaignRepository.GetAllAsync();
        var character = pc.SelectMany(c => c.PlayerCharacters).FirstOrDefault(p => p.Id == playerCharacterId);
        if (character == null) return;

        // Restore all spell slots to max
        for (int level = 1; level <= 9; level++)
        {
            var currentProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Current");
            var maxProp = typeof(PlayerCharacter).GetProperty($"SpellSlots{level}Max");
            if (currentProp != null && maxProp != null)
            {
                int max = (int)(maxProp.GetValue(character) ?? 0);
                currentProp.SetValue(character, max);
            }
        }
        await _campaignRepository.SaveChangesAsync();
    }

    // Feature Management
    public async Task<List<Feature>> GetAllFeaturesAsync()
    {
        return (await _featureRepository.GetAllAsync()).ToList();
    }

    // Equipment Management
    public async Task<List<Equipment>> GetAllEquipmentAsync()
    {
        return (await _equipmentRepository.GetAllAsync()).ToList();
    }

    public async Task<List<Equipment>> GetWeaponsAsync()
    {
        var all = await _equipmentRepository.GetAllAsync();
        return all.Where(e => e.Type == Core.Enums.EquipmentType.Weapon).ToList();
    }

    public async Task<List<CharacterEquipment>> GetCharacterEquipmentAsync(int playerCharacterId)
    {
        var all = await _characterEquipmentRepository.GetAllAsync();
        return all.Where(ce => ce.PlayerCharacterId == playerCharacterId).ToList();
    }

    public async Task<CharacterEquipment> AddEquipmentToCharacterAsync(int playerCharacterId, int equipmentId, int quantity = 1, bool isEquipped = false)
    {
        var charEquip = new CharacterEquipment
        {
            PlayerCharacterId = playerCharacterId,
            EquipmentId = equipmentId,
            Quantity = quantity,
            IsEquipped = isEquipped
        };
        await _characterEquipmentRepository.AddAsync(charEquip);
        return charEquip;
    }

    public async Task<Equipment> CreateCustomEquipmentAsync(Equipment equipment)
    {
        equipment.IsStandard = false;
        await _equipmentRepository.AddAsync(equipment);
        return equipment;
    }

    public async Task RemoveEquipmentFromCharacterAsync(int characterEquipmentId)
    {
        await _characterEquipmentRepository.DeleteAsync(characterEquipmentId);
    }

    public async Task UpdateCharacterEquipmentAsync(CharacterEquipment characterEquipment)
    {
        await _characterEquipmentRepository.UpdateAsync(characterEquipment);
    }
}

using System.Text.Json.Serialization;
using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.SignalR;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Repositories;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.Processors;
using GameServerApp.Repositories;
using GameServerApp.Services;
using GameServerApp.World;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure Options
builder.Services.Configure<WorldConfig>(
    builder.Configuration.GetSection("WorldSettings"));

// Add SignalR
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow any origin, including file:// (null)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

builder.Services.AddControllers();

// Add Hosted Service for game loop
builder.Services.AddHostedService<GameLoopService>();

// Register Core Services (Stateless)
builder.Services.AddSingleton<IMovementService, MovementService>();
builder.Services.AddSingleton<ICombatService, CombatService>();
builder.Services.AddSingleton<IProceduralWorldService, ProceduralWorldService>();
builder.Services.AddSingleton<IMonsterMovementService, MonsterMovementService>();
builder.Services.AddSingleton<IPathfindingService, AStarPathfindingService>();
builder.Services.AddSingleton<IWorldGenerator, WorldGenerator>();
builder.Services.AddSingleton<IFractalRiverWorldService, FractalRiverWorldService>();
builder.Services.AddSingleton<IItemFactory, ItemFactory>();
builder.Services.AddSingleton<IItemDefinitionRepository>(_ =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "items.json");
    return new ItemDefinitionRepository(File.ReadAllText(path));
});
builder.Services.AddSingleton<IMonsterDefinitionRepository>(_ =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "monsters.json");
    return new MonsterDefinitionRepository(File.ReadAllText(path));
});
builder.Services.AddSingleton<ILootTableRepository>(_ =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "loot-tables.json");
    return new LootTableRepository(File.ReadAllText(path));
});
builder.Services.AddSingleton<ILootTableService, LootTableService>();
builder.Services.AddSingleton<IBiomeDefinitionRepository>(_ =>
{
    var path = Path.Combine(builder.Environment.ContentRootPath, "Data", "biomes.json");
    return new BiomeDefinitionRepository(File.ReadAllText(path));
});
builder.Services.AddSingleton<IBiomeSelector, BiomeSelector>();
builder.Services.AddSingleton<IRankingRepository>(sp =>
{
    var relativePath = builder.Configuration["RankingSettings:FilePath"] ?? "Data/ranking.json";
    var path = Path.Combine(builder.Environment.ContentRootPath, relativePath);
    return new JsonRankingRepository(path);
});
builder.Services.AddSingleton<IRankingService, RankingService>();

// Register Processors
builder.Services.AddSingleton<IPlayerMovementProcessor, PlayerMovementProcessor>();
builder.Services.AddSingleton<ICombatProcessor, CombatProcessor>();
builder.Services.AddSingleton<IMonsterLifecycleProcessor, MonsterLifecycleProcessor>();
builder.Services.AddSingleton<IItemProcessor, ItemProcessor>();
builder.Services.AddSingleton<IEquipmentProcessor, EquipmentProcessor>();
builder.Services.AddSingleton<IChunkProcessor, ChunkProcessor>();
builder.Services.AddSingleton<IPlayerRegenerationProcessor, PlayerRegenerationProcessor>();

// Register Managers (Orchestrators/Stateful)
builder.Services.AddSingleton<IGameStateManager, GameStateManager>();
builder.Services.AddSingleton<IWorldManager, WorldManager>();
builder.Services.AddSingleton<IStaticWorldManager, StaticWorldManager>();
builder.Services.AddSingleton<IMonsterManager, MonsterManager>();
builder.Services.AddSingleton<IPlayerManager, PlayerManager>();
builder.Services.AddSingleton<IItemManager, ItemManager>();
builder.Services.AddSingleton<IInventoryManager, InventoryManager>();
builder.Services.AddSingleton<IEquipmentManager, EquipmentManager>();

// CollisionManager depende de IStaticWorldManager
builder.Services.AddSingleton<ICollisionManager>(sp =>
    new CollisionManager(sp.GetRequiredService<IStaticWorldManager>()));



// Register Infrastructure implementations
builder.Services.AddSingleton<IWorldEvents, SignalREventEmitter>();

builder.Services.AddSingleton<IIdGeneratorService, IdGeneratorService>();

var app = builder.Build();

InitializeWaterBlocks(app.Services);
InitializeRandomMonsters(app.Services);

app.UseCors("AllowAll");

app.MapGet("/", () => "MMO Game Server is running!");

// Map SignalR Hub
app.MapHub<GameHub>("/gamehub");

// Map Controller
app.MapControllers();

app.Run();


static void InitializeWaterBlocks(IServiceProvider services)
{
    var fractal = services.GetRequiredService<IFractalRiverWorldService>();
    var staticWorld = services.GetRequiredService<IStaticWorldManager>();
    var config = services.GetRequiredService<IOptions<WorldConfig>>().Value;

    for (int y = 0; y < config.Map.Height; y++)
        for (int x = 0; x < config.Map.Width; x++)
            if (fractal.IsWaterTile(x, y))
                staticWorld.AddBlockedPosition(new GameServerApp.Contracts.Types.Position(x, y));
}

static void InitializeRandomMonsters(IServiceProvider services)
{
    var monsterManager = services.GetRequiredService<IMonsterManager>();
    var config = services.GetRequiredService<IOptions<WorldConfig>>().Value;

    monsterManager.SpawnRandomMonsters(
        count: config.Monsters.MaxGlobal,
        width: config.Map.Width,
        height: config.Map.Height,
        safeSpawnRadius: config.Map.SafeSpawnRadius,
        seed: 20260310);
}

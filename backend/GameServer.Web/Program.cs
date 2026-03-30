using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.SignalR;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.Services;
using GameServerApp.World;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure Options
builder.Services.Configure<WorldConfig>(
    builder.Configuration.GetSection("MonsterSpawn"));

// Add SignalR
builder.Services.AddSignalR();

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

// Register Managers (Orchestrators/Stateful)
builder.Services.AddSingleton<IGameStateManager, GameStateManager>();
builder.Services.AddSingleton<IWorldManager, WorldManager>();
builder.Services.AddSingleton<IStaticWorldManager, StaticWorldManager>();
builder.Services.AddSingleton<IMonsterManager, MonsterManager>();
builder.Services.AddSingleton<IPlayerManager, PlayerManager>();
builder.Services.AddSingleton<IItemManager, ItemManager>();

// CollisionManager depende de IStaticWorldManager
builder.Services.AddSingleton<ICollisionManager>(sp =>
    new CollisionManager(sp.GetRequiredService<IStaticWorldManager>()));



// Register Infrastructure implementations
builder.Services.AddSingleton<IWorldEvents, SignalREventEmitter>();

builder.Services.AddSingleton<IIdGeneratorService, IdGeneratorService>();

var app = builder.Build();

InitializeRandomMonsters(app.Services);

app.UseCors("AllowAll");

app.MapGet("/", () => "MMO Game Server is running!");

// Map SignalR Hub
app.MapHub<GameHub>("/gamehub");

// Map Controller
app.MapControllers();

app.Run();


static void InitializeRandomMonsters(IServiceProvider services)
{
    var monsterManager = services.GetRequiredService<IMonsterManager>();
    var config = services.GetRequiredService<IOptions<WorldConfig>>().Value;

    monsterManager.SpawnRandomMonsters(
        count: config.MaxMonsters,
        width: config.WorldWidth,
        height: config.WorldHeight,
        safeSpawnRadius: config.SafeSpawnRadius,
        seed: 20260310);
}

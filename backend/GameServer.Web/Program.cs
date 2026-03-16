using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.SignalR;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.Services;
using GameServerApp.World;

var builder = WebApplication.CreateBuilder(args);

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

// Register Managers (Orchestrators/Stateful)
builder.Services.AddSingleton<IGameStateManager, GameStateManager>();
builder.Services.AddSingleton<IWorldManager, WorldManager>();
builder.Services.AddSingleton<IStaticWorldManager, StaticWorldManager>();
builder.Services.AddSingleton<IMonsterManager, MonsterManager>();
builder.Services.AddSingleton<IPlayerManager, PlayerManager>();

// CollisionManager depende de IStaticWorldManager
builder.Services.AddSingleton<ICollisionManager>(sp =>
    new CollisionManager(sp.GetRequiredService<IStaticWorldManager>()));



// Register Infrastructure implementations
builder.Services.AddSingleton<IWorldEvents, SignalREventEmitter>();

builder.Services.AddSingleton<IIdGeneratorService, IdGeneratorService>();

var app = builder.Build();

InitializeProceduralMap(app.Services);
InitializeRandomMonsters(app.Services);

app.UseCors("AllowAll");

app.MapGet("/", () => "MMO Game Server is running!");

// Map SignalR Hub
app.MapHub<GameHub>("/gamehub");

// Map Controller
app.MapControllers();

app.Run();

static void InitializeProceduralMap(IServiceProvider services)
{
    var worldManager = services.GetRequiredService<IWorldManager>();
    var proceduralWorldService = services.GetRequiredService<IProceduralWorldService>();

    var obstacles = proceduralWorldService.GenerateRandomObstacles(
        width: 32,
        height: 32,
        fillPercentage: 0.10,
        safeSpawnRadius: 3,
        seed: 20260309);

    foreach (var obstacle in obstacles)
    {
        worldManager.InstantiateObject(obstacle);
    }
}

static void InitializeRandomMonsters(IServiceProvider services)
{
    var monsterManager = services.GetRequiredService<IMonsterManager>();

    monsterManager.SpawnRandomMonsters(
        count: 15,
        width: 32,
        height: 32,
        safeSpawnRadius: 4,
        seed: 20260310);
}

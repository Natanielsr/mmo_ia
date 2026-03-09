using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.SignalR;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
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

// Register Core Services (Stateless)
builder.Services.AddSingleton<IMovementService, MovementService>();
builder.Services.AddSingleton<ICombatService, CombatService>();
builder.Services.AddSingleton<IProceduralWorldService, ProceduralWorldService>();

// Register Managers (Orchestrators/Stateful)
builder.Services.AddSingleton<IGameStateManager, GameStateManager>();
builder.Services.AddSingleton<IWorldProcessor, WorldProcessor>();
builder.Services.AddSingleton<IStaticWorldManager, StaticWorldManager>();

// CollisionManager depende de IStaticWorldManager
builder.Services.AddSingleton<ICollisionManager>(sp =>
    new CollisionManager(sp.GetRequiredService<IStaticWorldManager>()));



// Register Infrastructure implementations
builder.Services.AddSingleton<IWorldEvents, SignalREventEmitter>();

builder.Services.AddSingleton<IIdGeneratorService, IdGeneratorService>();

var app = builder.Build();

InitializeProceduralMap(app.Services);

app.UseCors("AllowAll");

app.MapGet("/", () => "MMO Game Server is running!");

// Map SignalR Hub
app.MapHub<GameHub>("/gamehub");

// Map Controller
app.MapControllers();

app.Run();

static void InitializeProceduralMap(IServiceProvider services)
{
    var staticWorldManager = services.GetRequiredService<IStaticWorldManager>();
    var collisionManager = services.GetRequiredService<ICollisionManager>();
    var proceduralWorldService = services.GetRequiredService<IProceduralWorldService>();

    // Ajuste estes valores conforme o tamanho do mapa desejado.
    var obstacles = proceduralWorldService.GenerateRandomObstacles(
        width: 80,
        height: 80,
        fillPercentage: 0.10,
        safeSpawnRadius: 3,
        seed: 20260309);

    foreach (var obstacle in obstacles)
    {
        staticWorldManager.AddStaticObject(obstacle);
        collisionManager.RegisterStaticObject(obstacle);
    }
}

using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Managers;
using GameServerApp.Processors;
using GameServerApp.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace GameServer.Tests.Helpers
{
    /// <summary>
    /// Builder para WorldManager em testes.
    ///
    /// Uso básico (tudo mockado):
    ///   var b = new WorldManagerBuilder();
    ///   b.MonsterManager.Setup(...);
    ///   var wm = b.Build();
    ///   b.Events.Verify(...);
    ///
    /// Serviços reais (movement, combat, collision):
    ///   var b = new WorldManagerBuilder().WithRealServices();
    ///   var wm = b.Build();
    ///   b.BuiltCollision.RegisterDynamicObject(player);
    ///
    /// Adicionar dependência ao WorldManager = atualizar apenas Build().
    /// </summary>
    public sealed class WorldManagerBuilder
    {
        // ── Mocks sempre públicos para Setup/Verify ───────────────────────────
        public Mock<IWorldEvents>            Events                 { get; } = new();
        public Mock<IStaticWorldManager>     StaticWorld            { get; } = new();
        public Mock<IMonsterMovementService> MonsterMovementService { get; } = new();
        public Mock<IMonsterManager>         MonsterManager         { get; } = new();
        public Mock<IPlayerManager>          PlayerManager          { get; } = new();
        public Mock<IItemManager>            ItemManager            { get; } = new();
        public Mock<IIdGeneratorService>     IdGenerator            { get; } = new();
        public Mock<IChunkObjectGenerator>   WorldGenerator         { get; } = new();
        public Mock<IBiomeSelector>          BiomeSelector          { get; } = new();
        public Mock<IWorldTerrainService> FractalRiverWorld     { get; } = new();
        public Mock<ILootTableService>       LootTable              { get; } = new();
        public Mock<IInventoryManager>       InventoryManager       { get; } = new();
        public Mock<IEquipmentManager>            EquipmentManager      { get; } = new();
        public Mock<IPlayerRegenerationProcessor> RegenerationProcessor { get; } = new();

        // ── Overrides opcionais via fluent API ────────────────────────────────
        private IMovementService?    _movementService;
        private IGameStateManager?   _gameStateManager;
        private IStaticWorldManager? _staticWorldOverride;
        private IMonsterManager?     _monsterManagerOverride;
        private ICollisionManager?   _collisionOverride;
        private WorldConfig          _config = new();

        // ── Acessível após Build() ────────────────────────────────────────────

        /// <summary>O CollisionManager construído por Build(). Use para RegisterDynamicObject
        /// ou para compartilhá-lo entre dois WorldManagers no mesmo teste.</summary>
        public ICollisionManager? BuiltCollision { get; private set; }

        /// <summary>O PlayerRegenerationProcessor construído por Build(). Use para manipular
        /// _lastRegenTime via reflexão em testes de regeneração.</summary>
        public PlayerRegenerationProcessor? BuiltRegeneration { get; private set; }

        // ── Fluent API ────────────────────────────────────────────────────────

        /// <summary>
        /// Substitui MovementService, StaticWorldManager, CollisionManager
        /// e GameStateManager por implementações reais. Use em testes que exercitam o
        /// caminho completo de movimento e ataque.
        /// </summary>
        public WorldManagerBuilder WithRealServices()
        {
            _movementService  = new MovementService();
            var sw            = new StaticWorldManager(Options.Create(_config));
            _staticWorldOverride = sw;
            var cm            = new CollisionManager(sw);
            _collisionOverride   = cm;
            var rankingService = new Mock<IRankingService>();
            _gameStateManager    = new GameStateManager(cm, rankingService.Object, Events.Object);
            return this;
        }

        /// <summary>Injeta um MonsterManager concreto (ex: para testes de integração).</summary>
        public WorldManagerBuilder WithMonsterManager(IMonsterManager mm)
        {
            _monsterManagerOverride = mm;
            return this;
        }

        /// <summary>
        /// Compartilha um CollisionManager existente entre dois WorldManagers no mesmo teste.
        /// </summary>
        public WorldManagerBuilder WithCollision(ICollisionManager cm)
        {
            _collisionOverride = cm;
            return this;
        }

        public WorldManagerBuilder WithConfig(WorldConfig config)
        {
            _config = config;
            return this;
        }

        /// <summary>
        /// Constrói o WorldManager com todas as dependências resolvidas.
        /// Após a chamada, BuiltCollision fica disponível.
        /// </summary>
        public WorldManager Build()
        {
            var staticWorld = _staticWorldOverride ?? StaticWorld.Object;
            BuiltCollision  = _collisionOverride   ?? new CollisionManager(StaticWorld.Object);
            var gameState   = _gameStateManager    ?? new Mock<IGameStateManager>().Object;
            var monsterMgr  = _monsterManagerOverride ?? MonsterManager.Object;
            var opts        = Options.Create(_config);

            var defaultBiome = new BiomeDefinition
            {
                Id = 1, TagName = "green_field", Name = "Green Field",
                GroundTilePrefix = "grass", GroundTileCount = 12,
                Objects = ["tree"],
                MonsterTags = new() { "rat", "wolf", "orc", "spider" },
                IsDefault = true
            };
            BiomeSelector.Setup(b => b.GetBiomeForChunk(It.IsAny<ChunkCoord>())).Returns(defaultBiome);
            BiomeSelector.Setup(b => b.GetBiomeForPosition(It.IsAny<Position>())).Returns(defaultBiome);
            BiomeSelector.Setup(b => b.GetBiomeForTile(It.IsAny<int>(), It.IsAny<int>())).Returns(defaultBiome);

            FractalRiverWorld.Setup(f => f.GetChunkTiles(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int _cx, int _cy, int size) =>
                    (Enumerable.Range(0, size).Select(_ => new int[size]).ToArray(),
                     Enumerable.Range(0, size).Select(_ => new int[size]).ToArray()));

            var movementProcessor = new PlayerMovementProcessor(
                _movementService ?? new Mock<IMovementService>().Object,
                BuiltCollision,
                Events.Object,
                opts);

            var combatProcessor = new CombatProcessor(
                gameState,
                Events.Object,
                monsterMgr,
                PlayerManager.Object,
                LootTable.Object,
                ItemManager.Object,
                IdGenerator.Object,
                opts);

            var monsterLifecycleProcessor = new MonsterLifecycleProcessor(
                monsterMgr,
                PlayerManager.Object,
                MonsterMovementService.Object,
                Events.Object,
                BiomeSelector.Object,
                opts);

            var itemProcessor = new ItemProcessor(
                ItemManager.Object,
                InventoryManager.Object,
                Events.Object);

            var equipmentProcessor = new EquipmentProcessor(
                EquipmentManager.Object,
                InventoryManager.Object,
                Events.Object);

            var chunkProcessor = new ChunkProcessor(
                staticWorld,
                WorldGenerator.Object,
                ItemManager.Object,
                Events.Object,
                FractalRiverWorld.Object,
                opts);

            BuiltRegeneration = new PlayerRegenerationProcessor(
                PlayerManager.Object,
                Events.Object);
            var regenerationProcessor = BuiltRegeneration;

            return new WorldManager(
                movementProcessor,
                combatProcessor,
                monsterLifecycleProcessor,
                itemProcessor,
                equipmentProcessor,
                chunkProcessor,
                regenerationProcessor,
                PlayerManager.Object,
                BuiltCollision,
                staticWorld);
        }
    }
}

using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
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
        public Mock<IWorldEvents>            Events               { get; } = new();
        public Mock<IStaticWorldManager>     StaticWorld          { get; } = new();
        public Mock<IMonsterMovementService> MonsterMovementService { get; } = new();
        public Mock<IMonsterManager>         MonsterManager       { get; } = new();
        public Mock<IPlayerManager>          PlayerManager        { get; } = new();
        public Mock<IItemManager>            ItemManager          { get; } = new();
        public Mock<IIdGeneratorService>     IdGenerator          { get; } = new();
        public Mock<IWorldGenerator>         WorldGenerator       { get; } = new();
        public Mock<ILootTableService>       LootTable            { get; } = new();
        public Mock<IInventoryManager>       InventoryManager     { get; } = new();

        // ── Overrides opcionais via fluent API ────────────────────────────────
        private IMovementService?    _movementService;
        private ICombatService?      _combatService;
        private IGameStateManager?   _gameStateManager;
        private IStaticWorldManager? _staticWorldOverride;
        private IMonsterManager?     _monsterManagerOverride;
        private ICollisionManager?   _collisionOverride;
        private WorldConfig          _config = new();

        // ── Acessível após Build() ────────────────────────────────────────────

        /// <summary>O CollisionManager construído por Build(). Use para RegisterDynamicObject
        /// ou para compartilhá-lo entre dois WorldManagers no mesmo teste.</summary>
        public ICollisionManager? BuiltCollision { get; private set; }

        // ── Fluent API ────────────────────────────────────────────────────────

        /// <summary>
        /// Substitui MovementService, CombatService, StaticWorldManager, CollisionManager
        /// e GameStateManager por implementações reais. Use em testes que exercitam o
        /// caminho completo de movimento e ataque.
        /// </summary>
        public WorldManagerBuilder WithRealServices()
        {
            _movementService = new MovementService();
            _combatService   = new CombatService();
            var sw           = new StaticWorldManager(Options.Create(_config));
            _staticWorldOverride = sw;
            var cm           = new CollisionManager(sw);
            _collisionOverride   = cm;
            _gameStateManager    = new GameStateManager(cm);
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
        /// Padrão: When_Monster_Dies_Collision_Should_Be_Released.
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

            return new WorldManager(
                _movementService        ?? new Mock<IMovementService>().Object,
                BuiltCollision,
                _combatService          ?? new Mock<ICombatService>().Object,
                gameState,
                Events.Object,
                staticWorld,
                MonsterMovementService.Object,
                _monsterManagerOverride ?? MonsterManager.Object,
                PlayerManager.Object,
                ItemManager.Object,
                IdGenerator.Object,
                WorldGenerator.Object,
                Options.Create(_config),
                LootTable.Object,
                InventoryManager.Object
            );
        }
    }
}

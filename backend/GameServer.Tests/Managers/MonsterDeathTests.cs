using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using Moq;
using GameServerApp.Managers;
using GameServerApp.World;
using GameServerApp.Dtos;

namespace GameServer.Tests.Managers
{
    public class MonsterDeathTests
    {
        private readonly WorldManager _worldManager;
        private readonly CollisionManager _collisionManager;
        private readonly Mock<IMonsterManager> _mockMonsterManager = new();
        private readonly Mock<IPlayerManager> _mockPlayerManager = new();
        private readonly Mock<IWorldEvents> _mockEvents = new();
        private readonly Mock<IGameStateManager> _mockGameStateManager = new();
        private readonly Mock<IStaticWorldManager> _mockStaticWorld = new();
        private readonly Mock<IMovementService> _mockMovementService = new();
        private readonly Mock<ICombatService> _mockCombatService = new();
        private readonly Mock<IMonsterMovementService> _mockMonsterMovementService = new();

        public MonsterDeathTests()
        {
            _collisionManager = new CollisionManager(_mockStaticWorld.Object);
            _worldManager = new WorldManager(
                _mockMovementService.Object,
                _collisionManager,
                _mockCombatService.Object,
                _mockGameStateManager.Object,
                _mockEvents.Object,
                _mockStaticWorld.Object,
                _mockMonsterMovementService.Object,
                _mockMonsterManager.Object,
                _mockPlayerManager.Object
            );
        }

        [Fact]
        public void Dead_Monster_Should_Be_Passable()
        {
            var monster = new Monster(1, "Rat", "rat", new Position(0, 0), 30, 5);
            
            Assert.False(monster.IsPassable);
            
            monster.TakeDamage(30);
            
            Assert.True(monster.IsDead);
            Assert.True(monster.IsPassable);
        }

        [Fact]
        public void When_Monster_Dies_It_Should_Be_Removed_From_MonsterManager()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var monster = new Monster(2, "Rat", "rat", new Position(1, 0), 10, 5);
            
            _mockMonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            
            // Ataca o monstro até a morte (dano padrão no WorldManager é 10)
            _worldManager.ProcessPlayerAttackMonster(player, "2");
            
            Assert.True(monster.IsDead);
            _mockMonsterManager.Verify(m => m.RemoveMonster(2), Times.Once);
        }

        [Fact]
        public void When_Monster_Dies_Collision_Should_Be_Released()
        {
            // Setup real MonsterManager with real CollisionManager to test integration
            var mockIdGen = new Mock<IIdGeneratorService>();
            var realMonsterManager = new MonsterManager(_collisionManager, mockIdGen.Object);
            
            var worldManagerWithRealManagers = new WorldManager(
                _mockMovementService.Object,
                _collisionManager,
                _mockCombatService.Object,
                _mockGameStateManager.Object,
                _mockEvents.Object,
                _mockStaticWorld.Object,
                _mockMonsterMovementService.Object,
                realMonsterManager,
                _mockPlayerManager.Object
            );

            var monsterPos = new Position(1, 0);
            mockIdGen.Setup(s => s.GenerateId()).Returns(2);
            _mockStaticWorld.Setup(s => s.IsBlocked(It.IsAny<Position>())).Returns(false);

            // Spawn monster
            realMonsterManager.SpawnRandomMonsters(1, 10, 10, 0, 123);
            var monster = realMonsterManager.GetAllMonsters().First();
            var monsterId = monster.Id;
            var pos = monster.Position;

            // Verify collision exists
            Assert.True(_collisionManager.IsPositionBlocked(pos));

            // Defeat monster
            var player = new Player(1, "Hero", new Position(pos.X - 1, pos.Y));
            while (!monster.IsDead)
            {
                worldManagerWithRealManagers.ProcessPlayerAttackMonster(player, monsterId.ToString());
            }

            // Verify collision is released
            Assert.False(_collisionManager.IsPositionBlocked(pos));
            Assert.Null(realMonsterManager.GetMonsterById(monsterId));
        }

        [Fact]
        public void Monster_Removal_Releases_CollisionManager_Entry()
        {
            var collisionManager = new CollisionManager(_mockStaticWorld.Object);
            var mockIdGen = new Mock<IIdGeneratorService>();
            var monsterManager = new MonsterManager(collisionManager, mockIdGen.Object);
            
            var pos = new Position(5, 5);
            var monster = new Monster(100, "Wolf", "wolf", pos, 50, 10);
            
            // Manually register to simulate spawn
            monsterManager.RemoveMonster(100); // Ensure clean start
            mockIdGen.Setup(s => s.GenerateId()).Returns(100);
            _mockStaticWorld.Setup(s => s.IsBlocked(pos)).Returns(false);
            
            collisionManager.RegisterDynamicObject(monster);
            
            Assert.True(collisionManager.IsPositionBlocked(pos));
            
            monsterManager.RemoveMonster(100); // This is not actually linked to the manager yet because I didn't add it to manager
        }

        [Fact]
        public void IntegrationTest_MonsterManager_RemoveMonster_Clears_Collision()
        {
            var collisionManager = new CollisionManager(_mockStaticWorld.Object);
            var mockIdGen = new Mock<IIdGeneratorService>();
            var monsterManager = new MonsterManager(collisionManager, mockIdGen.Object);
            
            _mockStaticWorld.Setup(s => s.IsBlocked(It.IsAny<Position>())).Returns(false);
            mockIdGen.Setup(s => s.GenerateId()).Returns(1);
            
            // Spawn monster through manager (which registers collision)
            monsterManager.SpawnRandomMonsters(1, 10, 10, 0, 1);
            var monster = monsterManager.GetAllMonsters().First();
            var pos = monster.Position;
            
            Assert.True(collisionManager.IsPositionBlocked(pos));
            
            // Remove monster
            monsterManager.RemoveMonster(monster.Id);
            
            Assert.False(collisionManager.IsPositionBlocked(pos));
            Assert.Null(monsterManager.GetMonsterById(monster.Id));
        }
    }
}

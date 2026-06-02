using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using Moq;
using GameServerApp.Managers;
using GameServerApp.World;
using GameServer.Tests.Helpers;

namespace GameServer.Tests.Managers
{
    public class MonsterDeathTests
    {
        // Builder padrão (tudo mockado). Mocks acessíveis via _b.*
        private readonly WorldManagerBuilder _b = new();
        private readonly WorldManager _worldManager;

        public MonsterDeathTests()
        {
            _worldManager = _b.Build();
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
            var player  = new Player(1, "Hero", new Position(0, 0));
            var monster = new Monster(2, "Rat", "rat", new Position(1, 0), 10, 5);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);

            _worldManager.ProcessPlayerAttackMonster(player, "2");

            Assert.True(monster.IsDead);
            _b.MonsterManager.Verify(m => m.RemoveMonster(2), Times.Once);
        }

        [Fact]
        public void When_Monster_Dies_Collision_Should_Be_Released()
        {
            // Usa o CollisionManager compartilhado do builder principal para que
            // o novo WorldManager e o MonsterManager operem sobre o mesmo estado.
            var mockIdGen        = new Mock<IIdGeneratorService>();
            var realMonsterManager = new MonsterManager(_b.BuiltCollision!, mockIdGen.Object, BuildMonsterRepo());

            var wm = new WorldManagerBuilder()
                .WithCollision(_b.BuiltCollision!)
                .WithMonsterManager(realMonsterManager)
                .Build();

            mockIdGen.Setup(s => s.GenerateId()).Returns(2);
            _b.StaticWorld.Setup(s => s.IsBlocked(It.IsAny<Position>())).Returns(false);

            realMonsterManager.SpawnRandomMonsters(1, 10, 10, 0, 123);
            var monster   = realMonsterManager.GetAllMonsters().First();
            var monsterId = monster.Id;
            var pos       = monster.Position;

            Assert.True(_b.BuiltCollision!.IsPositionBlocked(pos));

            var player = new Player(1, "Hero", new Position(pos.X - 1, pos.Y), 100);
            wm.ProcessPlayerAttackMonster(player, monsterId.ToString());

            Assert.True(monster.IsDead, "Monster should be dead after attacks");
            Assert.False(_b.BuiltCollision!.IsPositionBlocked(pos));
            Assert.Null(realMonsterManager.GetMonsterById(monsterId));
        }

        [Fact]
        public void Monster_Removal_Releases_CollisionManager_Entry()
        {
            // Teste de unidade puro — usa objetos locais, sem WorldManager
            var mockIdGen      = new Mock<IIdGeneratorService>();
            var localCollision = new CollisionManager(_b.StaticWorld.Object);
            var monsterManager = new MonsterManager(localCollision, mockIdGen.Object, BuildMonsterRepo());

            var pos     = new Position(5, 5);
            var monster = new Monster(100, "Wolf", "wolf", pos, 50, 10);

            _b.StaticWorld.Setup(s => s.IsBlocked(pos)).Returns(false);
            localCollision.RegisterDynamicObject(monster);

            Assert.True(localCollision.IsPositionBlocked(pos));

            monsterManager.RemoveMonster(100);
        }

        [Fact]
        public void IntegrationTest_MonsterManager_RemoveMonster_Clears_Collision()
        {
            var mockIdGen      = new Mock<IIdGeneratorService>();
            var localCollision = new CollisionManager(_b.StaticWorld.Object);
            var monsterManager = new MonsterManager(localCollision, mockIdGen.Object, BuildMonsterRepo());

            _b.StaticWorld.Setup(s => s.IsBlocked(It.IsAny<Position>())).Returns(false);
            mockIdGen.Setup(s => s.GenerateId()).Returns(1);

            monsterManager.SpawnRandomMonsters(1, 10, 10, 0, 1);
            var monster = monsterManager.GetAllMonsters().First();
            var pos     = monster.Position;

            Assert.True(localCollision.IsPositionBlocked(pos));

            monsterManager.RemoveMonster(monster.Id);

            Assert.False(localCollision.IsPositionBlocked(pos));
            Assert.Null(monsterManager.GetMonsterById(monster.Id));
        }

        [Fact]
        public void When_Monster_Dies_It_Should_Respawn_After_Delay()
        {
            var mockIdGen          = new Mock<IIdGeneratorService>();
            var realMonsterManager = new MonsterManager(_b.BuiltCollision!, mockIdGen.Object, BuildMonsterRepo());

            var wm = new WorldManagerBuilder()
                .WithCollision(_b.BuiltCollision!)
                .WithMonsterManager(realMonsterManager)
                .Build();

            mockIdGen.Setup(s => s.GenerateId()).Returns(10);
            _b.StaticWorld.Setup(s => s.IsBlocked(It.IsAny<Position>())).Returns(false);

            realMonsterManager.SpawnRandomMonsters(1, 32, 32, 0, 123);
            var monster   = realMonsterManager.GetAllMonsters().First();
            var monsterId = monster.Id;

            var player = new Player(1, "Hero", new Position(monster.Position.X - 1, monster.Position.Y), 100);
            wm.ProcessPlayerAttackMonster(player, monsterId.ToString());

            Assert.True(monster.IsDead, "Monster should be dead after attacks");
            Assert.Null(realMonsterManager.GetMonsterById(monsterId));

            wm.Tick();
            Assert.Empty(realMonsterManager.GetAllMonsters());

            // Verifica que respawn via mock funciona (sem mock de tempo)
            var mockMm = new Mock<IMonsterManager>();
            var wm2 = new WorldManagerBuilder()
                .WithMonsterManager(mockMm.Object)
                .Build();

            mockMm.Setup(m => m.GetMonsterById(It.IsAny<long>())).Returns(monster);
            mockMm.Setup(m => m.GetAllMonsters()).Returns(Array.Empty<IMonster>());

            wm2.ProcessPlayerAttackMonster(player, monsterId.ToString());
        }

        [Fact]
        public void Monster_Death_Should_Potentially_Drop_Item()
        {
            var pos     = new Position(1, 0);
            var potion  = new HealingItem("999", "Healing Potion", 0.1f, "healing-potion", pos, ItemType.Potion);
            var player  = new Player(1, "Hero", new Position(0, 0), 100);
            var monster = new Monster(2, "Rat", "rat", pos, 10, 5);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);
            _b.IdGenerator   .Setup(s => s.GenerateId()).Returns(999);
            _b.LootTable     .Setup(l => l.RollLoot(pos, It.IsAny<string>(), "rat", null)).Returns(potion);

            bool dropped = false;
            _b.ItemManager.Setup(m => m.DropItem(It.IsAny<IItem>())).Callback(() => dropped = true);

            _worldManager.ProcessPlayerAttackMonster(player, "2");

            Assert.True(dropped, "Item deve ter dropado conforme retorno do LootTableService");
            _b.Events.Verify(e => e.OnItemDropped(It.IsAny<IItem>()), Times.Once);
        }

        private static IMonsterDefinitionRepository BuildMonsterRepo()
        {
            var mock = new Mock<IMonsterDefinitionRepository>();
            var defs = new List<MonsterDefinition>
            {
                new(1, "rat", "Rat", 30, 4, 20)
            };
            mock.Setup(r => r.GetAll()).Returns(defs);
            return mock.Object;
        }
    }
}

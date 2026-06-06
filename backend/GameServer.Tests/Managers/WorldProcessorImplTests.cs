using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using Moq;
using GameServerApp.Managers;
using GameServerApp.World;
using GameServerApp.Dtos;
using GameServer.Tests.Helpers;

namespace GameServer.Tests.Managers
{
    public class WorldProcessorImplTests
    {
        // Um único builder com serviços reais — mocks acessíveis via b.*
        private readonly WorldManagerBuilder _b = new WorldManagerBuilder().WithRealServices();
        private readonly WorldManager _worldManager;

        public WorldProcessorImplTests()
        {
            _worldManager = _b.Build();
        }

        [Fact]
        public void Movement_Should_Succeed_If_Path_Is_Free()
        {
            var startPos = new Position(0, 0);
            var player = new Player(1, "Hero", startPos);

            bool moved = _worldManager.ProcessPlayerMovement(player, "east");

            Assert.True(moved);
            Assert.Equal(1, player.Position.X);
            Assert.Equal(0, player.Position.Y);
        }

        [Fact]
        public void Movement_Should_Fail_If_Blocked_By_Wall()
        {
            var startPos = new Position(0, 0);
            var targetPos = new Position(1, 0);
            var player = new Player(1, "Hero", startPos);

            var wall = new StaticObject(2, targetPos, "wall", "wall", false);
            _worldManager.InstantiateObject(wall);

            bool moved = _worldManager.ProcessPlayerMovement(player, "east");

            Assert.False(moved);
            Assert.Equal(0, player.Position.X);
        }

        [Fact]
        public void Attack_Should_Damage_Target_And_Reward_Experience_On_Kill()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var monster = new Player(2, "Goblin", new Position(1, 0));
            monster.TakeDamage(95);

            long initialXp = player.Experience;

            _worldManager.ProcessPlayerAttack(player, monster);

            Assert.Equal(PlayerState.Dead, monster.State);
            Assert.True(player.Experience > initialXp);
        }

        [Fact]
        public void Dead_Player_Should_Not_Be_Able_To_Move()
        {
            var player = new Player(1, "Ghost", new Position(0, 0));
            player.Die();

            bool moved = _worldManager.ProcessPlayerMovement(player, "east");

            Assert.False(moved);
            Assert.Equal(0, player.Position.X);
        }

        [Fact]
        public void Movement_Should_Fail_If_Moving_Too_Fast()
        {
            var startPos = new Position(0, 0);
            var player = new Player(1, "Speedster", startPos);

            bool moved1 = _worldManager.ProcessPlayerMovement(player, "east");
            Assert.True(moved1);
            Assert.Equal(1, player.Position.X);

            bool moved2 = _worldManager.ProcessPlayerMovement(player, "east");
            Assert.False(moved2);
            Assert.Equal(1, player.Position.X);
        }

        [Fact]
        public void Movement_Should_Fail_If_Blocked_By_Player()
        {
            var startPos  = new Position(0, 0);
            var targetPos = new Position(1, 0);
            var player1   = new Player(1, "Hero",    startPos);
            var player2   = new Player(2, "Blocker", targetPos);

            _b.BuiltCollision!.RegisterDynamicObject(player2);

            bool moved = _worldManager.ProcessPlayerMovement(player1, "east");

            Assert.False(moved);
            Assert.Equal(0, player1.Position.X);
        }

        [Fact]
        public void Monster_Should_Attack_Adjacent_Player()
        {
            var player  = new Player(1, "Hero", new Position(0, 0));
            var monster = new Monster(2, "Rat", "rat", new Position(1, 1), 30, 10);
            monster.LastAttackTime = DateTime.UtcNow.AddSeconds(-2);

            _b.MonsterManager.Setup(m => m.GetAllMonsters()).Returns(new List<IMonster> { monster });
            _b.PlayerManager .Setup(m => m.GetAllPlayers()).Returns(new List<IPlayer>  { player  });

            int initialHp = player.Hp;

            _worldManager.Tick();

            Assert.Equal(initialHp - 10, player.Hp);
            _b.Events.Verify(e => e.OnPlayerAttacked(It.Is<PlayerAttackData>(a =>
                a.AttackerId == monster.Id.ToString() &&
                a.TargetId   == player.Id.ToString()  &&
                a.Damage     == 10)), Times.Once);
        }

        [Fact]
        public void Player_Should_Attack_Adjacent_Monster()
        {
            var player  = new Player(1, "Hero", new Position(0, 0));
            var monster = new Monster(2, "Rat", "rat", new Position(1, 0), 30, 10);

            _b.MonsterManager.Setup(m => m.GetMonsterById(2)).Returns(monster);

            int initialHp = monster.Hp;

            _worldManager.ProcessPlayerAttackMonster(player, "2");

            Assert.Equal(initialHp - 10, monster.Hp);
            _b.Events.Verify(e => e.OnPlayerAttacked(It.Is<PlayerAttackData>(a =>
                a.AttackerId == player.Id.ToString()  &&
                a.TargetId   == monster.Id.ToString() &&
                a.Damage     == 10)), Times.Once);
        }

        [Fact]
        public void ProcessChunkLoading_ShouldIncludeItems()
        {
            var player       = new Player(1, "Hero", new Position(0, 0));
            var connectionId = "test-conn";
            var item         = new Item("item1", "Potion", 0.1f, "potion", new Position(5, 5), ItemType.Potion);

            _b.ItemManager.Setup(m => m.GetItemsInChunk(It.IsAny<ChunkCoord>()))
                          .Returns(new List<IItem> { item });

            _worldManager.ProcessChunkLoading(player, connectionId);

            // Mapa default 32×32, ChunkSize 16, LoadChunks 9 (3×3), player em (0,0):
            // dos 9 chunks vizinhos só os 4 dentro dos limites (cx,cy ∈ {0,1}) são carregados.
            _b.Events.Verify(e => e.OnChunkLoaded(connectionId, It.Is<ChunkData>(d =>
                d.Items.Any(i => i.Id == "item1" && i.Name == "Potion"))), Times.Exactly(4));
        }
    }
}

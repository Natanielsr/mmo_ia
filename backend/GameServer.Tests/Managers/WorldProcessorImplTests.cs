using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using Moq;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    public class WorldProcessorImplTests
    {
        private readonly WorldManager _worldManager;
        private readonly MovementService _movementService = new();
        private readonly StaticWorldManager staticWorldManager = new();
        private readonly CollisionManager _collisionManager;
        private readonly CombatService _combatService = new();
        private readonly GameStateManager _gameStateManager;
        private readonly Mock<IWorldEvents> _mockEvents = new();



        public WorldProcessorImplTests()
        {
            _collisionManager = new(staticWorldManager);
            _gameStateManager = new GameStateManager(_collisionManager);
            _worldManager = new WorldManager(
                _movementService,
                _collisionManager,
                _combatService,
                _gameStateManager,
                _mockEvents.Object,
                staticWorldManager
            );
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

            // Register a wall
            var wall = new StaticObject(2, targetPos, "wall", false);
            _worldManager.InstantiateObject(wall);

            bool moved = _worldManager.ProcessPlayerMovement(player, "east");

            Assert.False(moved);
            Assert.Equal(0, player.Position.X); // Stayed at 0
        }

        [Fact]
        public void Attack_Should_Damage_Target_And_Reward_Experience_On_Kill()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var monster = new Player(2, "Goblin", new Position(1, 0)); // Using Player class as monster for now
            monster.TakeDamage(95); // Leave goblin with 5 HP

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
            var player = new Player(1, "Speedster", startPos); // Speed is 2.0 (1 move every 0.5s)

            // First move should succeed
            bool moved1 = _worldManager.ProcessPlayerMovement(player, "east");
            Assert.True(moved1);
            Assert.Equal(1, player.Position.X);

            // Immediate second move should fail due to speed limit
            bool moved2 = _worldManager.ProcessPlayerMovement(player, "east");
            Assert.False(moved2);
            Assert.Equal(1, player.Position.X); // Position should not change
        }

        [Fact]
        public void Movement_Should_Fail_If_Blocked_By_Player()
        {
            var startPos = new Position(0, 0);
            var targetPos = new Position(1, 0);
            var player1 = new Player(1, "Hero", startPos);
            var player2 = new Player(2, "Blocker", targetPos);

            // Register player2 as an obstacle
            _collisionManager.RegisterDynamicObject(player2);

            bool moved = _worldManager.ProcessPlayerMovement(player1, "east");

            Assert.False(moved);
            Assert.Equal(0, player1.Position.X); // Stayed at 0
        }
    }
}

using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    public class WorldProcessorImplTests
    {
        private readonly WorldProcessor _processor;
        private readonly MovementService _movementService = new();
        private readonly CollisionManager _collisionManager = new();
        private readonly CombatService _combatService = new();
        private readonly GameStateManager _gameStateManager = new();

        public WorldProcessorImplTests()
        {
            _processor = new WorldProcessor(
                _movementService,
                _collisionManager,
                _combatService,
                _gameStateManager
            );
        }

        [Fact]
        public void Movement_Should_Succeed_If_Path_Is_Free()
        {
            var startPos = new Position(0, 0);
            var player = new Player("Hero", startPos);
            
            bool moved = _processor.ProcessPlayerMovement(player, "east");
            
            Assert.True(moved);
            Assert.Equal(1, player.Position.X);
            Assert.Equal(0, player.Position.Y);
        }

        [Fact]
        public void Movement_Should_Fail_If_Blocked_By_Wall()
        {
            var startPos = new Position(0, 0);
            var targetPos = new Position(1, 0);
            var player = new Player("Hero", startPos);
            
            // Register a wall
            var wall = new WorldObject("wall_1", ObjectType.Wall, targetPos, false);
            _collisionManager.RegisterObject(wall);
            
            bool moved = _processor.ProcessPlayerMovement(player, "east");
            
            Assert.False(moved);
            Assert.Equal(0, player.Position.X); // Stayed at 0
        }

        [Fact]
        public void Attack_Should_Damage_Target_And_Reward_Experience_On_Kill()
        {
            var player = new Player("Hero", new Position(0, 0));
            var monster = new Player("Goblin", new Position(1, 0)); // Using Player class as monster for now
            monster.TakeDamage(95); // Leave goblin with 5 HP
            
            long initialXp = player.Experience;
            
            _processor.ProcessPlayerAttack(player, monster);
            
            Assert.Equal(PlayerState.Dead, monster.State);
            Assert.True(player.Experience > initialXp);
        }

        [Fact]
        public void Dead_Player_Should_Not_Be_Able_To_Move()
        {
            var player = new Player("Ghost", new Position(0, 0));
            player.Die();
            
            bool moved = _processor.ProcessPlayerMovement(player, "east");
            
            Assert.False(moved);
            Assert.Equal(0, player.Position.X);
        }
    }
}

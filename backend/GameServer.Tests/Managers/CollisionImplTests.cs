using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    public class CollisionImplTests
    {
        private readonly CollisionManager _collisionManager = new();

        [Fact]
        public void CollisionManager_Should_Register_And_Find_Object()
        {
            var pos = new Position(10, 10);
            var wall = new WorldObject(1, "wall", ObjectType.Wall, pos, false);

            _collisionManager.RegisterObject(wall);

            var found = _collisionManager.GetObjectAt(pos);
            Assert.Same(wall, found);
            Assert.True(_collisionManager.IsPositionBlocked(pos));
        }

        [Fact]
        public void CollisionManager_Should_Block_Obstacles_But_Not_Passables()
        {
            var posBlocked = new Position(1, 1);
            var posFree = new Position(2, 2);

            var wall = new WorldObject(1, "wall", ObjectType.Wall, posBlocked, false);
            var openDoor = new WorldObject(2, "door", ObjectType.Door, posFree, true);

            _collisionManager.RegisterObject(wall);
            _collisionManager.RegisterObject(openDoor);

            Assert.True(_collisionManager.IsPositionBlocked(posBlocked));
            Assert.False(_collisionManager.IsPositionBlocked(posFree));
            Assert.True(_collisionManager.CanPlayerMove(new Position(0, 0), posFree));
            Assert.False(_collisionManager.CanPlayerMove(new Position(0, 0), posBlocked));
        }

        [Fact]
        public void CollisionManager_Should_Remove_Object()
        {
            var obstacleId = 1;
            var pos = new Position(5, 5);
            var obstacle = new WorldObject(obstacleId, "obstacle", ObjectType.Obstacle, pos, false);

            _collisionManager.RegisterObject(obstacle);
            Assert.True(_collisionManager.IsPositionBlocked(pos));

            _collisionManager.RemoveObject(obstacleId);
            Assert.False(_collisionManager.IsPositionBlocked(pos));
            Assert.Null(_collisionManager.GetObjectAt(pos));
        }

        [Fact]
        public void Unregistered_Position_Should_Not_Be_Blocked()
        {
            var pos = new Position(100, 100);
            Assert.False(_collisionManager.IsPositionBlocked(pos));
        }

        [Fact]
        public void CollisionManager_Should_Handle_Multiple_Objects_At_Same_Position()
        {
            var spawnPos = new Position(0, 0);
            var targetPos = new Position(1, 0);

            var player1 = new Player(1, "Player 1", spawnPos);
            var player2 = new Player(2, "Player 2", spawnPos);

            _collisionManager.RegisterObject(player1);
            _collisionManager.RegisterObject(player2);

            // Both at spawn: position is blocked
            Assert.True(_collisionManager.IsPositionBlocked(spawnPos));

            // Move player 2 away
            Position oldPos = player2.Position;
            player2.Move(targetPos);
            _collisionManager.UpdateObjectPosition(player2, oldPos);

            // spawnPos should STILL be blocked by player 1
            Assert.True(_collisionManager.IsPositionBlocked(spawnPos), "Spawn position should still be blocked by Player 1 after Player 2 moves.");

            // targetPos should also be blocked by player 2
            Assert.True(_collisionManager.IsPositionBlocked(targetPos));

            // Remove player 1
            _collisionManager.RemoveObject(player1.Id);

            // Now spawnPos should be free
            Assert.False(_collisionManager.IsPositionBlocked(spawnPos));
        }
    }
}

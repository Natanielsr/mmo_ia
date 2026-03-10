using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    public class CollisionImplTests
    {
        private readonly StaticWorldManager _staticWorldManager = new();
        private readonly CollisionManager _collisionManager;

        public CollisionImplTests()
        {
            _collisionManager = new(_staticWorldManager);
        }

        [Fact]
        public void CollisionManager_Should_Register_And_Find_Object()
        {
            var pos = new Position(10, 10);
            var wall = new StaticObject(1, pos, "wall", "wall", false);

            _staticWorldManager.AddStaticObject(wall);

            var found = _collisionManager.GetObjectAt(pos);
            Assert.Same(wall, found);
            Assert.True(_collisionManager.IsPositionBlocked(pos));
        }

        [Fact]
        public void CollisionManager_Should_Block_Obstacles_But_Not_Passables()
        {
            var posBlocked = new Position(1, 1);
            var posFree = new Position(2, 2);

            var wall = new StaticObject(1, posBlocked, "wall", "wall", false, false);
            var openDoor = new StaticObject(2, posFree, "door", "wall", true, true);

            _staticWorldManager.AddStaticObject(wall);
            _staticWorldManager.AddStaticObject(openDoor);

            Assert.True(_collisionManager.IsPositionBlocked(posBlocked));
            Assert.False(_collisionManager.IsPositionBlocked(posFree));
            Assert.True(_collisionManager.CanPlayerMove(new Position(0, 0), posFree));
            Assert.False(_collisionManager.CanPlayerMove(new Position(0, 0), posBlocked));
        }

        [Fact]
        public void CollisionManager_Should_Remove_Dynamic_Object()
        {
            var id = 1;
            var pos = new Position(5, 5);
            var player = new Player(id, "player", pos);

            _collisionManager.RegisterDynamicObject(player);
            Assert.True(_collisionManager.IsPositionBlocked(pos));

            _collisionManager.RemoveObject(id);
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

            _collisionManager.RegisterDynamicObject(player1);
            _collisionManager.RegisterDynamicObject(player2);

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

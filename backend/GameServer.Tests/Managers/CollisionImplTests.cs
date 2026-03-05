using Xunit;
using GameServerApp.Contracts;
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
            var wall = new WorldObject("wall_1", ObjectType.Wall, pos, false);
            
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
            
            var wall = new WorldObject("wall", ObjectType.Wall, posBlocked, false);
            var openDoor = new WorldObject("door", ObjectType.Door, posFree, true);
            
            _collisionManager.RegisterObject(wall);
            _collisionManager.RegisterObject(openDoor);
            
            Assert.True(_collisionManager.IsPositionBlocked(posBlocked));
            Assert.False(_collisionManager.IsPositionBlocked(posFree));
            Assert.True(_collisionManager.CanPlayerMove(new Position(0,0), posFree));
            Assert.False(_collisionManager.CanPlayerMove(new Position(0,0), posBlocked));
        }

        [Fact]
        public void CollisionManager_Should_Remove_Object()
        {
            var pos = new Position(5, 5);
            var obstacle = new WorldObject("obs", ObjectType.Obstacle, pos, false);
            
            _collisionManager.RegisterObject(obstacle);
            Assert.True(_collisionManager.IsPositionBlocked(pos));
            
            _collisionManager.RemoveObject("obs");
            Assert.False(_collisionManager.IsPositionBlocked(pos));
            Assert.Null(_collisionManager.GetObjectAt(pos));
        }

        [Fact]
        public void Unregistered_Position_Should_Not_Be_Blocked()
        {
            var pos = new Position(100, 100);
            Assert.False(_collisionManager.IsPositionBlocked(pos));
        }
    }
}

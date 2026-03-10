// backend/GameServer.Tests/Managers/StaticWorldManagerImplTests.cs
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers
{
    /// <summary>
    /// Testes para a implementação concreta de StaticWorldManager
    /// </summary>
    public class StaticWorldManagerImplTests
    {
        private readonly IStaticWorldManager _manager;

        public StaticWorldManagerImplTests()
        {
            _manager = new StaticWorldManager();
        }

        [Fact]
        public void Constructor_ShouldCreateEmptyManager()
        {
            // Assert
            Assert.NotNull(_manager);
        }

        [Fact]
        public void IsBlocked_ShouldReturnFalse_ForEmptyPosition()
        {
            // Arrange
            var position = new Position(10, 20);

            // Act
            var result = _manager.IsBlocked(position);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsPassable_ShouldReturnTrue_ForEmptyPosition()
        {
            // Arrange
            var position = new Position(10, 20);

            // Act
            var result = _manager.IsPassable(position);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddStaticObject_ShouldStoreObject()
        {
            // Arrange
            var position = new Position(10, 10);
            var staticObject = new StaticObject(1, position, "Tree", "Tree", isPassable: false);

            // Act
            _manager.AddStaticObject(staticObject);

            // Assert
            var retrieved = _manager.GetObjectAt(position);
            Assert.NotNull(retrieved);
            Assert.Equal(staticObject.Id, retrieved.Id);
            Assert.Equal(staticObject.Name, retrieved.Name);
        }

        [Fact]
        public void AddStaticObject_WhenNotPassable_ShouldAlsoBlockPosition()
        {
            // Arrange
            var position = new Position(10, 10);
            var staticObject = new StaticObject(1, position, "Wall", "Wall", isPassable: false);

            // Act
            _manager.AddStaticObject(staticObject);

            // Assert
            Assert.True(_manager.IsBlocked(position));
            Assert.False(_manager.IsPassable(position));
        }

        [Fact]
        public void AddStaticObject_WhenPassable_ShouldNotBlockPosition()
        {
            // Arrange
            var position = new Position(10, 10);
            var staticObject = new StaticObject(1, position, "Flower", "Flower", isPassable: true);

            // Act
            _manager.AddStaticObject(staticObject);

            // Assert
            Assert.False(_manager.IsBlocked(position));
            Assert.True(_manager.IsPassable(position));
        }

        [Fact]
        public void AddStaticObject_ShouldReplaceExistingObjectAtSamePosition()
        {
            // Arrange
            var position = new Position(10, 10);
            var obj1 = new StaticObject(1, position, "Tree", "Tree");
            var obj2 = new StaticObject(2, position, "Rock", "Rock");

            // Act
            _manager.AddStaticObject(obj1);
            _manager.AddStaticObject(obj2);

            // Assert
            var retrieved = _manager.GetObjectAt(position);
            Assert.NotNull(retrieved);
            Assert.Equal(2, retrieved.Id); // replaced
            Assert.Equal("Rock", retrieved.Name);
        }

        [Fact]
        public void RemoveObjectAt_ShouldRemoveObjectAndUnblockPosition()
        {
            // Arrange
            var position = new Position(10, 10);
            var staticObject = new StaticObject(1, position, "Tree", "Tree", isPassable: false);
            _manager.AddStaticObject(staticObject);

            // Act
            var result = _manager.RemoveObjectAt(position);

            // Assert
            Assert.True(result);
            Assert.Null(_manager.GetObjectAt(position));
            Assert.False(_manager.IsBlocked(position));
            Assert.True(_manager.IsPassable(position));
        }

        [Fact]
        public void RemoveObjectAt_ShouldReturnFalse_WhenNoObject()
        {
            // Arrange
            var position = new Position(999, 999);

            // Act
            var result = _manager.RemoveObjectAt(position);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RemoveObjectAt_ShouldKeepPositionPassable_IfObjectWasPassable()
        {
            // Arrange
            var position = new Position(10, 10);
            var staticObject = new StaticObject(1, position, "Flower", "Flower", isPassable: true);
            _manager.AddStaticObject(staticObject);

            // Act
            _manager.RemoveObjectAt(position);

            // Assert
            Assert.False(_manager.IsBlocked(position));
            Assert.True(_manager.IsPassable(position));
        }

        [Fact]
        public void GetObjectAt_ShouldReturnNull_ForEmptyPosition()
        {
            // Arrange
            var position = new Position(100, 100);

            // Act
            var result = _manager.GetObjectAt(position);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetObjectsInArea_ShouldReturnAllObjectsInArea()
        {
            // Arrange
            _manager.AddStaticObject(new StaticObject(1, new Position(5, 5), "Tree1", "Tree"));
            _manager.AddStaticObject(new StaticObject(2, new Position(10, 10), "Tree2", "Tree"));
            _manager.AddStaticObject(new StaticObject(3, new Position(20, 20), "Tree3", "Tree"));
            _manager.AddStaticObject(new StaticObject(4, new Position(3, 8), "Tree4", "Tree"));

            var topLeft = new Position(0, 0);
            var bottomRight = new Position(15, 15);

            // Act
            var result = _manager.GetObjectsInArea(topLeft, bottomRight).ToList();

            // Assert
            Assert.Equal(3, result.Count); // (5,5), (10,10),
        }
    }
}
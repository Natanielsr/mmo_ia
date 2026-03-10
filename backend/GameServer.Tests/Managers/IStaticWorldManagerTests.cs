// backend/GameServer.Tests/Managers/IStaticWorldManagerTests.cs
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Managers
{
    /// <summary>
    /// Testes para a interface IStaticWorldManager
    /// Testa o contrato/comportamento esperado
    /// </summary>
    public class IStaticWorldManagerTests
    {
        private readonly Mock<IStaticWorldManager> _mockManager;

        public IStaticWorldManagerTests()
        {
            _mockManager = new Mock<IStaticWorldManager>();
        }

        [Fact]
        public void IsBlocked_ShouldReturnTrue_WhenPositionIsBlocked()
        {
            // Arrange
            var position = new Position(10, 20);
            _mockManager.Setup(m => m.IsBlocked(position)).Returns(true);

            // Act
            var result = _mockManager.Object.IsBlocked(position);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPassable_ShouldReturnFalse_WhenPositionIsBlocked()
        {
            // Arrange
            var position = new Position(10, 20);
            _mockManager.Setup(m => m.IsPassable(position)).Returns(false);

            // Act
            var result = _mockManager.Object.IsPassable(position);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetObjectAt_ShouldReturnObject_WhenObjectExists()
        {
            // Arrange
            var position = new Position(5, 5);
            var expectedObject = new StaticObject(1, position, "Tree", "Tree", isPassable: false);
            _mockManager.Setup(m => m.GetObjectAt(position)).Returns(expectedObject);

            // Act
            var result = _mockManager.Object.GetObjectAt(position);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedObject.Id, result.Id);
            Assert.Equal(expectedObject.Name, result.Name);
        }

        [Fact]
        public void GetObjectAt_ShouldReturnNull_WhenNoObjectAtPosition()
        {
            // Arrange
            var position = new Position(100, 100);
            _mockManager.Setup(m => m.GetObjectAt(position)).Returns((StaticObject?)null);

            // Act
            var result = _mockManager.Object.GetObjectAt(position);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetObjectsInArea_ShouldReturnOnlyObjectsInArea()
        {
            // Arrange
            var topLeft = new Position(0, 0);
            var bottomRight = new Position(10, 10);

            var obj1 = new StaticObject(1, new Position(5, 5), "Tree1", "Tree");
            var obj2 = new StaticObject(2, new Position(15, 15), "Tree2", "Tree"); // fora da área
            var obj3 = new StaticObject(3, new Position(3, 3), "Tree3", "Tree");

            _mockManager.Setup(m => m.GetObjectsInArea(topLeft, bottomRight))
                .Returns(new List<StaticObject> { obj1, obj3 });

            // Act
            var result = _mockManager.Object.GetObjectsInArea(topLeft, bottomRight).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(obj1, result);
            Assert.Contains(obj3, result);
            Assert.DoesNotContain(obj2, result);
        }

        [Fact]
        public void AddStaticObject_ShouldBeCalled()
        {
            // Arrange
            var obj = new StaticObject(1, new Position(5, 5), "Tree", "Tree");
            _mockManager.Setup(m => m.AddStaticObject(obj));

            // Act
            _mockManager.Object.AddStaticObject(obj);

            // Assert
            _mockManager.Verify(m => m.AddStaticObject(obj), Times.Once);
        }

        [Fact]
        public void RemoveObjectAt_ShouldReturnTrue_WhenObjectExists()
        {
            // Arrange
            var position = new Position(10, 10);
            _mockManager.Setup(m => m.RemoveObjectAt(position)).Returns(true);

            // Act
            var result = _mockManager.Object.RemoveObjectAt(position);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void RemoveObjectAt_ShouldReturnFalse_WhenNoObject()
        {
            // Arrange
            var position = new Position(999, 999);
            _mockManager.Setup(m => m.RemoveObjectAt(position)).Returns(false);

            // Act
            var result = _mockManager.Object.RemoveObjectAt(position);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Clear_ShouldBeCalled()
        {
            // Arrange
            _mockManager.Setup(m => m.Clear());

            // Act
            _mockManager.Object.Clear();

            // Assert
            _mockManager.Verify(m => m.Clear(), Times.Once);
        }
    }
}
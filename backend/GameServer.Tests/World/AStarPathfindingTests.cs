using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Services;
using Moq;
using Xunit;

namespace GameServer.Tests.World
{
    public class AStarPathfindingTests
    {
        private readonly Mock<ICollisionManager> _collisionManagerMock;
        private readonly AStarPathfindingService _pathfinding;

        public AStarPathfindingTests()
        {
            _collisionManagerMock = new Mock<ICollisionManager>();
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>())).Returns(false);
            _pathfinding = new AStarPathfindingService(_collisionManagerMock.Object);
        }

        [Fact]
        public void FindPath_DirectPath_ReturnsShortestRoute()
        {
            // Arrange
            var start = new Position(0, 0);
            var goal = new Position(3, 0);

            // Act
            var path = _pathfinding.FindPath(start, goal);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(3, path.Count); // 3 passos para a direita
            Assert.Equal(new Position(1, 0), path[0]);
            Assert.Equal(new Position(2, 0), path[1]);
            Assert.Equal(new Position(3, 0), path[2]);
        }

        [Fact]
        public void FindPath_AroundWall_FindsAlternativeRoute()
        {
            // Arrange: parede vertical bloqueando o caminho direto
            //   S . # . G
            //   . . # . .
            //   . . . . .
            var start = new Position(0, 2);
            var goal = new Position(4, 2);

            // Bloqueia posições (2,2) e (2,1) formando uma parede
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(2, 2))).Returns(true);
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(2, 1))).Returns(true);

            // Act
            var path = _pathfinding.FindPath(start, goal);

            // Assert
            Assert.NotNull(path);
            Assert.True(path.Count > 0);
            Assert.Equal(goal, path[^1]); // Último passo é o destino

            // Verifica que nenhum passo do caminho passa pelos bloqueios
            foreach (var step in path)
            {
                Assert.NotEqual(new Position(2, 2), step);
                Assert.NotEqual(new Position(2, 1), step);
            }
        }

        [Fact]
        public void FindPath_CompletelyBlocked_ReturnsNull()
        {
            // Arrange: O monstro está cercado por paredes
            var start = new Position(5, 5);
            var goal = new Position(8, 5);

            // Bloqueia todas as posições adjacentes ao start
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(5, 6))).Returns(true);
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(5, 4))).Returns(true);
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(6, 5))).Returns(true);
            _collisionManagerMock.Setup(c => c.IsPositionBlocked(new Position(4, 5))).Returns(true);

            // Act
            var path = _pathfinding.FindPath(start, goal);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_MaxDepthExceeded_ReturnsNull()
        {
            // Arrange: Destino muito longe com profundidade máxima baixa
            var start = new Position(0, 0);
            var goal = new Position(100, 100);

            // Act
            var path = _pathfinding.FindPath(start, goal, maxSearchDepth: 5);

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindPath_SamePosition_ReturnsEmptyList()
        {
            // Arrange
            var pos = new Position(5, 5);

            // Act
            var path = _pathfinding.FindPath(pos, pos);

            // Assert
            Assert.NotNull(path);
            Assert.Empty(path);
        }

        [Fact]
        public void FindPath_DiagonalMovement_UsesManhattanSteps()
        {
            // Arrange: destino diagonal (requer mover tanto em X quanto em Y)
            var start = new Position(0, 0);
            var goal = new Position(2, 2);

            // Act
            var path = _pathfinding.FindPath(start, goal);

            // Assert
            Assert.NotNull(path);
            Assert.Equal(4, path.Count); // Manhattan distance = |2|+|2| = 4 passos
            Assert.Equal(goal, path[^1]);
        }
    }
}

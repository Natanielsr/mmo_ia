using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.Services;
using GameServerApp.World;
using Moq;
using Xunit;

namespace GameServer.Tests.World
{
    public class MonsterAggroTests
    {
        private readonly Mock<ICollisionManager> _collisionManagerMock;
        private readonly Mock<IMovementService> _movementServiceMock;
        private readonly IPlayerManager _playerManager;
        private readonly MonsterMovementService _monsterMovementService;

        public MonsterAggroTests()
        {
            _collisionManagerMock = new Mock<ICollisionManager>();
            _movementServiceMock = new Mock<IMovementService>();
            _playerManager = new PlayerManager();

            // A* pathfinding real para os testes de aggro
            var pathfindingService = new AStarPathfindingService(_collisionManagerMock.Object);

            _monsterMovementService = new MonsterMovementService(
                _collisionManagerMock.Object,
                _movementServiceMock.Object,
                _playerManager,
                pathfindingService);
        }

        [Fact]
        public void Monster_ShouldChase_PlayerWithinAggroRange()
        {
            // Arrange
            var monsterPos = new Position(10, 10);
            var monster = new Monster(1, "Rat", "rat", monsterPos);
            
            var playerPos = new Position(12, 12); // Dentro do range de 5
            var player = new Player(100, "Player1", playerPos);
            _playerManager.AddPlayer("conn1", player);

            _movementServiceMock.Setup(m => m.Move(monsterPos, It.IsAny<string>()))
                .Returns((Position p, string d) => d == "east" ? new Position(11, 10) : new Position(10, 11));

            // Act
            var newPos = _monsterMovementService.CalculateNewPosition(monster);

            // Assert
            // dx=2, dy=2 -> Chebyshev distance = 2 (dentro do range 5)
            // Deve mover para (11, 10) ou (10, 11) dependendo da prioridade dx/dy
            Assert.NotEqual(monsterPos, newPos);
            Assert.True(newPos.X == 11 || newPos.Y == 11);
        }

        [Fact]
        public void Monster_ShouldNotChase_PlayerOutsideAggroRange()
        {
            // Arrange
            var monsterPos = new Position(10, 10);
            var monster = new Monster(1, "Rat", "rat", monsterPos);
            monster.SetBehavior(MonsterBehavior.Passive); // Para evitar movimento aleatório interferir no teste

            var playerPos = new Position(20, 20); // Fora do range de 5
            var player = new Player(100, "Player1", playerPos);
            _playerManager.AddPlayer("conn1", player);

            // Act
            _movementServiceMock.Setup(m => m.Move(It.IsAny<Position>(), It.IsAny<string>()))
                .Returns((Position p, string d) => p); // Garante que não retorne null em movimentos aleatórios

            var newPos = _monsterMovementService.CalculateNewPosition(monster);

            // Assert
            // Distância é 10, range é 5. Deve manter posição (ou patrulhar se fosse Patrolling)
            Assert.Equal(monsterPos, newPos);
        }
    }
}

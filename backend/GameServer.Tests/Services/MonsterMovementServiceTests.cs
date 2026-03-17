using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Services;
using GameServerApp.World;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace GameServer.Tests.Services
{
    public class MonsterMovementServiceTests
    {
        private readonly Mock<ICollisionManager> _mockCollision = new();
        private readonly Mock<IMovementService> _mockMovement = new();
        private readonly Mock<IPlayerManager> _mockPlayerManager = new();
        private readonly Mock<IPathfindingService> _mockPathfinding = new();
        private readonly MonsterMovementService _service;

        public MonsterMovementServiceTests()
        {
            _service = new MonsterMovementService(
                _mockCollision.Object,
                _mockMovement.Object,
                _mockPlayerManager.Object,
                _mockPathfinding.Object
            );
        }

        [Fact]
        public void Monster_Should_Return_To_Spawn_Using_Pathfinding_If_Outside_Range()
        {
            // Initial position (spawn) is (0,0)
            var spawnPos = new Position(0, 0);
            var monster = new Monster(1, "Wolf", "wolf", spawnPos);
            
            // "Teleport" the monster far from spawn
            var currentPos = new Position(15, 15); // Exceeds new MAX_MOVEMENT_RANGE (12)
            monster.Move(currentPos);
            monster.LastMovementTime = DateTime.UtcNow.AddSeconds(-2); // Ensure cooldown is met

            var nextStep = new Position(14, 15); // A* should pick a step closer
            
            // The service will look for players first
            _mockPlayerManager.Setup(p => p.GetAllPlayers()).Returns(new List<IPlayer>());
            
            // Pathfinding should be called for return trip
            _mockPathfinding.Setup(p => p.FindPath(currentPos, spawnPos, It.IsAny<int>()))
                           .Returns(new List<Position> { nextStep });
            
            _mockCollision.Setup(c => c.IsPositionBlocked(nextStep)).Returns(false);

            // Act
            _service.UpdateMonster(monster);

            // Assert
            Assert.Equal(nextStep, monster.Position);
        }

        [Fact]
        public void Monster_Should_Update_LastMovementTime_Even_If_Blocked()
        {
            var pos = new Position(0, 0);
            var monster = new Monster(1, "Wolf", "wolf", pos);
            var initialTime = DateTime.UtcNow.AddSeconds(-2);
            monster.LastMovementTime = initialTime;

            // Setup a move that will be blocked
            var targetPos = new Position(1, 0);
            _mockMovement.Setup(m => m.Move(pos, It.IsAny<string>())).Returns(targetPos);
            _mockCollision.Setup(c => c.IsPositionBlocked(targetPos)).Returns(true);
            _mockPlayerManager.Setup(p => p.GetAllPlayers()).Returns(new List<IPlayer>());

            // Act
            _service.UpdateMonster(monster);

            // Assert
            Assert.Equal(pos, monster.Position); // Didn't move
            Assert.True(monster.LastMovementTime > initialTime); // But timer updated
        }

        [Fact]
        public void Monster_Should_Not_Move_If_Cooldown_Not_Met()
        {
            var pos = new Position(0, 0);
            var monster = new Monster(1, "Wolf", "wolf", pos);
            monster.LastMovementTime = DateTime.UtcNow; // Just moved

            // Setup movement
            _mockPlayerManager.Setup(p => p.GetAllPlayers()).Returns(new List<IPlayer>());

            // Act
            _service.UpdateMonster(monster);

            // Assert
            _mockCollision.Verify(c => c.IsPositionBlocked(It.IsAny<Position>()), Times.Never);
        }
    }
}

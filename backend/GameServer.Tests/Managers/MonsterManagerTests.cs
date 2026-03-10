using Moq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers;

public class MonsterManagerTests
{
    private readonly Mock<ICollisionManager> _collisionManagerMock;
    private readonly Mock<IIdGeneratorService> _idGeneratorServiceMock;
    private readonly MonsterManager _monsterManager;

    public MonsterManagerTests()
    {
        _collisionManagerMock = new Mock<ICollisionManager>();
        _idGeneratorServiceMock = new Mock<IIdGeneratorService>();
        _monsterManager = new MonsterManager(_collisionManagerMock.Object, _idGeneratorServiceMock.Object);
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Spawn_Correct_Number_Of_Monsters()
    {
        // Arrange
        const int count = 5;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 12345;
        var idCounter = 1000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Assert
        Assert.Equal(count, spawnedMonsters.Count);
        Assert.All(spawnedMonsters, monster => Assert.NotNull(monster));
        _collisionManagerMock.Verify(c => c.RegisterDynamicObject(It.IsAny<IMonster>()), Times.Exactly(count));
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Not_Spawn_In_SafeZone()
    {
        // Arrange
        const int count = 10;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 20;
        var seed = 54321;
        var idCounter = 2000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Assert
        Assert.All(spawnedMonsters, monster =>
        {
            Assert.True(
                Math.Abs(monster.Position.X) > safeSpawnRadius ||
                Math.Abs(monster.Position.Y) > safeSpawnRadius,
                $"Monster spawned in safe zone at position ({monster.Position.X}, {monster.Position.Y})"
            );
        });
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Not_Spawn_On_Blocked_Positions()
    {
        // Arrange
        const int count = 3;
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 99999;
        var idCounter = 3000L;
        var blockedPositions = new HashSet<Position>
        {
            new Position(10, 10),
            new Position(20, 20),
            new Position(30, 30)
        };

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns<Position>(pos => blockedPositions.Contains(pos));

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Assert
        Assert.All(spawnedMonsters, monster =>
        {
            Assert.False(blockedPositions.Contains(monster.Position),
                $"Monster spawned on blocked position ({monster.Position.X}, {monster.Position.Y})");
        });
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Handle_Invalid_Parameters()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _monsterManager.SpawnRandomMonsters(0, width, height, safeSpawnRadius));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _monsterManager.SpawnRandomMonsters(-5, width, height, safeSpawnRadius));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _monsterManager.SpawnRandomMonsters(10, 0, height, safeSpawnRadius));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _monsterManager.SpawnRandomMonsters(10, width, -5, safeSpawnRadius));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _monsterManager.SpawnRandomMonsters(10, width, height, -1));
    }

    [Fact]
    public void GetAllMonsters_Should_Return_All_Spawned_Monsters()
    {
        // Arrange
        const int count = 3;
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 77777;
        var idCounter = 4000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.Equal(count, allMonsters.Count);
        foreach (var monster in spawnedMonsters)
        {
            Assert.Contains(monster, allMonsters);
        }
    }

    [Fact]
    public void GetMonsterById_Should_Return_Correct_Monster()
    {
        // Arrange
        const int count = 3;
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 88888;
        var ids = new List<long> { 5000, 5001, 5002 };
        var currentIdIndex = 0;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => ids[currentIdIndex++]);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        _ = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Act & Assert
        foreach (var expectedId in ids)
        {
            var monster = _monsterManager.GetMonsterById(expectedId);
            Assert.NotNull(monster);
            Assert.Equal(expectedId, monster.Id);
        }

        // Test non-existent monster
        var nonExistentMonster = _monsterManager.GetMonsterById(9999);
        Assert.Null(nonExistentMonster);
    }

    [Fact]
    public void RemoveMonster_Should_Remove_Monster_From_Manager_And_CollisionManager()
    {
        // Arrange
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 66666;
        var monsterId = 6000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var spawnedMonster = spawnedMonsters.First();

        // Act
        var removed = _monsterManager.RemoveMonster(monsterId);

        // Assert
        Assert.True(removed);
        Assert.Null(_monsterManager.GetMonsterById(monsterId));
        _collisionManagerMock.Verify(c => c.RemoveObject(monsterId), Times.Once);
    }

    [Fact]
    public void RemoveMonster_Should_Return_False_For_NonExistent_Monster()
    {
        // Arrange
        var nonExistentMonsterId = 9999L;

        // Act
        var removed = _monsterManager.RemoveMonster(nonExistentMonsterId);

        // Assert
        Assert.False(removed);
        _collisionManagerMock.Verify(c => c.RemoveObject(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Monster_Should_Have_Correct_Properties_From_Template()
    {
        // Arrange
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 55555;
        var monsterId = 7000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var monster = spawnedMonsters.First();

        // Assert
        Assert.NotNull(monster);
        Assert.Equal(monsterId, monster.Id);
        Assert.False(string.IsNullOrWhiteSpace(monster.Name));
        Assert.False(string.IsNullOrWhiteSpace(monster.ObjectCode));
        Assert.True(monster.Hp > 0);
        Assert.True(monster.MaxHp > 0);
        Assert.True(monster.AttackPower >= 0);
        Assert.False(monster.IsDead);
        Assert.False(monster.IsPassable);
        Assert.Equal(ObjectType.NPC, monster.Type);
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Respect_MaxAttempts_And_Not_Hang()
    {
        // Arrange
        const int count = 100; // Unrealistically high count
        const int width = 10;  // Small area
        const int height = 10;
        const int safeSpawnRadius = 0;
        var seed = 44444;

        // Mark all positions as blocked
        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(true);

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(count, width, height, safeSpawnRadius, seed);

        // Assert - Should return empty list, not hang
        Assert.Empty(spawnedMonsters);
    }

    [Fact]
    public void Multiple_SpawnCalls_Should_Not_Overwrite_Existing_Monsters()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed1 = 11111;
        var seed2 = 22222;
        var idCounter = 8000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act
        var firstBatch = _monsterManager.SpawnRandomMonsters(2, width, height, safeSpawnRadius, seed1);
        var secondBatch = _monsterManager.SpawnRandomMonsters(2, width, height, safeSpawnRadius, seed2);
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.Equal(4, allMonsters.Count);
        Assert.All(firstBatch, monster => Assert.Contains(monster, allMonsters));
        Assert.All(secondBatch, monster => Assert.Contains(monster, allMonsters));
    }

    [Fact]
    public void Monster_Should_Be_Properly_Initialized_With_Template_Values()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 33333;
        var monsterId = 9000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var monster = spawnedMonsters.First();

        // Assert - Check that monster uses one of the templates
        var templateNames = new[] { "Rat", "Wolf", "Orc", "Spider" };
        var templateObjectCodes = new[] { "rat", "wolf", "orc", "spider" };

        Assert.Contains(monster.Name, templateNames);
        Assert.Contains(monster.ObjectCode, templateObjectCodes);

        // Check HP and attack ranges from templates
        Assert.True(monster.Hp >= 30 && monster.Hp <= 90);
        Assert.True(monster.AttackPower >= 4 && monster.AttackPower <= 14);
    }

    [Fact]
    public void SpawnRandomMonsters_Should_Throw_When_Duplicate_ID_Generated()
    {
        // Arrange
        const int width = 50;
        const int height = 50;
        const int safeSpawnRadius = 5;
        var seed = 10101;
        var duplicateId = 10000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() =>
            {
                // Return duplicate ID on second call to simulate ID generator failure
                return duplicateId;
            });

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _monsterManager.SpawnRandomMonsters(2, width, height, safeSpawnRadius, seed));

        Assert.Contains("Duplicate monster ID generated", exception.Message);
        Assert.Contains(duplicateId.ToString(), exception.Message);
    }

    [Fact]
    public void MonsterManager_Should_Detect_Duplicate_IDs_From_Faulty_Generator()
    {
        // Arrange - Simulate a faulty ID generator that returns the same ID twice
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 20202;
        var duplicateId = 20000L;
        var idsReturned = new List<long>();

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() =>
            {
                if (idsReturned.Count < 2)
                {
                    // First two calls return the same ID
                    idsReturned.Add(duplicateId);
                    return duplicateId;
                }
                // Subsequent calls return unique IDs
                return duplicateId + idsReturned.Count;
            });

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act & Assert - Should throw on second monster spawn attempt
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _monsterManager.SpawnRandomMonsters(3, width, height, safeSpawnRadius, seed));

        Assert.Contains("Duplicate monster ID generated", exception.Message);
        Assert.Contains(duplicateId.ToString(), exception.Message);
    }
}

using Moq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.World;

namespace GameServer.Tests.Managers;

public class MonsterManagerSyncTests
{
    private readonly Mock<ICollisionManager> _collisionManagerMock;
    private readonly Mock<IIdGeneratorService> _idGeneratorServiceMock;
    private readonly MonsterManager _monsterManager;

    public MonsterManagerSyncTests()
    {
        _collisionManagerMock = new Mock<ICollisionManager>();
        _idGeneratorServiceMock = new Mock<IIdGeneratorService>();
        _monsterManager = new MonsterManager(_collisionManagerMock.Object, _idGeneratorServiceMock.Object);
    }

    [Fact]
    public void GetAllMonsters_Should_Return_Empty_Collection_When_No_Monsters_Spawned()
    {
        // Act
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.NotNull(allMonsters);
        Assert.Empty(allMonsters);
    }

    [Fact]
    public void GetAllMonsters_Should_Return_Correct_Monster_Count_After_Spawn()
    {
        // Arrange
        const int spawnCount = 7;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 12345;
        var idCounter = 1000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn some monsters
        _ = _monsterManager.SpawnRandomMonsters(spawnCount, width, height, safeSpawnRadius, seed);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.Equal(spawnCount, allMonsters.Count);
    }

    [Fact]
    public void GetAllMonsters_Should_Not_Return_Removed_Monsters()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 54321;
        var ids = new List<long> { 2000, 2001, 2002, 2003 };
        var currentIdIndex = 0;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => ids[currentIdIndex++]);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters
        _ = _monsterManager.SpawnRandomMonsters(4, width, height, safeSpawnRadius, seed);

        // Remove one monster
        _monsterManager.RemoveMonster(2001);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.Equal(3, allMonsters.Count);
        Assert.All(allMonsters, monster => Assert.NotEqual(2001L, monster.Id));
    }

    [Fact]
    public void GetMonsterById_Should_Return_All_Spawned_Monsters()
    {
        // Arrange
        const int spawnCount = 5;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 98765;
        var startId = 3000L;
        var currentId = startId;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => currentId++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters
        _ = _monsterManager.SpawnRandomMonsters(spawnCount, width, height, safeSpawnRadius, seed);

        // Act & Assert - Verify each monster can be retrieved by ID
        for (long i = startId; i < startId + spawnCount; i++)
        {
            var monster = _monsterManager.GetMonsterById(i);
            Assert.NotNull(monster);
            Assert.Equal(i, monster.Id);
        }
    }

    [Fact]
    public void Monster_Properties_Should_Match_Spawned_Data()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 11111;
        var monsterId = 4000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var spawnedMonster = spawnedMonsters.First();

        // Act - Get the same monster by ID
        var retrievedMonster = _monsterManager.GetMonsterById(monsterId);

        // Assert
        Assert.NotNull(retrievedMonster);
        Assert.Equal(spawnedMonster.Id, retrievedMonster.Id);
        Assert.Equal(spawnedMonster.Name, retrievedMonster.Name);
        Assert.Equal(spawnedMonster.ObjectCode, retrievedMonster.ObjectCode);
        Assert.Equal(spawnedMonster.Position.X, retrievedMonster.Position.X);
        Assert.Equal(spawnedMonster.Position.Y, retrievedMonster.Position.Y);
        Assert.Equal(spawnedMonster.Hp, retrievedMonster.Hp);
        Assert.Equal(spawnedMonster.MaxHp, retrievedMonster.MaxHp);
        Assert.Equal(spawnedMonster.AttackPower, retrievedMonster.AttackPower);
        Assert.Equal(spawnedMonster.IsDead, retrievedMonster.IsDead);
    }

    [Fact]
    public void Concurrent_Spawns_Should_Not_Cause_Monsters_To_Be_Missed()
    {
        // Arrange
        const int batch1Count = 3;
        const int batch2Count = 4;
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed1 = 22222;
        var seed2 = 33333;
        var idCounter = 5000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Act - Spawn monsters in two batches
        var batch1 = _monsterManager.SpawnRandomMonsters(batch1Count, width, height, safeSpawnRadius, seed1);
        var batch2 = _monsterManager.SpawnRandomMonsters(batch2Count, width, height, safeSpawnRadius, seed2);
        
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert
        Assert.Equal(batch1Count + batch2Count, allMonsters.Count);
        
        // Verify all monsters from batch1 are present
        foreach (var monster in batch1)
        {
            Assert.Contains(monster, allMonsters);
        }
        
        // Verify all monsters from batch2 are present
        foreach (var monster in batch2)
        {
            Assert.Contains(monster, allMonsters);
        }
    }

    [Fact]
    public void Monster_Data_Should_Be_Consistent_Across_Retrieval_Methods()
    {
        // Arrange
        const int spawnCount = 3;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 44444;
        var ids = new List<long> { 6000, 6001, 6002 };
        var currentIdIndex = 0;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => ids[currentIdIndex++]);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(spawnCount, width, height, safeSpawnRadius, seed);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters().ToList();

        // Assert - Data should be consistent
        Assert.Equal(spawnCount, allMonsters.Count);
        
        foreach (var spawnedMonster in spawnedMonsters)
        {
            var fromAll = allMonsters.First(m => m.Id == spawnedMonster.Id);
            var byId = _monsterManager.GetMonsterById(spawnedMonster.Id);
            
            // All three ways of getting the monster should return consistent data
            Assert.Equal(spawnedMonster.Name, fromAll.Name);
            Assert.Equal(spawnedMonster.Name, byId?.Name);
            
            Assert.Equal(spawnedMonster.ObjectCode, fromAll.ObjectCode);
            Assert.Equal(spawnedMonster.ObjectCode, byId?.ObjectCode);
            
            Assert.Equal(spawnedMonster.Position.X, fromAll.Position.X);
            Assert.Equal(spawnedMonster.Position.X, byId?.Position.X);
            
            Assert.Equal(spawnedMonster.Position.Y, fromAll.Position.Y);
            Assert.Equal(spawnedMonster.Position.Y, byId?.Position.Y);
            
            Assert.Equal(spawnedMonster.Hp, fromAll.Hp);
            Assert.Equal(spawnedMonster.Hp, byId?.Hp);
        }
    }

    [Fact]
    public void Dead_Monsters_Should_Still_Be_Returned_In_GetAllMonsters()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 55555;
        var monsterId = 7000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var monster = spawnedMonsters.First();

        // Simulate monster taking damage and dying
        // Note: We need to get the actual monster instance to call TakeDamage
        var monsterInstance = _monsterManager.GetMonsterById(monsterId);
        Assert.NotNull(monsterInstance);
        
        // Kill the monster
        monsterInstance.TakeDamage(monsterInstance.MaxHp + 10);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters();
        var deadMonster = _monsterManager.GetMonsterById(monsterId);

        // Assert
        Assert.Single(allMonsters);
        Assert.True(deadMonster?.IsDead);
        Assert.Contains(deadMonster!, allMonsters);
    }

    [Fact]
    public void GetAllMonsters_Should_Return_ReadOnly_Collection()
    {
        // Arrange
        const int spawnCount = 2;
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 66666;
        var idCounter = 8000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn some monsters
        _ = _monsterManager.SpawnRandomMonsters(spawnCount, width, height, safeSpawnRadius, seed);

        // Act
        var allMonsters = _monsterManager.GetAllMonsters();

        // Assert - Should be read-only
        Assert.IsAssignableFrom<IReadOnlyCollection<IMonster>>(allMonsters);
    }
}
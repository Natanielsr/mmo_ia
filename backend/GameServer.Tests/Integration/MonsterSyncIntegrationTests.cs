using Moq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.World;
using GameServerApp.Dtos;

namespace GameServer.Tests.Integration;

public class MonsterSyncIntegrationTests
{
    private readonly Mock<ICollisionManager> _collisionManagerMock;
    private readonly Mock<IIdGeneratorService> _idGeneratorServiceMock;
    private readonly MonsterManager _monsterManager;

    public MonsterSyncIntegrationTests()
    {
        _collisionManagerMock = new Mock<ICollisionManager>();
        _idGeneratorServiceMock = new Mock<IIdGeneratorService>();
        _monsterManager = new MonsterManager(_collisionManagerMock.Object, _idGeneratorServiceMock.Object);
    }

    [Fact]
    public void MonsterData_Conversion_Should_Match_Original_Monster_Properties()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 12345;
        var monsterId = 1000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);
        var originalMonster = spawnedMonsters.First();

        // Act - Simulate what GameHub does to convert monsters to DTOs
        var allMonsters = _monsterManager.GetAllMonsters();
        var monsterDataList = allMonsters.Select(monster => new MonsterData
        {
            Id = monster.Id.ToString(),
            Name = monster.Name,
            ObjectCode = monster.ObjectCode,
            Position = monster.Position,
            Hp = monster.Hp,
            MaxHp = monster.MaxHp,
            AttackPower = monster.AttackPower,
            IsDead = monster.IsDead
        }).ToList();

        // Assert
        Assert.Single(monsterDataList);
        var monsterData = monsterDataList.First();

        Assert.Equal(originalMonster.Id.ToString(), monsterData.Id);
        Assert.Equal(originalMonster.Name, monsterData.Name);
        Assert.Equal(originalMonster.ObjectCode, monsterData.ObjectCode);
        Assert.Equal(originalMonster.Position.X, monsterData.Position.X);
        Assert.Equal(originalMonster.Position.Y, monsterData.Position.Y);
        Assert.Equal(originalMonster.Hp, monsterData.Hp);
        Assert.Equal(originalMonster.MaxHp, monsterData.MaxHp);
        Assert.Equal(originalMonster.AttackPower, monsterData.AttackPower);
        Assert.Equal(originalMonster.IsDead, monsterData.IsDead);
    }

    [Fact]
    public void GetAllMonsters_Should_Include_All_Monsters_For_Sync()
    {
        // Arrange - Create a scenario where we spawn monsters at different times
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed1 = 11111;
        var seed2 = 22222;
        var seed3 = 33333;
        var idCounter = 2000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters in three separate batches (simulating different times)
        var batch1 = _monsterManager.SpawnRandomMonsters(3, width, height, safeSpawnRadius, seed1);
        var batch2 = _monsterManager.SpawnRandomMonsters(2, width, height, safeSpawnRadius, seed2);

        // Remove one monster from batch1
        var monsterToRemove = batch1.First();
        _monsterManager.RemoveMonster(monsterToRemove.Id);

        // Spawn more monsters
        var batch3 = _monsterManager.SpawnRandomMonsters(4, width, height, safeSpawnRadius, seed3);

        // Act - Simulate GameHub getting all monsters for sync
        var allMonstersForSync = _monsterManager.GetAllMonsters();
        var monsterDataForSync = allMonstersForSync.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert
        // Should have: batch1(3) + batch2(2) - removed(1) + batch3(4) = 8 monsters
        Assert.Equal(8, allMonstersForSync.Count);
        Assert.Equal(8, monsterDataForSync.Count);

        // Verify the removed monster is not included
        Assert.DoesNotContain(allMonstersForSync, m => m.Id == monsterToRemove.Id);
        Assert.DoesNotContain(monsterDataForSync, m => m.Id == monsterToRemove.Id.ToString());

        // Verify all other monsters are included
        foreach (var monster in batch1.Where(m => m.Id != monsterToRemove.Id))
        {
            Assert.Contains(allMonstersForSync, m => m.Id == monster.Id);
            Assert.Contains(monsterDataForSync, m => m.Id == monster.Id.ToString());
        }

        foreach (var monster in batch2)
        {
            Assert.Contains(allMonstersForSync, m => m.Id == monster.Id);
            Assert.Contains(monsterDataForSync, m => m.Id == monster.Id.ToString());
        }

        foreach (var monster in batch3)
        {
            Assert.Contains(allMonstersForSync, m => m.Id == monster.Id);
            Assert.Contains(monsterDataForSync, m => m.Id == monster.Id.ToString());
        }
    }

    [Fact]
    public void Monster_Position_Changes_Should_Be_Reflected_In_Sync()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 44444;
        var monsterId = 3000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        _ = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);

        // Get the monster and move it
        var monster = _monsterManager.GetMonsterById(monsterId);
        Assert.NotNull(monster);

        var newPosition = new Position(monster.Position.X + 5, monster.Position.Y + 3);
        monster.MoveTo(newPosition);

        // Act - Get monsters for sync after movement
        var allMonsters = _monsterManager.GetAllMonsters();
        var monsterDataList = allMonsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert
        Assert.Single(monsterDataList);
        var monsterData = monsterDataList.First();
        Assert.Equal(newPosition.X, monsterData.Position.X);
        Assert.Equal(newPosition.Y, monsterData.Position.Y);
    }

    [Fact]
    public void Monster_HP_Changes_Should_Be_Reflected_In_Sync()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 55555;
        var monsterId = 4000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        _ = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);

        // Get the monster and damage it
        var monster = _monsterManager.GetMonsterById(monsterId);
        Assert.NotNull(monster);

        var originalHp = monster.Hp;
        var damage = 10;
        monster.TakeDamage(damage);

        // Act - Get monsters for sync after damage
        var allMonsters = _monsterManager.GetAllMonsters();
        var monsterDataList = allMonsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert
        Assert.Single(monsterDataList);
        var monsterData = monsterDataList.First();
        Assert.Equal(originalHp - damage, monsterData.Hp);
        Assert.False(monsterData.IsDead); // Should not be dead from 10 damage
    }

    [Fact]
    public void Dead_Monsters_Should_Be_Included_In_Sync_Until_Removed()
    {
        // Arrange
        const int width = 100;
        const int height = 100;
        const int safeSpawnRadius = 10;
        var seed = 66666;
        var monsterId = 5000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(monsterId);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn a monster
        _ = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed);

        // Get the monster and kill it
        var monster = _monsterManager.GetMonsterById(monsterId);
        Assert.NotNull(monster);

        monster.TakeDamage(monster.MaxHp + 10); // Ensure it dies

        // Act - Get monsters for sync after death
        var allMonsters = _monsterManager.GetAllMonsters();
        var monsterDataList = allMonsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert - Dead monster should still be included
        Assert.Single(monsterDataList);
        var monsterData = monsterDataList.First();
        Assert.True(monsterData.IsDead);
        Assert.Equal(0, monsterData.Hp);

        // Now remove the dead monster
        _monsterManager.RemoveMonster(monsterId);

        // Act - Get monsters for sync after removal
        var allMonstersAfterRemoval = _monsterManager.GetAllMonsters();
        var monsterDataListAfterRemoval = allMonstersAfterRemoval.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert - Dead monster should NOT be included after removal
        Assert.Empty(allMonstersAfterRemoval);
        Assert.Empty(monsterDataListAfterRemoval);
    }

    [Fact]
    public void Multiple_Clients_Should_Get_Same_Monster_Data_For_Sync()
    {
        // Arrange - Simulate multiple clients joining and requesting monster sync
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed = 77777;
        var idCounter = 6000L;
        const int monsterCount = 5;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters
        _ = _monsterManager.SpawnRandomMonsters(monsterCount, width, height, safeSpawnRadius, seed);

        // Act - Simulate 3 clients requesting monster sync
        var client1Monsters = _monsterManager.GetAllMonsters();
        var client2Monsters = _monsterManager.GetAllMonsters();
        var client3Monsters = _monsterManager.GetAllMonsters();

        // Convert to DTOs like GameHub would
        var client1Data = client1Monsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        var client2Data = client2Monsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        var client3Data = client3Monsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert - All clients should get the same monster data
        Assert.Equal(monsterCount, client1Monsters.Count);
        Assert.Equal(monsterCount, client2Monsters.Count);
        Assert.Equal(monsterCount, client3Monsters.Count);

        Assert.Equal(monsterCount, client1Data.Count);
        Assert.Equal(monsterCount, client2Data.Count);
        Assert.Equal(monsterCount, client3Data.Count);

        // All clients should have the same monster IDs
        var client1Ids = client1Data.Select(d => d.Id).OrderBy(id => id).ToList();
        var client2Ids = client2Data.Select(d => d.Id).OrderBy(id => id).ToList();
        var client3Ids = client3Data.Select(d => d.Id).OrderBy(id => id).ToList();

        Assert.Equal(client1Ids, client2Ids);
        Assert.Equal(client1Ids, client3Ids);
    }

    [Fact]
    public void Monster_Sync_Should_Work_With_Mixed_Alive_And_Dead_Monsters()
    {
        // Arrange
        const int width = 150;
        const int height = 150;
        const int safeSpawnRadius = 15;
        var seed = 88888;
        var idCounter = 7000L;
        const int totalMonsters = 6;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(totalMonsters, width, height, safeSpawnRadius, seed).ToList();

        // Kill some monsters (every other one)
        for (int i = 0; i < spawnedMonsters.Count; i += 2)
        {
            var monster = _monsterManager.GetMonsterById(spawnedMonsters[i].Id);
            Assert.NotNull(monster);
            monster.TakeDamage(monster.MaxHp + 10);
        }

        // Act - Get monsters for sync
        var allMonsters = _monsterManager.GetAllMonsters().ToList();
        var monsterDataList = allMonsters.Select(m => new MonsterData
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            ObjectCode = m.ObjectCode,
            Position = m.Position,
            Hp = m.Hp,
            MaxHp = m.MaxHp,
            AttackPower = m.AttackPower,
            IsDead = m.IsDead
        }).ToList();

        // Assert
        Assert.Equal(totalMonsters, allMonsters.Count);
        Assert.Equal(totalMonsters, monsterDataList.Count);

        // Check that we have both alive and dead monsters
        var deadMonsters = monsterDataList.Where(m => m.IsDead).ToList();
        var aliveMonsters = monsterDataList.Where(m => !m.IsDead).ToList();

        Assert.Equal(3, deadMonsters.Count);
        Assert.Equal(3, aliveMonsters.Count);

        // Verify dead monsters have 0 HP
        Assert.All(deadMonsters, m => Assert.Equal(0, m.Hp));

        // Verify alive monsters have positive HP
        Assert.All(aliveMonsters, m => Assert.True(m.Hp > 0));
    }
}
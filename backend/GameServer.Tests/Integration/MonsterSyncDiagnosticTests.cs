using Moq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Managers;
using GameServerApp.World;
using GameServerApp.Dtos;

namespace GameServer.Tests.Integration;

public class MonsterSyncDiagnosticTests
{
    private readonly Mock<ICollisionManager> _collisionManagerMock;
    private readonly Mock<IIdGeneratorService> _idGeneratorServiceMock;
    private readonly MonsterManager _monsterManager;

    public MonsterSyncDiagnosticTests()
    {
        _collisionManagerMock = new Mock<ICollisionManager>();
        _idGeneratorServiceMock = new Mock<IIdGeneratorService>();
        _monsterManager = new MonsterManager(_collisionManagerMock.Object, _idGeneratorServiceMock.Object);
    }

    [Fact]
    public void GameHub_SyncMonsters_Should_Send_All_Existing_Monsters_To_New_Player()
    {
        // Arrange - Simulate GameHub scenario
        const int monsterCount = 8;
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed = 12345;
        var idCounter = 1000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn monsters before player joins (simulating existing monsters in world)
        _ = _monsterManager.SpawnRandomMonsters(monsterCount, width, height, safeSpawnRadius, seed);

        // Act - Simulate what GameHub does when player joins
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
        Assert.Equal(monsterCount, monsterDataList.Count);

        // Verify all monsters have valid data
        foreach (var monsterData in monsterDataList)
        {
            Assert.True(long.Parse(monsterData.Id) > 0);
            Assert.False(string.IsNullOrWhiteSpace(monsterData.Name));
            Assert.False(string.IsNullOrWhiteSpace(monsterData.ObjectCode));
            Assert.True(monsterData.Hp > 0);
            Assert.True(monsterData.MaxHp > 0);
            Assert.True(monsterData.AttackPower >= 0);
            Assert.False(monsterData.IsDead);
        }
    }

    [Fact]
    public void Monster_Sync_Should_Include_Monsters_Spawned_During_Gameplay()
    {
        // Arrange - Simulate dynamic monster spawning during gameplay
        const int initialMonsters = 5;
        const int laterMonsters = 3;
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed1 = 11111;
        var seed2 = 22222;
        var idCounter = 2000L;

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Simulate initial monster spawn (when server starts)
        _ = _monsterManager.SpawnRandomMonsters(initialMonsters, width, height, safeSpawnRadius, seed1);

        // Player 1 joins and gets initial sync
        var player1Sync = _monsterManager.GetAllMonsters();
        var player1Data = player1Sync.Select(m => new MonsterData
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

        // Simulate more monsters spawning later (e.g., from respawn timer)
        _ = _monsterManager.SpawnRandomMonsters(laterMonsters, width, height, safeSpawnRadius, seed2);

        // Player 2 joins later and should get ALL monsters
        var player2Sync = _monsterManager.GetAllMonsters();
        var player2Data = player2Sync.Select(m => new MonsterData
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
        Assert.Equal(initialMonsters, player1Data.Count);
        Assert.Equal(initialMonsters + laterMonsters, player2Data.Count);

        // Player 2 should have all monsters that Player 1 has, plus the new ones
        var player1Ids = player1Data.Select(d => d.Id).ToHashSet();
        var player2Ids = player2Data.Select(d => d.Id).ToHashSet();

        Assert.Subset(player2Ids, player1Ids); // All of player1's monsters should be in player2's list
        Assert.Equal(laterMonsters, player2Ids.Count - player1Ids.Count); // Plus the new ones
    }

    [Fact]
    public void Monster_Removal_Should_Be_Reflected_In_Subsequent_Syncs()
    {
        // Arrange - Test that removed monsters don't appear in sync
        const int initialMonsters = 7;
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed = 33333;
        var idCounter = 3000L;
        var monsterIdsToRemove = new List<long>();

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Spawn initial monsters
        var spawned = _monsterManager.SpawnRandomMonsters(initialMonsters, width, height, safeSpawnRadius, seed).ToList();

        // Mark some monsters for removal (simulating them being killed and cleaned up)
        monsterIdsToRemove.Add(spawned[0].Id);
        monsterIdsToRemove.Add(spawned[2].Id);
        monsterIdsToRemove.Add(spawned[4].Id);

        // Player joins and gets initial sync
        var initialSync = _monsterManager.GetAllMonsters();
        var initialData = initialSync.Select(m => new MonsterData
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
        Assert.Equal(initialMonsters, initialData.Count);

        // Remove some monsters
        foreach (var monsterId in monsterIdsToRemove)
        {
            _monsterManager.RemoveMonster(monsterId);
        }

        // Another player joins after removals
        var laterSync = _monsterManager.GetAllMonsters();
        var laterData = laterSync.Select(m => new MonsterData
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
        Assert.Equal(initialMonsters - monsterIdsToRemove.Count, laterData.Count);

        // Verify removed monsters are not in later sync
        foreach (var removedId in monsterIdsToRemove)
        {
            Assert.DoesNotContain(laterData, m => m.Id == removedId.ToString());
        }

        // Verify remaining monsters are still there
        var remainingIds = spawned.Where(m => !monsterIdsToRemove.Contains(m.Id)).Select(m => m.Id).ToHashSet();
        foreach (var remainingId in remainingIds)
        {
            Assert.Contains(laterData, m => m.Id == remainingId.ToString());
        }
    }

    [Fact]
    public async Task Race_Condition_Test_Monster_Addition_During_Sync()
    {
        // This test simulates a potential race condition where monsters
        // are being added while GetMonsterById or GetAllMonsters is being called

        // Arrange
        const int width = 200;
        const int height = 200;
        const int safeSpawnRadius = 20;
        var seed = 44444;
        var idCounter = 4000L;
        var syncMonsterIds = new List<long>();
        object lockObject = new object();

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() =>
            {
                lock (lockObject)
                {
                    return idCounter++;
                }
            });

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        // Simulate concurrent monster spawning and syncing
        var syncTasks = new List<Task>();

        // Task 1: Continuously spawn monsters (simulating monster respawns)
        var spawnTask = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                lock (lockObject)
                {
                    _ = _monsterManager.SpawnRandomMonsters(1, width, height, safeSpawnRadius, seed + i);
                }
                await Task.Delay(10); // Small delay
            }
        });

        // Task 2: Continuously sync monsters (simulating players joining)
        var syncTask = Task.Run(async () =>
        {
            for (int i = 0; i < 20; i++)
            {
                IReadOnlyCollection<IMonster> monsters;
                lock (lockObject)
                {
                    monsters = _monsterManager.GetAllMonsters();
                }

                var monsterIds = monsters.Select(m => m.Id).ToList();
                lock (syncMonsterIds)
                {
                    syncMonsterIds.AddRange(monsterIds);
                }
                await Task.Delay(5);
            }
        });

        // Wait for both tasks
        await Task.WhenAll(spawnTask, syncTask);

        // Get final state
        var finalMonsters = _monsterManager.GetAllMonsters();
        var finalMonsterIds = finalMonsters.Select(m => m.Id).ToList();
        var uniqueSyncIds = syncMonsterIds.Distinct().ToList();

        // Assert - All monsters that were ever synced should exist in final state
        // (unless they were removed, which we're not doing in this test)
        foreach (var syncedId in uniqueSyncIds)
        {
            var monster = _monsterManager.GetMonsterById(syncedId);
            Assert.NotNull(monster); // If this fails, there's a race condition
            Assert.Contains(syncedId, finalMonsterIds);
        }
    }

    [Fact]
    public void Diagnostic_Test_For_Missing_Monsters_In_Sync()
    {
        // This test is designed to diagnose the exact issue mentioned:
        // "tem monstros que nao estao sendo enviados no SyncMonster"

        // Arrange
        const int expectedMonsterCount = 10;
        const int width = 300;
        const int height = 300;
        const int safeSpawnRadius = 30;
        var seed = 55555;
        var idCounter = 5000L;
        var spawnedIds = new List<long>();

        _idGeneratorServiceMock.Setup(s => s.GenerateId())
            .Returns(() => idCounter++);

        _collisionManagerMock.Setup(c => c.IsPositionBlocked(It.IsAny<Position>()))
            .Returns(false);

        Console.WriteLine($"Starting diagnostic test for monster sync issue");
        Console.WriteLine($"Attempting to spawn {expectedMonsterCount} monsters...");

        // Spawn monsters
        var spawnedMonsters = _monsterManager.SpawnRandomMonsters(
            expectedMonsterCount, width, height, safeSpawnRadius, seed);

        spawnedIds.AddRange(spawnedMonsters.Select(m => m.Id));

        Console.WriteLine($"Successfully spawned {spawnedMonsters.Count} monsters");
        Console.WriteLine($"Spawned monster IDs: {string.Join(", ", spawnedIds)}");

        // Act - Simulate GameHub sync
        var allMonsters = _monsterManager.GetAllMonsters();
        var syncedIds = allMonsters.Select(m => m.Id).ToList();

        Console.WriteLine($"Monsters returned by GetAllMonsters(): {allMonsters.Count}");
        Console.WriteLine($"Synced monster IDs: {string.Join(", ", syncedIds)}");

        // Try getting each monster by ID individually
        Console.WriteLine("\nAttempting to get each monster by ID:");
        foreach (var spawnedId in spawnedIds)
        {
            var monster = _monsterManager.GetMonsterById(spawnedId);
            if (monster == null)
            {
                Console.WriteLine($"  ERROR: Monster ID {spawnedId} not found by GetMonsterById()!");
            }
            else
            {
                Console.WriteLine($"  OK: Monster ID {spawnedId} found - Name: {monster.Name}, Position: ({monster.Position.X}, {monster.Position.Y})");
            }
        }

        // Check for missing monsters
        var missingInSync = spawnedIds.Except(syncedIds).ToList();
        var extraInSync = syncedIds.Except(spawnedIds).ToList();

        Console.WriteLine($"\nDiagnostic Results:");
        Console.WriteLine($"  Expected monster count: {expectedMonsterCount}");
        Console.WriteLine($"  Actually spawned: {spawnedMonsters.Count}");
        Console.WriteLine($"  Returned by GetAllMonsters(): {allMonsters.Count}");

        if (missingInSync.Any())
        {
            Console.WriteLine($"  ERROR: {missingInSync.Count} monsters missing from sync:");
            foreach (var missingId in missingInSync)
            {
                Console.WriteLine($"    - Monster ID {missingId}");
            }
        }
        else
        {
            Console.WriteLine($"  OK: No monsters missing from sync");
        }

        if (extraInSync.Any())
        {
            Console.WriteLine($"  WARNING: {extraInSync.Count} extra monsters in sync (ghost monsters):");
            foreach (var extraId in extraInSync)
            {
                Console.WriteLine($"    - Monster ID {extraId}");
            }
        }
        else
        {
            Console.WriteLine($"  OK: No extra/ghost monsters in sync");
        }

        // Assert - These should all pass if there's no bug
        Assert.Equal(expectedMonsterCount, spawnedMonsters.Count);
        Assert.Equal(expectedMonsterCount, allMonsters.Count);
        Assert.Empty(missingInSync);
        Assert.Empty(extraInSync);

        // Additional check: all spawned monsters should be retrievable by ID
        foreach (var spawnedId in spawnedIds)
        {
            var monster = _monsterManager.GetMonsterById(spawnedId);
            Assert.NotNull(monster);
            Assert.Equal(spawnedId, monster.Id);
        }
    }
}
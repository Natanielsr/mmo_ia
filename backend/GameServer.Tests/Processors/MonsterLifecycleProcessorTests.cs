using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Processors;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using Moq;

namespace GameServer.Tests.Processors;

public class MonsterLifecycleProcessorTests
{
    private readonly Mock<IMonsterManager> _monsterManagerMock = new();
    private readonly Mock<IPlayerManager> _playerManagerMock = new();
    private readonly Mock<IMonsterMovementService> _movementServiceMock = new();
    private readonly Mock<IWorldEvents> _worldEventsMock = new();
    private readonly Mock<IBiomeSelector> _biomeSelectorMock = new();
    private readonly MonsterLifecycleProcessor _sut;

    private static readonly BiomeDefinition GreenField = new()
    {
        Id = 1, TagName = "green_field", Name = "Green Field",
        GroundTilePrefix = "grass", GroundTileCount = 12,
        Objects = [],
        MonsterTags = ["rat", "wolf"],
        IsDefault = true
    };

    private static readonly BiomeDefinition DarkForest = new()
    {
        Id = 2, TagName = "dark_forest", Name = "Dark Forest",
        GroundTilePrefix = "dark_grass", GroundTileCount = 12,
        Objects = [],
        MonsterTags = ["spider", "orc"]
    };

    public MonsterLifecycleProcessorTests()
    {
        var config = Options.Create(new WorldConfig
        {
            Map = new MapConfig { SafeSpawnRadius = 4 },
            Monsters = new MonsterConfig
            {
                PerPlayer = 5,
                MinSpawnDistance = 10,
                SpawnRadius = 20,
                DespawnRadius = 40,
                RespawnTimeSec = 2.0
            }
        });

        _sut = new MonsterLifecycleProcessor(
            _monsterManagerMock.Object,
            _playerManagerMock.Object,
            _movementServiceMock.Object,
            _worldEventsMock.Object,
            _biomeSelectorMock.Object,
            config);
    }

    [Fact]
    public void ProcessMonsterRespawn_PassesBiomePositionValidator_RejectingWrongBiomeTiles()
    {
        // Arrange
        var player = new Player(1, "Tester", new Position(0, 50));
        _playerManagerMock.Setup(p => p.GetAllPlayers()).Returns(new IPlayer[] { player });
        _monsterManagerMock.Setup(m => m.GetAllMonsters()).Returns(Array.Empty<IMonster>());

        // Player is in green_field
        _biomeSelectorMock
            .Setup(b => b.GetBiomeForPosition(player.Position))
            .Returns(GreenField);

        // All candidate tiles return dark_forest (simulating transition zone)
        _biomeSelectorMock
            .Setup(b => b.GetBiomeForTile(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(DarkForest);

        Func<Position, bool>? capturedValidator = null;
        _monsterManagerMock
            .Setup(m => m.SpawnMonstersNearPosition(
                It.IsAny<int>(), It.IsAny<Position>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<int?>(), It.IsAny<IReadOnlyList<string>?>(), It.IsAny<Func<Position, bool>?>()))
            .Callback<int, Position, int, int, int?, IReadOnlyList<string>?, Func<Position, bool>?>(
                (_, _, _, _, _, _, validator) => capturedValidator = validator)
            .Returns(Array.Empty<IMonster>());

        // Act
        _sut.ProcessMonsterRespawn();

        // Assert — validator captured and correctly rejects dark_forest positions
        Assert.NotNull(capturedValidator);
        Assert.False(capturedValidator(new Position(100, 100))); // dark_forest tile → rejected
    }
}

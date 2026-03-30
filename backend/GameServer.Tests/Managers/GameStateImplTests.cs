using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Managers;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using GameServerApp.Contracts.Config;

namespace GameServer.Tests.Managers;

public class GameStateImplTests
{
    private readonly StaticWorldManager _staticWorldManager = new(Options.Create(new WorldConfig()));
    private readonly CollisionManager _collisionManager;
    private readonly GameStateManager _manager;

    public GameStateImplTests()
    {
        _collisionManager = new(_staticWorldManager);
        _manager = new GameStateManager(_collisionManager);
    }
    private readonly Position _spawnPoint = new Position(10, 10);
    private readonly Position _hospitalPoint = new Position(50, 50);

    private Player CreatePlayer(string name = "Hero")
        => new Player(1, name, new Position(0, 0));

    [Fact]
    public void GameStateManager_Should_Spawn_Player_In_World()
    {
        var player = CreatePlayer();
        _manager.SpawnPlayer(player, _spawnPoint);

        Assert.Equal(_spawnPoint, player.Position);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Player_Death()
    {
        var player = CreatePlayer();
        _manager.PlayerDied(player);

        Assert.Equal(PlayerState.Dead, player.State);
        Assert.Equal(0, player.Hp);
    }

    [Fact]
    public void GameStateManager_Should_Send_Dead_Player_To_Hospital()
    {
        var player = CreatePlayer();
        player.Die();

        _manager.SendPlayerToHospital(player);

        Assert.Equal(PlayerState.Alive, player.State);
        Assert.Equal(_hospitalPoint, player.Position);
        Assert.Equal(player.MaxHp, player.Hp);
    }

    [Fact]
    public void GameStateManager_Should_Return_Hospital_Spawn_Point()
    {
        var hospitalPoint = _manager.GetHospitalSpawnPoint();
        Assert.Equal(_hospitalPoint, hospitalPoint);
    }

    [Fact]
    public void GameStateManager_Should_Award_Experience()
    {
        var player = CreatePlayer();
        _manager.AddPlayerExperience(player, 500);

        Assert.Equal(500, player.Experience);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Level_Up_Flow()
    {
        var player = CreatePlayer();
        _manager.AddPlayerExperience(player, 1000);
        _manager.CheckForLevelUp(player);

        Assert.Equal(2, player.Level);
    }
}

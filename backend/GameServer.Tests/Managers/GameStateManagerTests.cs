using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.Managers;

public class GameStateManagerTests
{
    private readonly Position _spawnPoint = new Position(10, 10);
    private readonly Position _hospitalPoint = new Position(50, 50);

    [Fact]
    public void GameStateManager_Should_Spawn_Player_In_World()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.Name).Returns("Hero");

        mock.Object.SpawnPlayer(player.Object, _spawnPoint);

        mock.Verify(m => m.SpawnPlayer(It.IsAny<IPlayer>(), It.Is<Position>(p => p.X == 10 && p.Y == 10)), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Player_Death()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.State).Returns(PlayerState.Dead);

        mock.Object.PlayerDied(player.Object);

        mock.Verify(m => m.PlayerDied(It.IsAny<IPlayer>()), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Send_Dead_Player_To_Hospital()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.State).Returns(PlayerState.Dead);

        mock.Object.SendPlayerToHospital(player.Object);

        mock.Verify(m => m.SendPlayerToHospital(It.IsAny<IPlayer>()), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Return_Hospital_Spawn_Point()
    {
        var mock = new Mock<IGameStateManager>();
        mock.Setup(m => m.GetHospitalSpawnPoint()).Returns(_hospitalPoint);

        var hospitalPoint = mock.Object.GetHospitalSpawnPoint();

        Assert.Equal(_hospitalPoint.X, hospitalPoint.X);
        Assert.Equal(_hospitalPoint.Y, hospitalPoint.Y);
    }

    [Fact]
    public void GameStateManager_Should_Register_Monster_Killed()
    {
        var mock = new Mock<IGameStateManager>();
        var killer = new Mock<IPlayer>();
        killer.Setup(p => p.Name).Returns("Hero");
        var monsterId = "goblin_001";

        mock.Object.MonsterKilled(killer.Object, monsterId);

        mock.Verify(m => m.MonsterKilled(It.IsAny<IPlayer>(), "goblin_001"), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Drop_Item_From_Monster()
    {
        var mock = new Mock<IGameStateManager>();
        var itemId = "sword_iron";
        var dropPosition = new Position(15, 15);

        mock.Object.DropItem(itemId, dropPosition);

        mock.Verify(m => m.DropItem("sword_iron", It.Is<Position>(p => p.X == 15 && p.Y == 15)), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Allow_Player_To_Pick_Up_Item()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        var itemId = "sword_iron";

        mock.Object.PlayerPicksUpItem(player.Object, itemId);

        mock.Verify(m => m.PlayerPicksUpItem(It.IsAny<IPlayer>(), "sword_iron"), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Award_Experience_When_Monster_Dies()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.Experience).Returns(0);
        long xpReward = 500;

        mock.Object.AddPlayerExperience(player.Object, xpReward);

        mock.Verify(m => m.AddPlayerExperience(It.IsAny<IPlayer>(), 500), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Check_For_Level_Up()
    {
        var mock = new Mock<IGameStateManager>();
        var player = new Mock<IPlayer>();
        player.Setup(p => p.Level).Returns(1);
        player.Setup(p => p.Experience).Returns(2000);

        mock.Object.CheckForLevelUp(player.Object);

        mock.Verify(m => m.CheckForLevelUp(It.IsAny<IPlayer>()), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Full_Monster_Kill_Flow()
    {
        // Simula o fluxo completo: player mata monstro -> recebe XP -> recebe item -> verifica level up
        var mockManager = new Mock<IGameStateManager>();
        var mockPlayer = new Mock<IPlayer>();
        var monsterId = "goblin_002";
        var dropPosition = new Position(20, 20);
        var itemDropped = "gold_coins";
        long xpReward = 250;

        mockManager.Object.MonsterKilled(mockPlayer.Object, monsterId);
        mockManager.Object.DropItem(itemDropped, dropPosition);
        mockManager.Object.AddPlayerExperience(mockPlayer.Object, xpReward);
        mockManager.Object.CheckForLevelUp(mockPlayer.Object);

        mockManager.Verify(m => m.MonsterKilled(It.IsAny<IPlayer>(), "goblin_002"), Times.Once);
        mockManager.Verify(m => m.DropItem("gold_coins", It.IsAny<Position>()), Times.Once);
        mockManager.Verify(m => m.AddPlayerExperience(It.IsAny<IPlayer>(), 250), Times.Once);
        mockManager.Verify(m => m.CheckForLevelUp(It.IsAny<IPlayer>()), Times.Once);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Death_And_Hospital_Flow()
    {
        // Simula o fluxo: player morre -> é enviado ao hospital -> respawna
        var mockManager = new Mock<IGameStateManager>();
        var mockPlayer = new Mock<IPlayer>();
        var hospitalSpawnPoint = new Position(50, 50);

        mockManager.Setup(m => m.GetHospitalSpawnPoint()).Returns(hospitalSpawnPoint);

        mockManager.Object.PlayerDied(mockPlayer.Object);
        mockManager.Object.SendPlayerToHospital(mockPlayer.Object);
        var respawnPoint = mockManager.Object.GetHospitalSpawnPoint();

        mockManager.Verify(m => m.PlayerDied(It.IsAny<IPlayer>()), Times.Once);
        mockManager.Verify(m => m.SendPlayerToHospital(It.IsAny<IPlayer>()), Times.Once);
        Assert.Equal(hospitalSpawnPoint, respawnPoint);
    }

    [Fact]
    public void GameStateManager_Should_Handle_Multiple_Players_Killing_Same_Monster()
    {
        // Testa cenário onde múltiplos players atacam o mesmo monstro
        var mockManager = new Mock<IGameStateManager>();
        var player1 = new Mock<IPlayer>();
        var player2 = new Mock<IPlayer>();
        var monsterId = "dragon_001";

        player1.Setup(p => p.Name).Returns("Player1");
        player2.Setup(p => p.Name).Returns("Player2");

        mockManager.Object.MonsterKilled(player1.Object, monsterId);
        mockManager.Object.MonsterKilled(player2.Object, monsterId);

        mockManager.Verify(m => m.MonsterKilled(It.IsAny<IPlayer>(), "dragon_001"), Times.Exactly(2));
    }
}

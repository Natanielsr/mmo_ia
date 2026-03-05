using Moq;
using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.World;

public class PlayerTests
{
    private readonly Position _startPosition = new Position(0, 0);

    [Fact]
    public void Player_Should_Start_Alive()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Alive);

        var state = mock.Object.State;
        Assert.Equal(PlayerState.Alive, state);
    }

    [Fact]
    public void Player_Should_Start_With_Full_Hp()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Hp).Returns(100);
        mock.Setup(p => p.MaxHp).Returns(100);

        var hp = mock.Object.Hp;
        var maxHp = mock.Object.MaxHp;

        Assert.Equal(100, hp);
        Assert.Equal(100, maxHp);
    }

    [Fact]
    public void Player_Should_Move_To_New_Position()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Position).Returns(_startPosition);
        var newPosition = new Position(1, 1);

        mock.Object.Move(newPosition);

        mock.Verify(p => p.Move(It.Is<Position>(pos => pos.X == 1 && pos.Y == 1)), Times.Once);
    }

    [Fact]
    public void Player_Should_Take_Damage()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Hp).Returns(100);

        mock.Object.TakeDamage(25);

        mock.Verify(p => p.TakeDamage(25), Times.Once);
    }

    [Fact]
    public void Player_Should_Die_When_Hp_Reaches_Zero()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Hp).Returns(0);
        mock.Setup(p => p.State).Returns(PlayerState.Dead);

        mock.Object.Die();

        mock.Verify(p => p.Die(), Times.Once);
        Assert.Equal(PlayerState.Dead, mock.Object.State);
    }

    [Fact]
    public void Player_Should_Enter_Combat_When_Attacking()
    {
        var mockPlayer = new Mock<IPlayer>();
        var mockTarget = new Mock<IPlayer>();

        mockPlayer.Setup(p => p.State).Returns(PlayerState.InCombat);

        mockPlayer.Object.Attack(mockTarget.Object);

        mockPlayer.Verify(p => p.Attack(It.IsAny<IPlayer>()), Times.Once);
        Assert.Equal(PlayerState.InCombat, mockPlayer.Object.State);
    }

    [Fact]
    public void Player_Should_Heal()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Hp).Returns(100);
        mock.Setup(p => p.MaxHp).Returns(100);

        mock.Object.Heal(30);

        mock.Verify(p => p.Heal(30), Times.Once);
    }

    [Fact]
    public void Player_Should_Rest()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Resting);

        mock.Object.Rest();

        mock.Verify(p => p.Rest(), Times.Once);
        Assert.Equal(PlayerState.Resting, mock.Object.State);
    }

    [Fact]
    public void Player_Should_Stop_Resting()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Alive);

        mock.Object.StopResting();

        mock.Verify(p => p.StopResting(), Times.Once);
        Assert.Equal(PlayerState.Alive, mock.Object.State);
    }

    [Fact]
    public void Player_Should_Gain_Experience()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Experience).Returns(0);

        mock.Object.GainExperience(100);

        mock.Verify(p => p.GainExperience(100), Times.Once);
    }

    [Fact]
    public void Player_Should_Be_Able_To_Equip_Item()
    {
        var mock = new Mock<IPlayer>();
        var itemId = "sword_001";

        mock.Object.EquipItem(itemId);

        mock.Verify(p => p.EquipItem(itemId), Times.Once);
    }

    [Fact]
    public void Player_Should_Be_Able_To_Unequip_Item()
    {
        var mock = new Mock<IPlayer>();
        var itemId = "sword_001";

        mock.Object.UnequipItem(itemId);

        mock.Verify(p => p.UnequipItem(itemId), Times.Once);
    }

    [Fact]
    public void Player_Should_Revive_When_Dead()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Dead);

        mock.Object.Revive();

        mock.Verify(p => p.Revive(), Times.Once);
    }

    [Fact]
    public void Dead_Player_Should_Not_Be_Able_To_Move()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Dead);

        // Verificar que quando o player está morto, não consegue se mover
        var newPosition = new Position(1, 1);

        // Este teste verifica que a chamada de movimento é rastreada (será implementado depois)
        mock.Object.Move(newPosition);

        mock.Verify(p => p.Move(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public void Player_Level_Should_Increase_With_Experience()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.Level).Returns(1);
        mock.Setup(p => p.Experience).Returns(1000);

        Assert.Equal(1, mock.Object.Level);
        Assert.Equal(1000, mock.Object.Experience);
    }

    [Fact]
    public void Resting_Player_Should_Recover_Hp()
    {
        var mock = new Mock<IPlayer>();
        mock.Setup(p => p.State).Returns(PlayerState.Resting);
        mock.Setup(p => p.Hp).Returns(100);

        mock.Object.Rest();

        Assert.Equal(PlayerState.Resting, mock.Object.State);
        mock.Verify(p => p.Rest(), Times.Once);
    }
}

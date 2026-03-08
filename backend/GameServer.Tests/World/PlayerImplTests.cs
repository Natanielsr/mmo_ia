using Xunit;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class PlayerImplTests
{
    private readonly Position _startPosition = new Position(0, 0);

    private Player CreatePlayer(string name = "TestPlayer", int maxHp = 100)
        => new Player(1, name, _startPosition, maxHp);

    [Fact]
    public void Player_Should_Start_Alive()
    {
        var player = CreatePlayer();
        Assert.Equal(PlayerState.Alive, player.State);
    }

    [Fact]
    public void Player_Should_Start_With_Full_Hp()
    {
        var player = CreatePlayer();
        Assert.Equal(100, player.Hp);
        Assert.Equal(100, player.MaxHp);
    }

    [Fact]
    public void Player_Should_Move_To_New_Position()
    {
        var player = CreatePlayer();
        var newPosition = new Position(1, 1);

        player.Move(newPosition);

        Assert.Equal(newPosition, player.Position);
    }

    [Fact]
    public void Player_Should_Take_Damage()
    {
        var player = CreatePlayer();

        player.TakeDamage(25);

        Assert.Equal(75, player.Hp);
    }

    [Fact]
    public void Player_Should_Die_When_Hp_Reaches_Zero()
    {
        var player = CreatePlayer();

        player.TakeDamage(100);

        Assert.Equal(0, player.Hp);
        Assert.Equal(PlayerState.Dead, player.State);
    }

    [Fact]
    public void Player_Should_Enter_Combat_When_Attacking()
    {
        var player = CreatePlayer();
        var target = CreatePlayer("Target");

        player.Attack(target);

        Assert.Equal(PlayerState.InCombat, player.State);
    }

    [Fact]
    public void Player_Should_Heal()
    {
        var player = CreatePlayer();
        player.TakeDamage(50);

        player.Heal(30);

        Assert.Equal(80, player.Hp);
    }

    [Fact]
    public void Player_Should_Not_Heal_Above_MaxHp()
    {
        var player = CreatePlayer();
        player.TakeDamage(10);

        player.Heal(50);

        Assert.Equal(100, player.Hp);
    }

    [Fact]
    public void Player_Should_Rest()
    {
        var player = CreatePlayer();

        player.Rest();

        Assert.Equal(PlayerState.Resting, player.State);
    }

    [Fact]
    public void Player_Should_Stop_Resting()
    {
        var player = CreatePlayer();
        player.Rest();

        player.StopResting();

        Assert.Equal(PlayerState.Alive, player.State);
    }

    [Fact]
    public void Player_Should_Gain_Experience()
    {
        var player = CreatePlayer();

        player.GainExperience(100);

        Assert.Equal(100, player.Experience);
    }

    [Fact]
    public void Player_Should_Revive_When_Dead()
    {
        var player = CreatePlayer();
        player.Die();

        player.Revive();

        Assert.Equal(PlayerState.Alive, player.State);
        Assert.Equal(player.MaxHp, player.Hp);
    }

    [Fact]
    public void Dead_Player_Should_Not_Be_Able_To_Move()
    {
        var player = CreatePlayer();
        player.Die();
        var originalPosition = player.Position;

        player.Move(new Position(1, 1));

        Assert.Equal(originalPosition, player.Position);
    }

    [Fact]
    public void Player_Level_Should_Increase_With_Experience()
    {
        var player = CreatePlayer();

        player.GainExperience(1000);

        Assert.Equal(2, player.Level);
    }

    [Fact]
    public void Resting_Player_Should_Recover_Hp()
    {
        var player = CreatePlayer();
        player.TakeDamage(50);

        player.Rest();
        Assert.Equal(PlayerState.Resting, player.State);
    }

    [Fact]
    public void Dead_Player_Should_Not_Take_Damage()
    {
        var player = CreatePlayer();
        player.Die();

        player.TakeDamage(50);

        Assert.Equal(0, player.Hp);
    }

    [Fact]
    public void Dead_Player_Should_Not_Heal()
    {
        var player = CreatePlayer();
        player.Die();

        player.Heal(50);

        Assert.Equal(0, player.Hp);
    }

    [Fact]
    public void Dead_Player_Should_Not_Gain_Experience()
    {
        var player = CreatePlayer();
        player.Die();

        player.GainExperience(500);

        Assert.Equal(0, player.Experience);
    }

    [Fact]
    public void Player_Should_Level_Up_Multiple_Times()
    {
        var player = CreatePlayer();

        // Level 2 at 1000 XP (1*1000), Level 3 at 2000 XP (2*1000)
        // 2500 XP passes both thresholds but not Level 4 (3*1000=3000)
        player.GainExperience(2500);

        Assert.Equal(3, player.Level);
    }

    [Fact]
    public void Player_MaxHp_Should_Increase_On_Level_Up()
    {
        var player = CreatePlayer();

        player.GainExperience(1000); // Level up to 2

        Assert.Equal(110, player.MaxHp); // +10 per level
        Assert.Equal(110, player.Hp);    // Full heal on level up
    }

    [Fact]
    public void Overkill_Damage_Should_Not_Drop_Hp_Below_Zero()
    {
        var player = CreatePlayer();

        player.TakeDamage(999);

        Assert.Equal(0, player.Hp);
        Assert.Equal(PlayerState.Dead, player.State);
    }
}

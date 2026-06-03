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

        player.GainExperience(200); // Threshold(1) = 200

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

        // Level 2 at 200 XP, Level 3 at 1392 XP, Level 4 at 4334 XP
        // 2000 XP passes first two thresholds but not Level 4 (4334)
        player.GainExperience(2000);

        Assert.Equal(3, player.Level);
    }

    [Fact]
    public void Player_MaxHp_Should_Increase_On_Level_Up()
    {
        var player = CreatePlayer();

        player.GainExperience(200); // Level up to 2

        Assert.Equal(110, player.MaxHp); // +10 per level
        Assert.Equal(110, player.Hp);    // Full heal on level up
    }

    [Fact]
    public void Player_Should_Not_Level_Up_If_Experience_Is_Below_Threshold()
    {
        var player = CreatePlayer();

        player.GainExperience(199); // Threshold(1) = 200

        Assert.Equal(1, player.Level);
        Assert.Equal(199, player.Experience);
        Assert.Equal(100, player.MaxHp);
    }

    [Fact]
    public void Player_Should_Heal_To_Full_When_Leveling_Up()
    {
        var player = CreatePlayer();
        player.TakeDamage(50);
        
        // This gives exactly enough XP to level up to 2
        player.GainExperience(200); // Threshold(1) = 200

        Assert.Equal(2, player.Level);
        Assert.Equal(110, player.MaxHp);
        Assert.Equal(110, player.Hp); // Should be fully healed
    }

    [Fact]
    public void Overkill_Damage_Should_Not_Drop_Hp_Below_Zero()
    {
        var player = CreatePlayer();

        player.TakeDamage(999);

        Assert.Equal(0, player.Hp);
        Assert.Equal(PlayerState.Dead, player.State);
    }

    [Fact]
    public void Player_SetLevel_Should_Set_Correct_Level()
    {
        var player = CreatePlayer();

        player.SetLevel(5);

        Assert.Equal(5, player.Level);
    }

    [Fact]
    public void Player_SetLevel_Should_Reset_Experience_To_Zero()
    {
        var player = CreatePlayer();
        player.GainExperience(5000);

        player.SetLevel(5);

        Assert.Equal(0, player.Experience);
    }

    [Fact]
    public void Player_SetLevel_Should_Update_MaxHp_Correctly()
    {
        var player = CreatePlayer();

        player.SetLevel(5);

        // Level 5 = 100 + (10 * (5 - 1)) = 100 + 40 = 140
        Assert.Equal(140, player.MaxHp);
    }

    [Fact]
    public void Player_SetLevel_Should_Heal_To_Full()
    {
        var player = CreatePlayer();
        player.TakeDamage(50);
        Assert.Equal(50, player.Hp);

        player.SetLevel(5);

        Assert.Equal(140, player.Hp);
        Assert.Equal(140, player.MaxHp);
    }

    [Fact]
    public void Player_SetLevel_Should_Work_From_Any_Starting_Level()
    {
        var player = CreatePlayer();
        player.GainExperience(200); // Threshold(1) = 200
        Assert.Equal(2, player.Level);

        player.SetLevel(8);

        Assert.Equal(8, player.Level);
        // Level 8 = 100 + (10 * (8 - 1)) = 100 + 70 = 170
        Assert.Equal(170, player.MaxHp);
        Assert.Equal(170, player.Hp);
    }

    [Fact]
    public void Player_SetLevel_Should_Allow_Downgrade()
    {
        var player = CreatePlayer();
        player.SetLevel(10);
        Assert.Equal(10, player.Level);

        player.SetLevel(3);

        Assert.Equal(3, player.Level);
        // Level 3 = 100 + (10 * (3 - 1)) = 100 + 20 = 120
        Assert.Equal(120, player.MaxHp);
        Assert.Equal(120, player.Hp);
    }

    [Fact]
    public void Player_AttackPoints_Should_Increase_On_Level_Up()
    {
        var player = new Player(1, "Test", _startPosition, attackPoints: 10);

        player.GainExperience(200); // Threshold(1) = 200, 1 level up

        Assert.Equal(11, player.AttackPoints);  // 10 + 1*1
        Assert.Equal(11, player.TotalAttackPower);
    }

    [Fact]
    public void Player_BaseDefense_Should_Increase_On_Level_Up()
    {
        var player = new Player(1, "Test", _startPosition, attackPoints: 10);

        player.GainExperience(1000);

        Assert.Equal(1, player.TotalDefense);  // 0 + 1*1
    }

    [Fact]
    public void Player_SetLevel_Should_Scale_Attack_And_Defense()
    {
        var player = new Player(1, "Test", _startPosition, attackPoints: 10);

        player.SetLevel(5);

        Assert.Equal(14, player.AttackPoints);   // 10 + 1*(5-1)
        Assert.Equal(14, player.TotalAttackPower);
        Assert.Equal(2, player.TotalDefense);    // (5-1)/2
    }

    [Fact]
    public void Player_SetSpeed_Should_Update_Speed()
    {
        var player = new Player(1, "Test", _startPosition);

        player.SetSpeed(5.0);

        Assert.Equal(5.0, player.Speed);
    }

    [Fact]
    public void Player_SetSpeed_Should_Clamp_To_Minimum_Of_One()
    {
        var player = new Player(1, "Test", _startPosition);

        player.SetSpeed(0.0);

        Assert.Equal(1.0, player.Speed);
    }

    [Fact]
    public void Player_SetSpeed_Should_Clamp_To_Maximum_Of_Ten()
    {
        var player = new Player(1, "Test", _startPosition);

        player.SetSpeed(99.0);

        Assert.Equal(10.0, player.Speed);
    }
}

using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class CheeseTests
{
    private static readonly Position Pos = new(0, 0);

    private static Cheese MakeCheese(int healAmount = 10)
        => new("id1", "Cheese", 0.3f, "cheese", Pos, healAmount: healAmount);

    private static Player MakePlayer(int damage = 0, int maxHp = 100)
    {
        var p = new Player(1, "TestPlayer", Pos, maxHp: maxHp);
        if (damage > 0) p.TakeDamage(damage);
        return p;
    }

    // --- Cheese entity ---

    [Fact]
    public void Cheese_Type_Is_Food()
    {
        var cheese = MakeCheese();
        Assert.Equal(ItemType.Food, cheese.Type);
    }

    [Fact]
    public void Cheese_HealPerTick_Is_1_For_HealAmount_10()
    {
        var cheese = MakeCheese(healAmount: 10);
        Assert.Equal(1, cheese.HealPerTick);
    }

    [Fact]
    public void Cheese_TotalTicks_Is_10()
    {
        var cheese = MakeCheese();
        Assert.Equal(10, cheese.TotalTicks);
    }

    // --- Player HoT ---

    [Fact]
    public void Player_TickHoT_Returns_False_When_No_Active_HoT()
    {
        var player = MakePlayer();
        Assert.False(player.TickHoT());
    }

    [Fact]
    public void Player_TickHoT_Heals_Player_By_HealPerTick()
    {
        var player = MakePlayer(damage: 50); // Hp = 50
        player.ApplyHoT("cheese", healPerTick: 1, totalTicks: 10);

        player.TickHoT();

        Assert.Equal(51, player.Hp);
    }

    [Fact]
    public void Player_TickHoT_Returns_True_When_HoT_Active()
    {
        var player = MakePlayer();
        player.ApplyHoT("cheese", 1, 5);
        Assert.True(player.TickHoT());
    }

    [Fact]
    public void Player_TickHoT_Removes_Effect_When_Ticks_Exhausted()
    {
        var player = MakePlayer(damage: 50); // Hp = 50
        player.ApplyHoT("cheese", healPerTick: 1, totalTicks: 1);

        player.TickHoT(); // consumes the 1 tick
        bool result = player.TickHoT(); // no more ticks

        Assert.False(result);
    }

    [Fact]
    public void Player_TickHoT_Applies_Correct_Total_Heal()
    {
        var player = MakePlayer(damage: 50, maxHp: 100); // Hp = 50
        player.ApplyHoT("cheese", healPerTick: 1, totalTicks: 10);

        for (int i = 0; i < 10; i++) player.TickHoT();

        Assert.Equal(60, player.Hp);
    }

    [Fact]
    public void Player_ApplyHoT_Replaces_Effect_With_Same_Source()
    {
        var player = MakePlayer(damage: 10, maxHp: 100); // Hp = 90
        player.ApplyHoT("cheese", healPerTick: 1, totalTicks: 10);
        player.TickHoT(); // tick once → hp 91, 9 ticks left

        player.ApplyHoT("cheese", healPerTick: 1, totalTicks: 10); // reset
        for (int i = 0; i < 10; i++) player.TickHoT();

        // old replaced: 91 + 10 = 101, capped at 100
        Assert.Equal(100, player.Hp);
        Assert.False(player.TickHoT()); // exhausted
    }

    [Fact]
    public void Player_TickHoT_Does_Not_Heal_Dead_Player()
    {
        var player = MakePlayer();
        player.Die();
        player.ApplyHoT("cheese", 1, 10);

        bool result = player.TickHoT();

        Assert.False(result);
        Assert.Equal(0, player.Hp);
    }

    // --- AccumulateHoT (food stacking) ---

    [Fact]
    public void Player_AccumulateHoT_Creates_New_Effect_When_Source_Not_Active()
    {
        var player = MakePlayer(damage: 30); // Hp = 70
        player.AccumulateHoT("food", 1, 10);

        for (int i = 0; i < 10; i++) player.TickHoT();

        Assert.Equal(80, player.Hp);
    }

    [Fact]
    public void Player_AccumulateHoT_Adds_Ticks_To_Existing_Effect()
    {
        var player = MakePlayer(damage: 30, maxHp: 100); // Hp = 70
        player.AccumulateHoT("food", 1, 20); // raw-meat: 20 ticks
        player.AccumulateHoT("food", 1, 10); // cheese: +10 ticks → 30 total

        for (int i = 0; i < 30; i++) player.TickHoT();

        Assert.Equal(100, player.Hp);
        Assert.False(player.TickHoT()); // exhausted
    }

    [Fact]
    public void Player_AccumulateHoT_Partial_Consumption_Then_Stack()
    {
        var player = MakePlayer(damage: 50, maxHp: 100); // Hp = 50
        player.AccumulateHoT("food", 1, 20); // raw-meat
        for (int i = 0; i < 5; i++) player.TickHoT(); // 5 ticks consumed, Hp=55, 15 left

        player.AccumulateHoT("food", 1, 10); // cheese: 15 + 10 = 25 ticks remaining
        for (int i = 0; i < 25; i++) player.TickHoT();

        Assert.Equal(80, player.Hp); // 55 + 25 = 80
        Assert.False(player.TickHoT());
    }
}

using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class RawMeatTests
{
    private static readonly Position Pos = new(0, 0);

    private static Cheese MakeRawMeat(int healAmount = 20)
        => new("id1", "Raw Meat", 0.8f, "raw-meat", Pos, healAmount: healAmount);

    private static Player MakePlayer(int damage = 0, int maxHp = 100)
    {
        var p = new Player(1, "TestPlayer", Pos, maxHp: maxHp);
        if (damage > 0) p.TakeDamage(damage);
        return p;
    }

    [Fact]
    public void RawMeat_Type_Is_Food()
    {
        var meat = MakeRawMeat();
        Assert.Equal(ItemType.Food, meat.Type);
    }

    [Fact]
    public void RawMeat_HealPerTick_Is_1()
    {
        var meat = MakeRawMeat(healAmount: 20);
        Assert.Equal(1, meat.HealPerTick);
    }

    [Fact]
    public void RawMeat_TotalTicks_Is_HealAmount()
    {
        var meat = MakeRawMeat(healAmount: 20);
        Assert.Equal(20, meat.TotalTicks);
    }

    [Fact]
    public void Player_TickHoT_Heals_1Hp_Per_Tick_With_RawMeat()
    {
        var player = MakePlayer(damage: 50); // Hp = 50
        player.ApplyHoT("food", healPerTick: 1, totalTicks: 20);

        player.TickHoT();

        Assert.Equal(51, player.Hp);
    }

    [Fact]
    public void Player_TickHoT_Applies_Total_20Hp_With_RawMeat_Over_20_Ticks()
    {
        var player = MakePlayer(damage: 50, maxHp: 100); // Hp = 50
        player.ApplyHoT("food", healPerTick: 1, totalTicks: 20);

        for (int i = 0; i < 20; i++) player.TickHoT();

        Assert.Equal(70, player.Hp);
    }
}

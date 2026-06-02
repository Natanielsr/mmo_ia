using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServer.Tests.World;

public class HealingItemTests
{
    private static readonly Position Pos = new(0, 0);

    [Fact]
    public void HealingItem_Food_Type_Is_Food()
    {
        var item = new HealingItem("id1", "Cheese", 0.3f, "cheese", Pos, ItemType.Food, healAmount: 20);
        Assert.Equal(ItemType.Food, item.Type);
    }

    [Fact]
    public void HealingItem_Food_HealAmount_Correct()
    {
        var item = new HealingItem("id1", "Cheese", 0.3f, "cheese", Pos, ItemType.Food, healAmount: 20);
        Assert.Equal(20, item.HealAmount);
    }

    [Fact]
    public void HealingItem_Potion_Type_Is_Potion()
    {
        var item = new HealingItem("id2", "Healing Potion", 0.5f, "potion", Pos, ItemType.Potion, healAmount: 20);
        Assert.Equal(ItemType.Potion, item.Type);
    }

    [Fact]
    public void HealingItem_Potion_HealAmount_Correct()
    {
        var item = new HealingItem("id2", "Healing Potion", 0.5f, "potion", Pos, ItemType.Potion, healAmount: 20);
        Assert.Equal(20, item.HealAmount);
    }

    [Fact]
    public void HealingItem_RawMeat_HealAmount_Is_40()
    {
        var item = new HealingItem("id3", "Raw Meat", 0.8f, "raw-meat", Pos, ItemType.Food, healAmount: 40);
        Assert.Equal(40, item.HealAmount);
    }
}

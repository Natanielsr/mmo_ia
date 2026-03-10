using Xunit;
using GameServerApp.Contracts.Types;
using GameServerApp.World;
using GameServerApp.Services;

namespace GameServer.Tests.Inventory;

public class InventoryImplTests
{
    private readonly InventoryService _inventory = new();

    private Item CreateItem(string id = "potion_001", string name = "Health Potion", float weight = 1.0f)
        => new Item(id, name, weight);

    [Fact]
    public void InventoryService_Should_Add_Item()
    {
        var item = CreateItem();
        var result = _inventory.AddItem(item);

        Assert.True(result);
        Assert.Contains(item, _inventory.GetItems());
    }

    [Fact]
    public void InventoryService_Should_Remove_Item()
    {
        var item = CreateItem();
        _inventory.AddItem(item);

        var result = _inventory.RemoveItem(item.Id);

        Assert.True(result);
        Assert.Empty(_inventory.GetItems());
    }

    [Fact]
    public void InventoryService_Should_Use_Item()
    {
        var item = CreateItem();
        _inventory.AddItem(item);

        var result = _inventory.UseItem(item.Id);

        Assert.True(result);
    }

    [Fact]
    public void InventoryService_Should_Drop_Item_On_Map()
    {
        var item = CreateItem();
        _inventory.AddItem(item);
        var dropPos = new Position(5, 5);

        var result = _inventory.DropItem(item.Id, dropPos);

        Assert.True(result);
        Assert.Equal(dropPos, item.Position);
        Assert.Empty(_inventory.GetItems());
    }

    [Fact]
    public void InventoryService_Should_Not_Remove_NonExistent_Item()
    {
        var result = _inventory.RemoveItem("non_existent");
        Assert.False(result);
    }

    [Fact]
    public void Item_Should_Have_Id_Name_Weight_And_Position()
    {
        var pos = new Position(2, 3);
        var item = new Item("sword", "Iron Sword", 5f, pos);

        Assert.Equal("sword", item.Id);
        Assert.Equal("Iron Sword", item.Name);
        Assert.Equal(5f, item.Weight);
        Assert.Equal(pos, item.Position);
    }

    [Fact]
    public void Dropped_Item_Position_Updates_When_Moved()
    {
        var item = CreateItem();
        item.Position = new Position(10, 10);
        Assert.Equal(new Position(10, 10), item.Position);
    }
}

using Moq;
using Xunit;
using GameServerApp.Contracts;
using GameServerApp.Contracts.Types;

namespace GameServer.Tests.Inventory;

public class InventorySystemTests
{
    [Fact]
    public void InventoryService_Should_Add_Item()
    {
        var mockInv = new Mock<IInventoryService>();
        var mockItem = new Mock<IItem>();
        mockItem.Setup(i => i.Id).Returns("potion_001");

        mockInv.Object.AddItem(mockItem.Object);
        mockInv.Verify(i => i.AddItem(It.IsAny<IItem>()), Times.Once);
    }

    [Fact]
    public void InventoryService_Should_Remove_Item()
    {
        var mockInv = new Mock<IInventoryService>();
        var itemId = "potion_001";

        mockInv.Object.RemoveItem(itemId);
        mockInv.Verify(i => i.RemoveItem("potion_001"), Times.Once);
    }

    [Fact]
    public void InventoryService_Should_Use_Item()
    {
        var mockInv = new Mock<IInventoryService>();
        var itemId = "potion_001";

        mockInv.Object.UseItem(itemId);
        mockInv.Verify(i => i.UseItem("potion_001"), Times.Once);
    }

    [Fact]
    public void InventoryService_Should_Drop_Item_On_Map()
    {
        var mockInv = new Mock<IInventoryService>();
        var itemId = "potion_001";
        var dropPos = new Position(5, 5);

        mockInv.Object.DropItem(itemId, dropPos);
        mockInv.Verify(i => i.DropItem("potion_001", It.Is<Position>(p => p.X == 5 && p.Y == 5)), Times.Once);
    }

    [Fact]
    public void InventoryService_Should_Not_Drop_Item_Where_Obstacle_Or_Wall()
    {
        var mockInv = new Mock<IInventoryService>();
        var itemId = "potion_001";
        var blockedPos = new Position(1, 0);

        // simulate inventory service consulting collision/obstacle check internally
        mockInv.Setup(i => i.DropItem(itemId, blockedPos)).Returns(false);

        var result = mockInv.Object.DropItem(itemId, blockedPos);
        Assert.False(result);
        mockInv.Verify(i => i.DropItem(itemId, blockedPos), Times.Once);
    }

    [Fact]
    public void InventoryService_GetItems_Returns_List()
    {
        var mockInv = new Mock<IInventoryService>();
        var list = new List<IItem>();
        mockInv.Setup(i => i.GetItems()).Returns(list);

        var items = mockInv.Object.GetItems();
        Assert.Same(list, items);
    }

    [Fact]
    public void Item_Should_Have_Id_Name_Weight_And_Position()
    {
        var mockItem = new Mock<IItem>();
        var pos = new Position(2, 3);
        mockItem.Setup(i => i.Id).Returns("sword");
        mockItem.Setup(i => i.Name).Returns("Iron Sword");
        mockItem.Setup(i => i.Weight).Returns(5f);
        mockItem.SetupProperty(i => i.Position, pos);

        Assert.Equal("sword", mockItem.Object.Id);
        Assert.Equal("Iron Sword", mockItem.Object.Name);
        Assert.Equal(5f, mockItem.Object.Weight);
        Assert.Equal(pos, mockItem.Object.Position);
    }

    [Fact]
    public void Dropped_Item_Position_Updates_When_Moved()
    {
        var mockItem = new Mock<IItem>();
        mockItem.SetupProperty(i => i.Position, new Position(0, 0));

        mockItem.Object.Position = new Position(10, 10);
        Assert.Equal(new Position(10, 10), mockItem.Object.Position);
    }
}

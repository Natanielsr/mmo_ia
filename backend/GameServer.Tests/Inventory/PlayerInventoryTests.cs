using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Inventory
{
    /// <summary>
    /// Testa comportamentos exclusivos de PlayerInventory que não estão cobertos por
    /// InventoryImplTests (que testa InventoryService — operações básicas add/remove/drop).
    /// </summary>
    public class PlayerInventoryTests
    {
        private readonly PlayerInventory _inv = new();

        private static IItem MakeItem(string id = "item1", ItemType type = ItemType.Potion)
        {
            var m = new Mock<IItem>();
            m.Setup(i => i.Id).Returns(id);
            m.Setup(i => i.Type).Returns(type);
            m.SetupProperty(i => i.Position, new Position(0, 0));
            return m.Object;
        }

        [Fact]
        public void Inventory_Starts_Empty()
            => Assert.Empty(_inv.GetItems());

        [Fact]
        public void AddItem_Returns_False_When_Inventory_Is_Full_At_20_Slots()
        {
            for (int i = 0; i < 20; i++) _inv.AddItem(MakeItem($"item{i}"));
            Assert.False(_inv.AddItem(MakeItem("overflow")));
            Assert.Equal(20, _inv.GetItems().Count);
        }

        [Fact]
        public void AddItem_Returns_False_For_Null_Item()
            => Assert.False(_inv.AddItem(null!));

        [Fact]
        public void UseItem_Returns_False_When_Item_Not_Found()
        {
            var player = new Mock<IPlayer>();
            Assert.False(_inv.UseItem("ghost", player.Object));
        }

        [Fact]
        public void UseItem_On_Potion_Heals_Player_And_Removes_Item()
        {
            var item = MakeItem("p1", ItemType.Potion);
            _inv.AddItem(item);
            var player = new Mock<IPlayer>();

            Assert.True(_inv.UseItem("p1", player.Object));

            player.Verify(p => p.Heal(It.IsAny<int>()), Times.Once);
            Assert.Empty(_inv.GetItems());
        }

        [Fact]
        public void UseItem_On_Non_Potion_Returns_False()
        {
            var item = MakeItem("w1", ItemType.Weapon);
            _inv.AddItem(item);
            var player = new Mock<IPlayer>();

            Assert.False(_inv.UseItem("w1", player.Object));

            player.Verify(p => p.Heal(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void DropItem_Removes_Item_And_Updates_Position()
        {
            var item = MakeItem();
            _inv.AddItem(item);
            var pos = new Position(5, 5);

            Assert.True(_inv.DropItem("item1", pos));

            Assert.Empty(_inv.GetItems());
            Assert.Equal(pos, item.Position);
        }

        [Fact]
        public void DropItem_Returns_False_When_Item_Not_Found()
            => Assert.False(_inv.DropItem("ghost", new Position(0, 0)));
    }
}

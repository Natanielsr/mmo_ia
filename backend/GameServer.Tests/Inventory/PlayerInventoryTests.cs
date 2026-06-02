using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;
using Moq;

namespace GameServer.Tests.Inventory
{
    public class PlayerInventoryTests
    {
        private readonly PlayerInventory _inv = new();

        private static IItem MakeItem(string id = "item1", ItemType type = ItemType.Potion, int quantity = 1)
        {
            var m = new Mock<IItem>();
            m.Setup(i => i.Id).Returns(id);
            m.Setup(i => i.Type).Returns(type);
            m.SetupProperty(i => i.Position, new Position(0, 0));
            m.SetupProperty(i => i.Quantity, quantity);
            return m.Object;
        }

        [Fact]
        public void Inventory_Starts_Empty()
            => Assert.Empty(_inv.GetItems());

        [Fact]
        public void AddItem_Returns_False_When_Inventory_Is_Full_At_20_Slots()
        {
            for (int i = 0; i < 20; i++) _inv.AddItem(MakeItem($"item{i}", ItemType.Weapon));
            Assert.False(_inv.AddItem(MakeItem("overflow", ItemType.Weapon)));
            Assert.Equal(20, _inv.GetItems().Count);
        }

        [Fact]
        public void AddItem_Returns_False_For_Null_Item()
            => Assert.False(_inv.AddItem(null!));

        [Fact]
        public void AddItem_Stacks_Second_Potion_Into_Same_Slot()
        {
            var p1 = MakeItem("p1", ItemType.Potion);
            var p2 = MakeItem("p2", ItemType.Potion);
            _inv.AddItem(p1);
            Assert.True(_inv.AddItem(p2));
            Assert.Single(_inv.GetItems());
            Assert.Equal(2, _inv.GetItems()[0].Quantity);
        }

        [Fact]
        public void AddItem_Non_Potion_Does_Not_Stack()
        {
            _inv.AddItem(MakeItem("w1", ItemType.Weapon));
            _inv.AddItem(MakeItem("w2", ItemType.Weapon));
            Assert.Equal(2, _inv.GetItems().Count);
        }

        [Fact]
        public void UseItem_Returns_False_When_Item_Not_Found()
        {
            var player = new Mock<IPlayer>();
            Assert.False(_inv.UseItem("ghost", player.Object));
        }

        [Fact]
        public void UseItem_On_Potion_Heals_Player_And_Removes_Slot_When_Quantity_Is_1()
        {
            var item = new HealingItem("p1", "Potion", 0.5f, "potion", new Position(0, 0), ItemType.Potion, healAmount: 20);
            _inv.AddItem(item);
            var player = new Mock<IPlayer>();

            Assert.True(_inv.UseItem("p1", player.Object));

            player.Verify(p => p.Heal(It.IsAny<int>()), Times.Once);
            Assert.Empty(_inv.GetItems());
        }

        [Fact]
        public void UseItem_On_Stacked_Potion_Decrements_Quantity_Without_Removing_Slot()
        {
            var item = new HealingItem("p1", "Potion", 0.5f, "potion", new Position(0, 0), ItemType.Potion, healAmount: 20);
            item.Quantity = 3;
            _inv.AddItem(item);
            var player = new Mock<IPlayer>();

            Assert.True(_inv.UseItem("p1", player.Object));

            player.Verify(p => p.Heal(It.IsAny<int>()), Times.Once);
            Assert.Single(_inv.GetItems());
            Assert.Equal(2, _inv.GetItems()[0].Quantity);
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
            var item = MakeItem("item1", ItemType.Weapon, quantity: 1);
            _inv.AddItem(item);
            var pos = new Position(5, 5);

            Assert.True(_inv.DropItem("item1", pos));

            Assert.Empty(_inv.GetItems());
            Assert.Equal(pos, item.Position);
        }

        [Fact]
        public void DropItem_Returns_False_When_Item_Not_Found()
            => Assert.False(_inv.DropItem("ghost", new Position(0, 0)));

        // --- Food (Cheese) ---

        private static GameServerApp.World.HealingItem MakeCheese(string id = "c1", int quantity = 1)
        {
            var cheese = new GameServerApp.World.HealingItem(id, "Cheese", 0.3f, "cheese", new Position(0, 0),
                GameServerApp.Contracts.World.ItemType.Food, healAmount: 10);
            cheese.Quantity = quantity;
            return cheese;
        }

        [Fact]
        public void AddItem_Food_Same_TagName_Stacks()
        {
            var c1 = MakeCheese("c1");
            var c2 = MakeCheese("c2");
            _inv.AddItem(c1);
            Assert.True(_inv.AddItem(c2));
            Assert.Single(_inv.GetItems());
            Assert.Equal(2, _inv.GetItems()[0].Quantity);
        }

        [Fact]
        public void UseItem_Food_Calls_Heal_On_Player_And_Returns_True()
        {
            var cheese = MakeCheese();
            _inv.AddItem(cheese);
            var player = new Mock<IPlayer>();

            bool result = _inv.UseItem("c1", player.Object);

            Assert.True(result);
            player.Verify(p => p.Heal(10), Times.Once);
        }

        [Fact]
        public void UseItem_Food_Decrements_Quantity_And_Removes_When_Zero()
        {
            var cheese = MakeCheese(quantity: 1);
            _inv.AddItem(cheese);
            var player = new Mock<IPlayer>();

            _inv.UseItem("c1", player.Object);

            Assert.Empty(_inv.GetItems());
        }

        [Fact]
        public void UseItem_Food_Stacked_Decrements_Without_Removing()
        {
            var cheese = MakeCheese(quantity: 3);
            _inv.AddItem(cheese);
            var player = new Mock<IPlayer>();

            _inv.UseItem("c1", player.Object);

            Assert.Single(_inv.GetItems());
            Assert.Equal(2, _inv.GetItems()[0].Quantity);
        }
    }
}

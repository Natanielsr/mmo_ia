using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;
using GameServer.Tests.Helpers;
using Moq;

namespace GameServer.Tests.Managers
{
    public class WorldManagerPickupTests
    {
        // WithRealServices para que ProcessPlayerMovement execute de verdade e
        // dispare ProcessItemPickup ao mover para a posição do item.
        private readonly WorldManagerBuilder _b = new WorldManagerBuilder().WithRealServices();

        [Fact]
        public void Pickup_Does_Not_Auto_Heal_Player()
        {
            var player = new Player(1, "Hero", new Position(0, 0), maxHp: 50);
            var potion = new HealingPotion("p1", new Position(1, 0));

            _b.ItemManager.Setup(m => m.GetItemAt(new Position(1, 0))).Returns(potion);
            _b.InventoryManager.Setup(m => m.AddItem(1L, potion)).Returns(true);

            int hpBefore = player.Hp;
            _b.Build().ProcessPlayerMovement(player, "east");

            Assert.Equal(hpBefore, player.Hp);
        }

        [Fact]
        public void Pickup_Adds_Item_To_Player_Inventory()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var potion = new HealingPotion("p1", new Position(1, 0));

            _b.ItemManager.Setup(m => m.GetItemAt(new Position(1, 0))).Returns(potion);
            _b.InventoryManager.Setup(m => m.AddItem(1L, potion)).Returns(true);

            _b.Build().ProcessPlayerMovement(player, "east");

            _b.InventoryManager.Verify(m => m.AddItem(1L, potion), Times.Once);
            _b.ItemManager.Verify(m => m.RemoveItem("p1"), Times.Once);
        }

        [Fact]
        public void Pickup_Fails_Silently_When_Inventory_Full()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var potion = new HealingPotion("p1", new Position(1, 0));

            _b.ItemManager.Setup(m => m.GetItemAt(new Position(1, 0))).Returns(potion);
            _b.InventoryManager.Setup(m => m.AddItem(1L, potion)).Returns(false);

            _b.Build().ProcessPlayerMovement(player, "east");

            _b.ItemManager.Verify(m => m.RemoveItem(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Pickup_Fires_InventoryUpdated_Event()
        {
            var player = new Player(1, "Hero", new Position(0, 0));
            var potion = new HealingPotion("p1", new Position(1, 0));

            _b.ItemManager.Setup(m => m.GetItemAt(new Position(1, 0))).Returns(potion);
            _b.InventoryManager.Setup(m => m.AddItem(1L, potion)).Returns(true);
            _b.InventoryManager.Setup(m => m.GetInventory(1L))
                               .Returns(() => { var inv = new GameServerApp.World.PlayerInventory(); return inv; });

            _b.Build().ProcessPlayerMovement(player, "east");

            _b.Events.Verify(e => e.OnInventoryUpdated(1L, It.IsAny<IReadOnlyList<IItem>>()), Times.Once);
        }

        [Fact]
        public void UseItem_From_Inventory_Heals_Player()
        {
            var pos    = new Position(0, 0);
            var player = new Player(1, "Hero", pos, maxHp: 100);
            player.TakeDamage(40); // garante espaço para cura
            var potion = new HealingPotion("p1", pos);

            var inv = new GameServerApp.World.PlayerInventory();
            inv.AddItem(potion);
            _b.InventoryManager.Setup(m => m.GetInventory(1L)).Returns(inv);

            int hpBefore = player.Hp;
            _b.Build().ProcessUseItem(player, "p1");

            Assert.True(player.Hp > hpBefore);
            _b.Events.Verify(e => e.OnPlayerStatusUpdated(It.Is<PlayerStatusData>(d =>
                d.Id == "1")), Times.Once);
            _b.Events.Verify(e => e.OnInventoryUpdated(1L, It.IsAny<IReadOnlyList<IItem>>()), Times.Once);
        }

        [Fact]
        public void DropItem_From_Inventory_Places_Item_On_Map()
        {
            var pos    = new Position(0, 0);
            var player = new Player(1, "Hero", pos);
            var sword  = new Weapon("w1", "Sword", 1f, pos, attackBonus: 5);
            var target = new Position(3, 3);

            var inv = new GameServerApp.World.PlayerInventory();
            inv.AddItem(sword);
            _b.InventoryManager.Setup(m => m.GetInventory(1L)).Returns(inv);

            _b.Build().ProcessDropItem(player, "w1", target);

            _b.ItemManager.Verify(m => m.DropItem(sword), Times.Once);
            _b.Events.Verify(e => e.OnItemDropped(sword), Times.Once);
            _b.Events.Verify(e => e.OnInventoryUpdated(1L, It.IsAny<IReadOnlyList<IItem>>()), Times.Once);
        }
    }
}

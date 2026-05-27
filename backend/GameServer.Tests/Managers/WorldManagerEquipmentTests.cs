using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.World;
using GameServer.Tests.Helpers;
using Moq;

namespace GameServer.Tests.Managers
{
    public class WorldManagerEquipmentTests
    {
        private readonly WorldManagerBuilder _b = new();
        private static Position P => new(0, 0);

        private (Player player, PlayerInventory inv, PlayerEquipment eq) Setup(
            System.Action<PlayerInventory>? addToInv = null,
            System.Action<PlayerEquipment>? addToEq  = null)
        {
            var player = new Player(1, "Hero", P, attackPoints: 10);
            var inv    = new PlayerInventory();
            var eq     = new PlayerEquipment();
            addToInv?.Invoke(inv);
            addToEq?.Invoke(eq);
            _b.InventoryManager.Setup(m => m.GetInventory(1L)).Returns(inv);
            _b.InventoryManager.Setup(m => m.GetOrCreate(1L)).Returns(inv);
            _b.EquipmentManager.Setup(m => m.GetOrCreate(1L)).Returns(eq);
            _b.EquipmentManager.Setup(m => m.GetEquipment(1L)).Returns(eq);
            return (player, inv, eq);
        }

        [Fact]
        public void ProcessEquipItem_Equips_From_Inventory_And_Fires_Events()
        {
            var sword = new Weapon("w1", "Sword", 1f, "sword", P, attackBonus: 5);
            var (player, inv, eq) = Setup(addToInv: i => i.AddItem(sword));

            var result = _b.Build().ProcessEquipItem(player, "w1");

            Assert.True(result);
            Assert.Same(sword, eq.GetItem(EquipmentSlot.Weapon));
            Assert.Empty(inv.GetItems());
            _b.Events.Verify(e => e.OnEquipmentUpdated(1L, It.IsAny<IReadOnlyDictionary<EquipmentSlot, IItem?>>()), Times.Once);
            _b.Events.Verify(e => e.OnInventoryUpdated(1L, It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Once);
        }

        [Fact]
        public void ProcessEquipItem_Returns_False_When_Item_Not_In_Inventory()
        {
            var (player, _, _) = Setup(); // empty inventory

            var result = _b.Build().ProcessEquipItem(player, "ghost");

            Assert.False(result);
            _b.Events.Verify(e => e.OnEquipmentUpdated(It.IsAny<long>(), It.IsAny<IReadOnlyDictionary<EquipmentSlot, IItem?>>()), Times.Never);
        }

        [Fact]
        public void ProcessEquipItem_Returns_False_When_Item_Not_Equippable()
        {
            var potion = new HealingPotion("p1", "Healing Potion", 0.1f, "healing-potion", P); // Slot == null
            var (player, _, _) = Setup(addToInv: i => i.AddItem(potion));

            var result = _b.Build().ProcessEquipItem(player, "p1");

            Assert.False(result);
        }

        [Fact]
        public void ProcessEquipItem_Returns_False_When_Player_Is_Dead()
        {
            var sword = new Weapon("w1", "Sword", 1f, "sword", P, attackBonus: 5);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(sword));
            player.Die();

            var result = _b.Build().ProcessEquipItem(player, "w1");

            Assert.False(result);
        }

        [Fact]
        public void ProcessEquipItem_Displaced_Item_Returns_To_Inventory()
        {
            var sword1 = new Weapon("w1", "Sword",   1f, "sword", P, attackBonus: 5);
            var sword2 = new Weapon("w2", "GreatAxe", 2f, "great-axe", P, attackBonus: 10);
            var (player, inv, eq) = Setup(
                addToInv: i => i.AddItem(sword2),
                addToEq:  e => e.Equip(sword1));

            _b.Build().ProcessEquipItem(player, "w2");

            Assert.Same(sword2, eq.GetItem(EquipmentSlot.Weapon));
            Assert.Contains(sword1, inv.GetItems()); // displaced came back
        }

        [Fact]
        public void ProcessUnequipItem_Returns_Item_To_Inventory()
        {
            var sword = new Weapon("w1", "Sword", 1f, "sword", P, attackBonus: 5);
            var (player, inv, eq) = Setup(addToEq: e => e.Equip(sword));

            var result = _b.Build().ProcessUnequipItem(player, EquipmentSlot.Weapon);

            Assert.True(result);
            Assert.Null(eq.GetItem(EquipmentSlot.Weapon));
            Assert.Contains(sword, inv.GetItems());
            _b.Events.Verify(e => e.OnEquipmentUpdated(1L, It.IsAny<IReadOnlyDictionary<EquipmentSlot, IItem?>>()), Times.Once);
            _b.Events.Verify(e => e.OnInventoryUpdated(1L, It.IsAny<IReadOnlyList<(int, IItem)>>()), Times.Once);
        }

        [Fact]
        public void ProcessUnequipItem_Returns_False_When_Slot_Empty()
        {
            var (player, _, _) = Setup(); // empty equipment

            var result = _b.Build().ProcessUnequipItem(player, EquipmentSlot.Weapon);

            Assert.False(result);
        }

        [Fact]
        public void EquipWeapon_Updates_Player_TotalAttackPower()
        {
            var sword = new Weapon("w1", "Sword", 1f, "sword", P, attackBonus: 5);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(sword));

            _b.Build().ProcessEquipItem(player, "w1");

            Assert.Equal(15, player.TotalAttackPower); // base 10 + bonus 5
            _b.Events.Verify(e => e.OnPlayerStatusUpdated(It.Is<PlayerStatusData>(d =>
                d.Id == "1" && d.AttackPower == 15)), Times.Once);
        }

        // ── OnPlayerItemEquipped broadcast ───────────────────────────────────

        [Fact]
        public void ProcessEquipItem_Weapon_Fires_ItemEquipped_With_Slot_And_Name()
        {
            var dagger = new Weapon("d1", "Dagger", 1f, "dagger", P, attackBonus: 3);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(dagger));

            _b.Build().ProcessEquipItem(player, "d1");

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Weapon", "dagger"), Times.Once);
        }

        [Fact]
        public void ProcessEquipItem_Legs_Fires_ItemEquipped_With_Slot_And_Name()
        {
            var pants = new Legs("l1", "Leather Pants", 1f, "leather-pants", P, defenseBonus: 1);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(pants));

            _b.Build().ProcessEquipItem(player, "l1");

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Legs", "leather-pants"), Times.Once);
        }

        [Fact]
        public void ProcessEquipItem_Helmet_Fires_ItemEquipped_With_Slot_And_Name()
        {
            var helmet = new Helmet("h1", "Iron Helmet", 1.2f, "iron-helmet", P, defenseBonus: 4);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(helmet));

            _b.Build().ProcessEquipItem(player, "h1");

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Helmet", "iron-helmet"), Times.Once);
        }

        [Fact]
        public void ProcessUnequipItem_Weapon_Fires_ItemEquipped_With_Null()
        {
            var dagger = new Weapon("d1", "Dagger", 1f, "dagger", P, attackBonus: 3);
            var (player, _, _) = Setup(addToEq: e => e.Equip(dagger));

            _b.Build().ProcessUnequipItem(player, EquipmentSlot.Weapon);

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Weapon", null), Times.Once);
        }

        [Fact]
        public void ProcessUnequipItem_Legs_Fires_ItemEquipped_With_Null()
        {
            var pants = new Legs("l1", "Leather Pants", 1f, "leather-pants", P, defenseBonus: 1);
            var (player, _, _) = Setup(addToEq: e => e.Equip(pants));

            _b.Build().ProcessUnequipItem(player, EquipmentSlot.Legs);

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Legs", null), Times.Once);
        }

        [Fact]
        public void ProcessUnequipItem_Helmet_Fires_ItemEquipped_With_Null()
        {
            var helmet = new Helmet("h1", "Iron Helmet", 1.2f, "iron-helmet", P, defenseBonus: 4);
            var (player, _, _) = Setup(addToEq: e => e.Equip(helmet));

            _b.Build().ProcessUnequipItem(player, EquipmentSlot.Helmet);

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Helmet", null), Times.Once);
        }

        [Fact]
        public void ProcessUnequipItem_Chest_Still_Fires_Weapon_Slot_With_Current_Weapon()
        {
            var dagger = new Weapon("d1", "Dagger", 1f, "dagger", P, attackBonus: 3);
            var vest   = new Armor ("a1", "Leather Vest", 2f, "leather-vest", P, defenseBonus: 3);
            var (player, _, _) = Setup(addToEq: e => { e.Equip(dagger); e.Equip(vest); });

            _b.Build().ProcessUnequipItem(player, EquipmentSlot.Chest);

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Weapon", "dagger"), Times.Once);
        }

        [Fact]
        public void ProcessEquipItem_DeadPlayer_Does_Not_Fire_ItemEquipped()
        {
            var dagger = new Weapon("d1", "Dagger", 1f, "dagger", P, attackBonus: 3);
            var (player, _, _) = Setup(addToInv: i => i.AddItem(dagger));
            player.Die();

            _b.Build().ProcessEquipItem(player, "d1");

            _b.Events.Verify(e => e.OnPlayerItemEquipped(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public void ProcessEquipItem_Displaces_OldWeapon_Fires_ItemEquipped_With_NewWeaponName()
        {
            var sword  = new Weapon("w1", "Sword",  1f, "sword",  P, attackBonus: 5);
            var dagger = new Weapon("d1", "Dagger", 1f, "dagger", P, attackBonus: 3);
            var (player, _, _) = Setup(
                addToInv: i => i.AddItem(dagger),
                addToEq:  e => e.Equip(sword));

            _b.Build().ProcessEquipItem(player, "d1");

            _b.Events.Verify(e => e.OnPlayerItemEquipped("1", "Weapon", "dagger"), Times.Once);
        }
    }
}

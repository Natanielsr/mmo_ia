using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServer.Tests.World
{
    public class PlayerEquipmentTests
    {
        private static Position P => new(0, 0);

        // ── Player stat properties ─────────────────────────────────────────────

        [Fact]
        public void Player_BaseAttackPower_Equals_AttackPoints_Before_Equipment()
        {
            var player = new Player(1, "Hero", P, attackPoints: 10);
            Assert.Equal(10, player.TotalAttackPower);
        }

        [Fact]
        public void Player_BaseDefense_Is_Zero_Before_Equipment()
        {
            var player = new Player(1, "Hero", P);
            Assert.Equal(0, player.TotalDefense);
        }

        [Fact]
        public void TakeDamage_Is_Reduced_By_Defense()
        {
            var player = new Player(1, "Hero", P, maxHp: 100);
            player.ApplyEquipmentBonuses(attackBonus: 0, defenseBonus: 5);

            player.TakeDamage(8); // 8 - 5 = 3 effective damage

            Assert.Equal(97, player.Hp);
        }

        [Fact]
        public void TakeDamage_Defense_Cannot_Go_Negative()
        {
            var player = new Player(1, "Hero", P, maxHp: 100);
            player.ApplyEquipmentBonuses(attackBonus: 0, defenseBonus: 50);

            player.TakeDamage(3); // 3 - 50 = -47, clamped to 0

            Assert.Equal(100, player.Hp);
        }

        // ── PlayerEquipment operations ─────────────────────────────────────────

        [Fact]
        public void EquipWeapon_Increases_TotalAttackPower()
        {
            var player = new Player(1, "Hero", P, attackPoints: 10);
            var eq     = new PlayerEquipment();
            var sword  = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);

            eq.Equip(sword);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            Assert.Equal(15, player.TotalAttackPower);
        }

        [Fact]
        public void EquipWeapon_When_SlotOccupied_Replaces_And_Returns_Old_Item()
        {
            var eq    = new PlayerEquipment();
            var sw1   = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);
            var sw2   = new Weapon("w2", "Axe",   2f, P, attackBonus: 10);

            eq.Equip(sw1);
            var displaced = eq.Equip(sw2);

            Assert.Same(sw1, displaced);
            Assert.Same(sw2, eq.GetItem(EquipmentSlot.Weapon));
        }

        [Fact]
        public void UnequipWeapon_Reduces_TotalAttackPower_Back_To_Base()
        {
            var player = new Player(1, "Hero", P, attackPoints: 10);
            var eq     = new PlayerEquipment();
            var sword  = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);

            eq.Equip(sword);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());
            Assert.Equal(15, player.TotalAttackPower);

            eq.Unequip(EquipmentSlot.Weapon);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            Assert.Equal(10, player.TotalAttackPower);
        }

        [Fact]
        public void EquipArmor_Increases_TotalDefense()
        {
            var player = new Player(1, "Hero", P);
            var eq     = new PlayerEquipment();
            var vest   = new Armor("a1", "Leather Vest", 2f, P, defenseBonus: 3);

            eq.Equip(vest);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            Assert.Equal(3, player.TotalDefense);
        }

        [Fact]
        public void UnequipArmor_Reduces_TotalDefense()
        {
            var player = new Player(1, "Hero", P);
            var eq     = new PlayerEquipment();
            var vest   = new Armor("a1", "Leather Vest", 2f, P, defenseBonus: 3);

            eq.Equip(vest);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());
            Assert.Equal(3, player.TotalDefense);

            eq.Unequip(EquipmentSlot.Chest);
            player.ApplyEquipmentBonuses(eq.GetAttackBonus(), eq.GetDefenseBonus());

            Assert.Equal(0, player.TotalDefense);
        }

        [Fact]
        public void GetEquippedItem_Returns_Null_When_Slot_Empty()
        {
            var eq = new PlayerEquipment();
            Assert.Null(eq.GetItem(EquipmentSlot.Weapon));
        }

        [Fact]
        public void GetEquippedItem_Returns_Item_After_Equip()
        {
            var eq    = new PlayerEquipment();
            var sword = new Weapon("w1", "Sword", 1f, P, attackBonus: 5);
            eq.Equip(sword);
            Assert.Same(sword, eq.GetItem(EquipmentSlot.Weapon));
        }

        [Fact]
        public void Equip_Non_Equippable_Item_Returns_Null_And_Does_Nothing()
        {
            var eq     = new PlayerEquipment();
            var potion = new HealingPotion("p1", P); // Slot == null
            var result = eq.Equip(potion);
            Assert.Null(result);
            Assert.Null(eq.GetItem(EquipmentSlot.Weapon));
        }
    }
}

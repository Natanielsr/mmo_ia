using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;

namespace GameServer.Tests.Items
{
    public class WeaponArmorTests
    {
        private static readonly Position Pos = new(0, 0);

        // ── Weapon ────────────────────────────────────────────────────────────

        [Fact]
        public void Weapon_Should_Have_ItemType_Weapon()
        {
            var weapon = new Weapon("w1", "Iron Sword", 1.5f, "iron-sword", Pos, attackBonus: 5);
            Assert.Equal(ItemType.Weapon, weapon.Type);
        }

        [Fact]
        public void Weapon_Should_Expose_AttackBonus()
        {
            var weapon = new Weapon("w1", "Iron Sword", 1.5f, "iron-sword", Pos, attackBonus: 8);
            Assert.Equal(8, weapon.AttackBonus);
        }

        [Fact]
        public void Weapon_With_Zero_AttackBonus_Is_Valid()
        {
            var weapon = new Weapon("w1", "Wooden Stick", 0.5f, "wooden-stick", Pos, attackBonus: 0);
            Assert.Equal(0, weapon.AttackBonus);
            Assert.Equal(ItemType.Weapon, weapon.Type);
        }

        [Fact]
        public void Weapon_Inherits_Id_Name_Weight_From_Item()
        {
            var weapon = new Weapon("w99", "Steel Axe", 3.0f, "steel-axe", Pos, attackBonus: 12);
            Assert.Equal("w99", weapon.Id);
            Assert.Equal("Steel Axe", weapon.Name);
            Assert.Equal(3.0f, weapon.Weight, precision: 3);
        }

        // ── Armor ─────────────────────────────────────────────────────────────

        [Fact]
        public void Armor_Should_Have_ItemType_Armor()
        {
            var armor = new Armor("a1", "Leather Vest", 2.0f, "leather-vest", Pos, defenseBonus: 3);
            Assert.Equal(ItemType.Armor, armor.Type);
        }

        [Fact]
        public void Armor_Should_Expose_DefenseBonus()
        {
            var armor = new Armor("a1", "Iron Helmet", 1.8f, "iron-helmet", Pos, defenseBonus: 5);
            Assert.Equal(5, armor.DefenseBonus);
        }

        [Fact]
        public void Armor_With_Zero_DefenseBonus_Is_Valid()
        {
            var armor = new Armor("a1", "Cloth Robe", 0.3f, "cloth-robe", Pos, defenseBonus: 0);
            Assert.Equal(0, armor.DefenseBonus);
            Assert.Equal(ItemType.Armor, armor.Type);
        }

        [Fact]
        public void Armor_Inherits_Id_Name_Weight_From_Item()
        {
            var armor = new Armor("a99", "Full Plate", 10f, "full-plate", Pos, defenseBonus: 20);
            Assert.Equal("a99", armor.Id);
            Assert.Equal("Full Plate", armor.Name);
            Assert.Equal(10f, armor.Weight, precision: 3);
        }

        // ── ItemData DTO ──────────────────────────────────────────────────────

        [Fact]
        public void ItemData_Accepts_AttackBonus_For_Weapon()
        {
            var data = new GameServerApp.Dtos.ItemData
            {
                Id = "w1",
                Name = "Sword",
                Position = Pos,
                Type = ItemType.Weapon,
                AttackBonus = 7
            };
            Assert.Equal(7, data.AttackBonus);
        }

        [Fact]
        public void ItemData_Accepts_DefenseBonus_For_Armor()
        {
            var data = new GameServerApp.Dtos.ItemData
            {
                Id = "a1",
                Name = "Shield",
                Position = Pos,
                Type = ItemType.Armor,
                DefenseBonus = 4
            };
            Assert.Equal(4, data.DefenseBonus);
        }

        [Fact]
        public void ItemData_AttackBonus_Is_Null_By_Default()
        {
            var data = new GameServerApp.Dtos.ItemData
            {
                Id = "p1",
                Name = "Potion",
                Position = Pos,
                Type = ItemType.Potion
            };
            Assert.Null(data.AttackBonus);
            Assert.Null(data.DefenseBonus);
        }
    }
}

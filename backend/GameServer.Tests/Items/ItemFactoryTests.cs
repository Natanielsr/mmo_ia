using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Services;
using GameServerApp.World;

namespace GameServer.Tests.Items
{
    public class ItemFactoryTests
    {
        private readonly ItemFactory _sut = new();
        private static readonly Position Pos = new(3, 7);

        private static ItemDefinition MakeDef(string type, string tagName = "test-item",
            int? attackBonus = null, int? defenseBonus = null, int? healAmount = null)
            => new()
            {
                Id = 1,
                TagName = tagName,
                Name = "Test Item",
                Description = "Desc",
                Value = 5,
                Type = type,
                Weight = 1.0f,
                AttackBonus = attackBonus,
                DefenseBonus = defenseBonus,
                HealAmount = healAmount,
            };

        [Fact]
        public void Create_Weapon_Returns_Weapon_Instance()
        {
            var def = MakeDef("Weapon", "dagger", attackBonus: 5);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Weapon>(item);
            Assert.Equal(ItemType.Weapon, item.Type);
            Assert.Equal(5, ((Weapon)item).AttackBonus);
        }

        [Fact]
        public void Create_Armor_Returns_Armor_With_Chest_Slot()
        {
            var def = MakeDef("Armor", "leather-vest", defenseBonus: 3);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Armor>(item);
            Assert.Equal(ItemType.Armor, item.Type);
            Assert.Equal(EquipmentSlot.Chest, item.Slot);
            Assert.Equal(3, ((Armor)item).DefenseBonus);
        }

        [Fact]
        public void Create_Helmet_Returns_Helmet_Instance()
        {
            var def = MakeDef("Helmet", "iron-helmet", defenseBonus: 1);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Helmet>(item);
            Assert.Equal(EquipmentSlot.Helmet, item.Slot);
        }

        [Fact]
        public void Create_Shield_Returns_Shield_Instance()
        {
            var def = MakeDef("Shield", "wooden-shield", defenseBonus: 1);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Shield>(item);
            Assert.Equal(EquipmentSlot.Shield, item.Slot);
        }

        [Fact]
        public void Create_Legs_Returns_Legs_Instance()
        {
            var def = MakeDef("Legs", "leather-pants", defenseBonus: 1);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Legs>(item);
            Assert.Equal(EquipmentSlot.Legs, item.Slot);
        }

        [Fact]
        public void Create_Boots_Returns_Boots_Instance()
        {
            var def = MakeDef("Boots", "iron-boots", defenseBonus: 1);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<Boots>(item);
            Assert.Equal(EquipmentSlot.Boots, item.Slot);
        }

        [Fact]
        public void Create_Potion_Returns_HealingItem_Instance()
        {
            var def = MakeDef("Potion", "potion", healAmount: 20);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<HealingItem>(item);
            Assert.Equal(ItemType.Potion, item.Type);
            Assert.Equal(20, ((HealingItem)item).HealAmount);
        }

        [Fact]
        public void Create_Populates_Description_Value_TagName()
        {
            var def = MakeDef("Weapon", "dagger", attackBonus: 5);
            var item = _sut.Create(def, "id1", Pos);

            Assert.Equal("Desc", item.Description);
            Assert.Equal(5, item.Value);
            Assert.Equal("dagger", item.TagName);
        }

        [Fact]
        public void Create_Sets_InstanceId_And_Position()
        {
            var def = MakeDef("Weapon", attackBonus: 3);
            var item = _sut.Create(def, "my-instance-id", Pos);

            Assert.Equal("my-instance-id", item.Id);
            Assert.Equal(Pos, item.Position);
        }

        [Fact]
        public void Create_Throws_On_Unknown_Type()
        {
            var def = MakeDef("UnknownType");
            Assert.Throws<InvalidOperationException>(() => _sut.Create(def, "id1", Pos));
        }

        [Fact]
        public void Create_Food_Returns_HealingItem_Instance()
        {
            var def = MakeDef("Food", "cheese", healAmount: 10);
            var item = _sut.Create(def, "id1", Pos);

            Assert.IsType<HealingItem>(item);
            Assert.Equal(ItemType.Food, item.Type);
        }

        [Fact]
        public void Create_Food_Sets_HealAmount()
        {
            var def = MakeDef("Food", "cheese", healAmount: 10);
            var item = _sut.Create(def, "id1", Pos);

            Assert.Equal(10, ((HealingItem)item).HealAmount);
        }

        [Fact]
        public void Create_Food_RawMeat_HealAmount_Is_HealAmount()
        {
            var def = MakeDef("Food", "raw-meat", healAmount: 20);
            var item = _sut.Create(def, "id1", Pos);

            Assert.Equal(20, ((HealingItem)item).HealAmount);
        }
    }
}

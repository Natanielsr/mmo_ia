using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.World;

namespace GameServer.Tests.Items
{
    public class LootTableServiceTests
    {
        private static readonly string ItemsJson = """
            {
              "items": [
                { "id": 1, "tagName": "potion",       "name": "Healing Potion", "description": "", "value": 10, "type": "Potion",  "weight": 0.5, "healAmount": 20 },
                { "id": 2, "tagName": "dagger",        "name": "Dagger",         "description": "", "value": 25, "type": "Weapon",  "weight": 1.5, "attackBonus": 5 },
                { "id": 3, "tagName": "leather-vest",  "name": "Leather Vest",   "description": "", "value": 20, "type": "Armor",   "weight": 2.0, "defenseBonus": 1 },
                { "id": 4, "tagName": "iron-helmet",   "name": "Iron Helmet",    "description": "", "value": 18, "type": "Helmet",  "weight": 1.2, "defenseBonus": 1 },
                { "id": 5, "tagName": "wooden-shield", "name": "Wooden Shield",  "description": "", "value": 15, "type": "Shield",  "weight": 2.5, "defenseBonus": 1 },
                { "id": 6, "tagName": "leather-pants", "name": "Leather Pants",  "description": "", "value": 12, "type": "Legs",    "weight": 1.0, "defenseBonus": 1 },
                { "id": 7, "tagName": "iron-boots",    "name": "Iron Boots",     "description": "", "value": 14, "type": "Boots",   "weight": 1.5, "defenseBonus": 1 }
              ]
            }
            """;

        private readonly ILootTableService _sut =
            new LootTableService(new ItemDefinitionRepository(ItemsJson), new ItemFactory());

        private static readonly Position Pos = new(5, 5);

        [Fact]
        public void RollLoot_Returns_Null_When_Rng_Above_All_Thresholds()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 1.0);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_HealingPotion_When_Rng_In_Potion_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.15);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
            Assert.IsType<HealingPotion>(result);
        }

        [Fact]
        public void RollLoot_Returns_Weapon_When_Rng_In_Weapon_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.30);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Weapon, result.Type);
            Assert.IsType<Weapon>(result);
        }

        [Fact]
        public void RollLoot_Returns_Armor_When_Rng_In_Armor_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.40);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Armor, result.Type);
            Assert.IsType<Armor>(result);
        }

        [Fact]
        public void RollLoot_Returns_Helmet_When_Rng_In_Helmet_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.50);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Helmet, result.Type);
            Assert.IsType<Helmet>(result);
        }

        [Fact]
        public void RollLoot_Returns_Shield_When_Rng_In_Shield_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.60);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Shield, result.Type);
            Assert.IsType<Shield>(result);
        }

        [Fact]
        public void RollLoot_Returns_Legs_When_Rng_In_Legs_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.75);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Legs, result.Type);
            Assert.IsType<Legs>(result);
        }

        [Fact]
        public void RollLoot_Returns_Boots_When_Rng_In_Boots_Range()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.85);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Boots, result.Type);
            Assert.IsType<Boots>(result);
        }

        [Fact]
        public void RollLoot_Uses_Provided_Rng_For_Determinism()
        {
            int callCount = 0;
            _sut.RollLoot(Pos, "id1", rng: () => { callCount++; return 0.5; });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void RollLoot_Never_Returns_Item_When_Rng_Above_093()
        {
            for (int i = 0; i < 50; i++)
            {
                var result = _sut.RollLoot(Pos, $"id{i}", rng: () => 0.99);
                Assert.Null(result);
            }
        }

        [Fact]
        public void RollLoot_Returns_Potion_When_Rng_Is_Zero()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.0);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
        }

        [Fact]
        public void RollLoot_Assigns_InstanceId_And_Position()
        {
            var pos = new Position(3, 7);
            var result = _sut.RollLoot(pos, "myId", rng: () => 0.15);

            Assert.NotNull(result);
            Assert.Equal("myId", result.Id);
            Assert.Equal(pos, result.Position);
        }

        [Fact]
        public void RollLoot_Item_Has_TagName_From_Catalog()
        {
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.30);

            Assert.NotNull(result);
            Assert.Equal("dagger", result.TagName);
        }

        [Fact]
        public void RollLoot_Without_Custom_Rng_Does_Not_Throw()
        {
            var ex = Record.Exception(() => _sut.RollLoot(Pos, "id1"));
            Assert.Null(ex);
        }
    }
}

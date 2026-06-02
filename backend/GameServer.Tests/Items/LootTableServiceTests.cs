using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.World;
using Moq;

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

        // Tabela de teste: rat → potion(60) + dagger(10) + nothing(30) = totalWeight 100
        // potion: 60/100 = 60%, dagger: 10/100 = 10%, nothing: 30/100 = 30% no drop
        private static readonly string LootJson = """
            {
              "lootTables": {
                "rat": [
                  { "itemTag": "potion", "weight": 60 },
                  { "itemTag": "dagger", "weight": 10 },
                  { "itemTag": "nothing", "weight": 30 }
                ]
              }
            }
            """;

        private readonly ILootTableService _sut = new LootTableService(
            new LootTableRepository(LootJson),
            new ItemDefinitionRepository(ItemsJson),
            new ItemFactory());

        private static readonly Position Pos = new(5, 5);

        // roll * 100: [0, 60) → potion, [60, 70) → dagger, [70, 100) → nothing (no drop)

        [Fact]
        public void RollLoot_Returns_Null_For_Unknown_Monster()
        {
            var result = _sut.RollLoot(Pos, "id1", "unknown-monster", rng: () => 0.0);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_Null_When_Roll_Hits_Nothing_Entry()
        {
            // roll = 0.75 * 100 = 75, cai no "nothing" [70, 100)
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.75);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_Potion_When_Roll_In_Potion_Range()
        {
            // roll * 70 = 0 < 60 → potion
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.0);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
            Assert.IsType<HealingPotion>(result);
        }

        [Fact]
        public void RollLoot_Returns_Dagger_When_Roll_In_Dagger_Range()
        {
            // roll = 0.65 * 100 = 65, cai em dagger [60, 70)
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.65);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Weapon, result.Type);
            Assert.IsType<Weapon>(result);
            Assert.Equal("dagger", result.TagName);
        }

        [Fact]
        public void RollLoot_Uses_Provided_Rng_Exactly_Once()
        {
            int callCount = 0;
            _sut.RollLoot(Pos, "id1", "rat", rng: () => { callCount++; return 0.5; });
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void RollLoot_Assigns_InstanceId_And_Position()
        {
            var pos = new Position(3, 7);
            var result = _sut.RollLoot(pos, "myId", "rat", rng: () => 0.0);

            Assert.NotNull(result);
            Assert.Equal("myId", result.Id);
            Assert.Equal(pos, result.Position);
        }

        [Fact]
        public void RollLoot_Item_Has_TagName_From_Catalog()
        {
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.0);

            Assert.NotNull(result);
            Assert.Equal("potion", result.TagName);
        }

        [Fact]
        public void RollLoot_Without_Custom_Rng_Does_Not_Throw()
        {
            var ex = Record.Exception(() => _sut.RollLoot(Pos, "id1", "rat"));
            Assert.Null(ex);
        }

        [Fact]
        public void RollLoot_Single_Entry_Returns_Item_When_Not_Nothing()
        {
            // Monta repo com apenas um potion para o monstro
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat"))
                    .Returns([new LootEntry("potion", 100)]);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            var result = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.0);
            Assert.NotNull(result);
        }

        [Fact]
        public void RollLoot_Returns_Null_When_Single_Entry_Is_Nothing()
        {
            // Apenas "nothing" — sempre sem drop
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat"))
                    .Returns([new LootEntry("nothing", 100)]);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            var result = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.5);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_Null_When_Monster_Has_No_Entries()
        {
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat")).Returns((IReadOnlyList<LootEntry>?)null);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            var result = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.0);
            Assert.Null(result);
        }
    }
}

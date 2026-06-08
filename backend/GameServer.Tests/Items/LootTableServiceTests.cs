using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services.Combat;
using GameServerApp.Services.Items;
using GameServerApp.Services.Movement;
using GameServerApp.Services.Ranking;
using GameServerApp.Services.Repositories;
using GameServerApp.Services.World;
using GameServerApp.Services.World.WorldFormations;
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

        // Tabela de teste: rat → potion(60) + dagger(10); totalWeight=70
        // Weight é porcentagem: 70% chance de dropar algo, 30% de não dropar
        // Se dropar: potion=60/70, dagger=10/70
        private static readonly string LootJson = """
            {
              "lootTables": {
                "rat": [
                  { "itemTag": "potion", "weight": 60 },
                  { "itemTag": "dagger", "weight": 10 }
                ]
              }
            }
            """;

        private readonly ILootTableService _sut = new LootTableService(
            new LootTableRepository(LootJson),
            new ItemDefinitionRepository(ItemsJson),
            new ItemFactory());

        private static readonly Position Pos = new(5, 5);

        // roll = random() * 100; if > 70, no drop; else select item by accumulated weight

        [Fact]
        public void RollLoot_Returns_Null_For_Unknown_Monster()
        {
            var result = _sut.RollLoot(Pos, "id1", "unknown-monster", rng: () => 0.0);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_Null_When_Roll_Exceeds_Total_Weight()
        {
            // totalWeight = 70; roll = 0.75 * 100 = 75; 75 > 70 → no drop
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.75);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_Potion_When_Roll_In_Potion_Range()
        {
            // roll = 0.0 * 100 = 0; 0 < 60 → potion
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.0);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
            Assert.IsType<HealingItem>(result);
        }

        [Fact]
        public void RollLoot_Returns_Dagger_When_Roll_In_Dagger_Range()
        {
            // roll = 0.65 * 100 = 65; 60 <= 65 < 70 → dagger
            var result = _sut.RollLoot(Pos, "id1", "rat", rng: () => 0.65);

            Assert.NotNull(result);
            Assert.Equal(ItemType.Weapon, result.Type);
            Assert.IsType<Weapon>(result);
            Assert.Equal("dagger", result.TagName);
        }

        [Fact]
        public void RollLoot_Uses_Provided_Rng_Once()
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
        public void RollLoot_Single_Entry_Returns_Item_When_Roll_Succeeds()
        {
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
        public void RollLoot_Returns_Null_When_Roll_Exceeds_Single_Entry_Weight()
        {
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat"))
                    .Returns([new LootEntry("potion", 10)]);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            // totalWeight=10; roll=50*100=50; 50 > 10 → no drop
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

        [Fact]
        public void RollLoot_Respects_Weight_As_Percentage_Drop_Chance()
        {
            // Setup: potion weight=5, dagger weight=10; totalWeight=15
            // Expected: 5% potion drop, 10% dagger drop, 85% no drop
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat"))
                    .Returns([
                        new LootEntry("potion", 5),
                        new LootEntry("dagger", 10)
                    ]);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            // No drop should occur when roll > totalWeight (15)
            var noDropRoll = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.20); // 20 > 15
            Assert.Null(noDropRoll);

            // Potion: roll in [0, 5)
            var potionRoll = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.04); // 4 < 5
            Assert.NotNull(potionRoll);
            Assert.Equal("potion", potionRoll.TagName);

            // Dagger: roll in [5, 15)
            var daggerRoll = sut.RollLoot(Pos, "id1", "rat", rng: () => 0.12); // 12 in [5, 15)
            Assert.NotNull(daggerRoll);
            Assert.Equal("dagger", daggerRoll.TagName);
        }

        [Fact]
        public void RollLoot_Distribution_Matches_Weight_Percentages()
        {
            // Statistical test: weight 25 & 75 should produce ~25% & ~75% distribution
            var lootRepo = new Mock<ILootTableRepository>();
            lootRepo.Setup(r => r.GetEntriesFor("rat"))
                    .Returns([
                        new LootEntry("potion", 25),
                        new LootEntry("dagger", 75)
                    ]);

            var sut = new LootTableService(
                lootRepo.Object,
                new ItemDefinitionRepository(ItemsJson),
                new ItemFactory());

            int potionCount = 0;
            int daggerCount = 0;
            int noDropCount = 0;
            int iterations = 10000;

            for (int i = 0; i < iterations; i++)
            {
                double randomValue = i / (double)iterations; // Uniform distribution [0, 1)
                var result = sut.RollLoot(Pos, $"id{i}", "rat", rng: () => randomValue);

                if (result == null)
                    noDropCount++;
                else if (result.TagName == "potion")
                    potionCount++;
                else if (result.TagName == "dagger")
                    daggerCount++;
            }

            // Expected: potion 25%, dagger 75%, no drop 0% (totalWeight=100)
            double potionPercent = potionCount / (double)iterations * 100;
            double daggerPercent = daggerCount / (double)iterations * 100;
            double noDropPercent = noDropCount / (double)iterations * 100;

            // Allow ±5% tolerance for distribution test
            Assert.InRange(potionPercent, 20, 30);
            Assert.InRange(daggerPercent, 70, 80);
            Assert.InRange(noDropPercent, 0, 5);
        }
    }
}

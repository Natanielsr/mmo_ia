using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.World;

namespace GameServer.Tests.Items
{
    public class LootTableServiceTests
    {
        private readonly ILootTableService _sut = new LootTableService();
        private static readonly Position Pos = new(5, 5);

        [Fact]
        public void RollLoot_Returns_Null_When_Rng_Above_All_Thresholds()
        {
            // rng always returns 1.0 → nenhum item dropa
            var result = _sut.RollLoot(Pos, "id1", rng: () => 1.0);
            Assert.Null(result);
        }

        [Fact]
        public void RollLoot_Returns_HealingPotion_When_Rng_In_Potion_Range()
        {
            // Poção: 0–0.25 (25%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.15);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
            Assert.IsType<HealingPotion>(result);
        }

        [Fact]
        public void RollLoot_Returns_Weapon_When_Rng_In_Weapon_Range()
        {
            // Arma: 0.25–0.35 (10%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.30);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Weapon, result.Type);
            Assert.IsType<Weapon>(result);
        }

        [Fact]
        public void RollLoot_Returns_Armor_When_Rng_In_Armor_Range()
        {
            // Armadura: 0.35–0.45 (10%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.40);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Armor, result.Type);
            Assert.IsType<Armor>(result);
        }

        [Fact]
        public void RollLoot_Returns_Helmet_When_Rng_In_Helmet_Range()
        {
            // Helmet: 0.45–0.57 (12%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.50);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Helmet, result.Type);
            Assert.IsType<Helmet>(result);
        }

        [Fact]
        public void RollLoot_Returns_Shield_When_Rng_In_Shield_Range()
        {
            // Shield: 0.57–0.69 (12%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.60);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Shield, result.Type);
            Assert.IsType<Shield>(result);
        }

        [Fact]
        public void RollLoot_Returns_Legs_When_Rng_In_Legs_Range()
        {
            // Legs: 0.69–0.81 (12%)
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.75);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Legs, result.Type);
            Assert.IsType<Legs>(result);
        }

        [Fact]
        public void RollLoot_Returns_Boots_When_Rng_In_Boots_Range()
        {
            // Boots: 0.81–0.93 (12%)
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
        public void RollLoot_Never_Returns_Item_When_Chance_Is_Zero()
        {
            // Força rng > 0.93 → fora de todos os ranges
            for (int i = 0; i < 50; i++)
            {
                var result = _sut.RollLoot(Pos, $"id{i}", rng: () => 0.99);
                Assert.Null(result);
            }
        }

        [Fact]
        public void RollLoot_Returns_Potion_When_Rng_Is_Zero()
        {
            // 0.0 está no range da poção
            var result = _sut.RollLoot(Pos, "id1", rng: () => 0.0);
            Assert.NotNull(result);
            Assert.Equal(ItemType.Potion, result.Type);
        }

        [Fact]
        public void RollLoot_Assigns_Id_And_Position_To_Dropped_Item()
        {
            var pos = new Position(3, 7);
            var result = _sut.RollLoot(pos, "myId", rng: () => 0.15);
            Assert.NotNull(result);
            Assert.Equal("myId", result.Id);
            Assert.Equal(pos, result.Position);
        }

        [Fact]
        public void RollLoot_Without_Custom_Rng_Does_Not_Throw()
        {
            // Usa Random.Shared internamente — não deve lançar
            var ex = Record.Exception(() => _sut.RollLoot(Pos, "id1"));
            Assert.Null(ex);
        }
    }
}

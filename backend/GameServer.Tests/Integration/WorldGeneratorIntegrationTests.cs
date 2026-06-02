using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Services;
using GameServerApp.Contracts.Config;
using GameServerApp.World;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;

namespace GameServer.Tests.Integration
{
    public class WorldGeneratorIntegrationTests
    {
        private readonly Mock<IStaticWorldManager> _mockStaticWorldManager = new();
        private readonly Mock<IIdGeneratorService> _mockIdGenerator = new();
        private readonly Mock<IItemManager> _mockItemManager = new();
        private readonly Mock<IWorldEvents> _mockEvents = new();
        private readonly Mock<IItemDefinitionRepository> _mockItemRepo = new();
        private readonly Mock<IItemFactory> _mockItemFactory = new();
        private readonly Mock<IBiomeSelector> _mockBiomeSelector = new();
        private readonly IOptions<WorldConfig> _config = Options.Create(new WorldConfig());

        public WorldGeneratorIntegrationTests()
        {
            _mockIdGenerator.Setup(g => g.GenerateId()).Returns(1001);

            var potionDef = new ItemDefinition
            {
                Id = 1, TagName = "potion", Name = "Healing Potion",
                Type = "Potion", Weight = 0.5f, HealAmount = 20,
                Description = "Restaura 20 de HP", Value = 10
            };
            _mockItemRepo.Setup(r => r.GetByTagName("potion")).Returns(potionDef);
            _mockItemFactory.Setup(f => f.Create(potionDef, It.IsAny<string>(), It.IsAny<Position>()))
                .Returns((ItemDefinition def, string id, Position pos) =>
                    new HealingItem(id, def.Name, def.Weight, def.TagName, pos, ItemType.Potion));

            _mockBiomeSelector
                .Setup(b => b.GetBiomeForChunk(It.IsAny<ChunkCoord>()))
                .Returns(new BiomeDefinition
                {
                    Id = 1, TagName = "green_field", Name = "Green Field",
                    GroundTilePrefix = "grass", GroundTileCount = 12,
                    SemanticObjectMap = new() { ["default"] = "tree", ["tree"] = "tree" },
                    MonsterTags = new() { "rat" },
                    IsDefault = true
                });
        }

        [Fact]
        public void WorldGenerator_ShouldRegisterItemInManagerAndNotifyEvents_WhenPotionSpawnRequested()
        {
            var generator = new WorldGenerator(
                _mockStaticWorldManager.Object,
                _mockIdGenerator.Object,
                _mockItemManager.Object,
                _mockEvents.Object,
                _config,
                _mockItemRepo.Object,
                _mockItemFactory.Object,
                _mockBiomeSelector.Object);

            var method = typeof(WorldGenerator).GetMethod("SpawnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // SpawnObject(int x, int y, Random rng, string? forcedType, BiomeDefinition? biome)
            method!.Invoke(generator, new object?[] { 10, 10, new Random(), "item:potion", null });

            _mockItemManager.Verify(m => m.DropItem(It.Is<IItem>(i => i.Type == ItemType.Potion && i.Name == "Healing Potion")), Times.Once);
            _mockEvents.Verify(e => e.OnItemDropped(It.Is<IItem>(i => i.Type == ItemType.Potion)), Times.Once);
        }

        [Fact]
        public void WorldGenerator_SpawnObject_UsesBiomeSemanticMap_ForObjectCode()
        {
            var darkForestBiome = new BiomeDefinition
            {
                Id = 2, TagName = "dark_forest", Name = "Dark Forest",
                GroundTilePrefix = "darkgrass", GroundTileCount = 12,
                SemanticObjectMap = new() { ["default"] = "dark_tree", ["tree"] = "dark_tree", ["rock"] = "dark_rock" },
                MonsterTags = new() { "spider" },
                IsDefault = false
            };

            _mockBiomeSelector
                .Setup(b => b.GetBiomeForChunk(It.IsAny<ChunkCoord>()))
                .Returns(darkForestBiome);

            var generator = new WorldGenerator(
                _mockStaticWorldManager.Object,
                _mockIdGenerator.Object,
                _mockItemManager.Object,
                _mockEvents.Object,
                _config,
                _mockItemRepo.Object,
                _mockItemFactory.Object,
                _mockBiomeSelector.Object);

            var method = typeof(WorldGenerator).GetMethod("SpawnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            method!.Invoke(generator, new object?[] { 5, 5, new Random(), "tree", darkForestBiome });

            _mockStaticWorldManager.Verify(
                m => m.AddStaticObject(It.Is<IStaticWorldObject>(o => o.ObjectCode == "dark_tree")),
                Times.Once);
        }
    }
}

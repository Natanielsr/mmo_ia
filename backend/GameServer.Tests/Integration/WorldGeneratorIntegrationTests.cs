using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.Contracts.Config;
using GameServerApp.Dtos;
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
                    new HealingPotion(id, def.Name, def.Weight, def.TagName, pos));
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
                _mockItemFactory.Object);

            var method = typeof(WorldGenerator).GetMethod("SpawnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            method!.Invoke(generator, new object[] { 10, 10, new Random(), "item:potion" });

            _mockItemManager.Verify(m => m.DropItem(It.Is<IItem>(i => i.Type == ItemType.Potion && i.Name == "Healing Potion")), Times.Once);
            _mockEvents.Verify(e => e.OnItemDropped(It.Is<IItem>(i => i.Type == ItemType.Potion)), Times.Once);
        }
    }
}

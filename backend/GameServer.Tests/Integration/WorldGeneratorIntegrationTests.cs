using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Services;
using GameServerApp.Contracts.Config;
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
        private readonly IOptions<WorldConfig> _config = Options.Create(new WorldConfig());

        public WorldGeneratorIntegrationTests()
        {
            _mockIdGenerator.Setup(g => g.GenerateId()).Returns(1001);
        }

        [Fact]
        public void WorldGenerator_ShouldRegisterItemInManagerAndNotifyEvents_WhenPotionSpawnRequested()
        {
            // Arrange
            var generator = new WorldGenerator(
                _mockStaticWorldManager.Object,
                _mockIdGenerator.Object,
                _mockItemManager.Object,
                _mockEvents.Object,
                _config);

            // Accessing the private SpawnObject via Reflection or just testing a formation that uses it.
            // But WorldGenerator is IWorldGenerator, so we can test GenerateChunk.
            // To ensure a specific formation (like StoneCircle) spawns a potion, we might need multiple tries or a specific seed.
            
            // For testing purposes, we can directly call the private SpawnObject method if we make it internal/protected, 
            // but let's try a more black-box approach first by using a seed that we know triggers a potion spawn.
            // Or better, we can test GenerateChunk with a specific mock setup.
            
            // Let's use a seed that triggers StoneCircleFormation (Weight 0.04)
            // Weight distribution: Organic(0.85), StoneCircle(0.04), Row(0.04), Cluster(0.04), Maze(0.03)
            // Roll range for StoneCircle: [0.85, 0.89)
            
            // Find a seed:
            var coord = new ChunkCoord(0, 0);
            int seed = HashCode.Combine(coord.CX, coord.CY);
            var rng = new Random(seed);
            double roll = rng.NextDouble();
            // We might need to iterate or force the formation in the test.
            
            // Alternative: Test the SpawnObject logic directly if I can.
            // Since it's private, I'll use Reflection for this specific integration test to avoid seed hunting.
            var method = typeof(WorldGenerator).GetMethod("SpawnObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            method.Invoke(generator, new object[] { 10, 10, new Random(), "item:healing_potion" });

            // Assert
            _mockItemManager.Verify(m => m.DropItem(It.Is<IItem>(i => i.Type == ItemType.Potion && i.Name == "Healing Potion")), Times.Once);
            _mockEvents.Verify(e => e.OnItemDropped(It.Is<IItem>(i => i.Type == ItemType.Potion)), Times.Once);
        }
    }
}

using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using GameServerApp.Processors;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using Moq;

namespace GameServer.Tests.Processors
{
    public class ChunkProcessorTests
    {
        private readonly Mock<IStaticWorldManager> _staticWorldManager = new();
        private readonly Mock<IChunkObjectGenerator> _worldGenerator = new();
        private readonly Mock<IItemManager> _itemManager = new();
        private readonly Mock<IWorldEvents> _worldEvents = new();
        private readonly Mock<IWorldTerrainService> _fractalWorld = new();
        private readonly IOptions<WorldConfig> _config;
        private readonly ChunkProcessor _sut;

        public ChunkProcessorTests()
        {
            _config = Options.Create(new WorldConfig
            {
                Map = new MapConfig { Width = 32, Height = 32, ChunkSize = 16, LoadChunks = 9 }
            });

            _staticWorldManager.Setup(m => m.IsChunkLoaded(It.IsAny<ChunkCoord>())).Returns(true);
            _staticWorldManager.Setup(m => m.GetChunkObjects(It.IsAny<ChunkCoord>()))
                .Returns(Enumerable.Empty<IStaticWorldObject>());
            _itemManager.Setup(m => m.GetItemsInChunk(It.IsAny<ChunkCoord>()))
                .Returns(Array.Empty<IItem>());
            _fractalWorld.Setup(m => m.GetChunkTiles(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((new int[0][], new int[0][]));

            _sut = new ChunkProcessor(
                _staticWorldManager.Object,
                _worldGenerator.Object,
                _itemManager.Object,
                _worldEvents.Object,
                _fractalWorld.Object,
                _config);
        }

        private static Player MakePlayer() =>
            new Player(1, "Hero", new Position(0, 0));

        [Fact]
        public void ProcessChunkRequest_Should_EmitChunkLoaded_ForEachRequestedCoord()
        {
            var player = MakePlayer();
            var coords = new List<ChunkCoordDto>
            {
                new(0, 0),
                new(1, 0),
                new(0, 1),
            };

            _sut.ProcessChunkRequest(player, "conn-1", coords);

            _worldEvents.Verify(e => e.OnChunkLoaded("conn-1", It.IsAny<ChunkData>()), Times.Exactly(3));
        }

        [Fact]
        public void ProcessChunkRequest_Should_Skip_OutOfBounds_Coords()
        {
            var player = MakePlayer();
            var coords = new List<ChunkCoordDto>
            {
                new(5, 5),  // 5 * 16 = 80 >= 32 → fora do mapa
                new(-1, 0), // negativo → fora do mapa
            };

            _sut.ProcessChunkRequest(player, "conn-1", coords);

            _worldEvents.Verify(e => e.OnChunkLoaded(It.IsAny<string>(), It.IsAny<ChunkData>()), Times.Never);
        }

        [Fact]
        public void ProcessChunkRequest_Should_Send_EvenIfAlreadyInSentChunks()
        {
            var player = MakePlayer();
            player.SentChunks.Add(new ChunkCoord(0, 0));

            var coords = new List<ChunkCoordDto> { new(0, 0) };

            _sut.ProcessChunkRequest(player, "conn-1", coords);

            _worldEvents.Verify(e => e.OnChunkLoaded("conn-1", It.IsAny<ChunkData>()), Times.Once);
        }

        [Fact]
        public void ProcessChunkRequest_Should_Not_Modify_SentChunks()
        {
            var player = MakePlayer();
            var coords = new List<ChunkCoordDto> { new(0, 0) };

            _sut.ProcessChunkRequest(player, "conn-1", coords);

            Assert.Empty(player.SentChunks);
        }
    }
}

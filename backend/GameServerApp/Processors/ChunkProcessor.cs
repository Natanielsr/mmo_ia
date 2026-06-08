using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Processors;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;
using Microsoft.Extensions.Options;

namespace GameServerApp.Processors
{
    public class ChunkProcessor : IChunkProcessor
    {
        private readonly IStaticWorldManager _staticWorldManager;
        private readonly IChunkObjectGenerator _worldGenerator;
        private readonly IItemManager _itemManager;
        private readonly IWorldEvents _worldEvents;
        private readonly IWorldTerrainService _fractalWorld;
        private readonly WorldConfig _config;

        public ChunkProcessor(
            IStaticWorldManager staticWorldManager,
            IChunkObjectGenerator worldGenerator,
            IItemManager itemManager,
            IWorldEvents worldEvents,
            IWorldTerrainService fractalWorld,
            IOptions<WorldConfig> config)
        {
            _staticWorldManager = staticWorldManager;
            _worldGenerator = worldGenerator;
            _itemManager = itemManager;
            _worldEvents = worldEvents;
            _fractalWorld = fractalWorld;
            _config = config.Value;
        }

        public void ProcessChunkLoading(IPlayer player, string connectionId)
        {
            int cx = (int)Math.Floor((double)player.Position.X / _config.Map.ChunkSize);
            int cy = (int)Math.Floor((double)player.Position.Y / _config.Map.ChunkSize);

            int side = (int)Math.Floor(Math.Sqrt(_config.Map.LoadChunks));
            int half = side / 2;

            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    var coord = new ChunkCoord(cx + dx, cy + dy);

                    if (!IsChunkInsideMap(coord))
                        continue;

                    if (player.SentChunks.Contains(coord))
                        continue;

                    _worldEvents.OnChunkLoaded(connectionId, BuildChunkData(coord));
                    player.SentChunks.Add(coord);
                }
            }
        }

        public void ProcessChunkRequest(IPlayer player, string connectionId, IEnumerable<ChunkCoordDto> coords)
        {
            foreach (var dto in coords)
            {
                var coord = new ChunkCoord(dto.Cx, dto.Cy);

                if (!IsChunkInsideMap(coord))
                    continue;

                _worldEvents.OnChunkLoaded(connectionId, BuildChunkData(coord));
            }
        }

        private ChunkData BuildChunkData(ChunkCoord coord)
        {
            if (!_staticWorldManager.IsChunkLoaded(coord))
                _worldGenerator.GenerateChunk(coord);

            var chunkObjects = _staticWorldManager.GetChunkObjects(coord);
            var chunkItems   = _itemManager.GetItemsInChunk(coord);
            var (tiles, biomes) = _fractalWorld.GetChunkTiles(coord.CX, coord.CY, _config.Map.ChunkSize);

            return new ChunkData
            {
                Cx = coord.CX,
                Cy = coord.CY,
                Tiles = tiles,
                Biomes = biomes,
                Objects = chunkObjects.Select(obj => new MapObjectData
                {
                    Id         = obj.Id,
                    Name       = obj.Name,
                    ObjectCode = obj.ObjectCode,
                    Position   = obj.Position,
                    Type       = obj.Type,
                    IsPassable = obj.IsPassable
                }).ToList(),
                Items = chunkItems.Select(item => new ItemData
                {
                    Id          = item.Id,
                    Name        = item.Name,
                    Description = item.Description,
                    Value       = item.Value,
                    TagName     = item.TagName,
                    Position    = item.Position,
                    Type        = item.Type,
                    AttackBonus  = (item as GameServerApp.World.Weapon)?.AttackBonus,
                    DefenseBonus = (item as GameServerApp.Contracts.World.IDefensiveItem)?.DefenseBonus,
                    Quantity    = item.Quantity,
                }).ToList()
            };
        }

        /// <summary>True se o chunk sobrepõe os limites do mapa [0,Width) × [0,Height).</summary>
        private bool IsChunkInsideMap(ChunkCoord coord)
        {
            int size = _config.Map.ChunkSize;
            return coord.CX >= 0 && coord.CY >= 0
                && coord.CX * size < _config.Map.Width
                && coord.CY * size < _config.Map.Height;
        }
    }
}

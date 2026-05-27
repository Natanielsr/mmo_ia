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
        private readonly IWorldGenerator _worldGenerator;
        private readonly IItemManager _itemManager;
        private readonly IWorldEvents _worldEvents;
        private readonly WorldConfig _config;

        public ChunkProcessor(
            IStaticWorldManager staticWorldManager,
            IWorldGenerator worldGenerator,
            IItemManager itemManager,
            IWorldEvents worldEvents,
            IOptions<WorldConfig> config)
        {
            _staticWorldManager = staticWorldManager;
            _worldGenerator = worldGenerator;
            _itemManager = itemManager;
            _worldEvents = worldEvents;
            _config = config.Value;
        }

        public void ProcessChunkLoading(IPlayer player, string connectionId)
        {
            int cx = (int)Math.Floor((double)player.Position.X / _config.Map.ChunkSize);
            int cy = (int)Math.Floor((double)player.Position.Y / _config.Map.ChunkSize);

            for (int dx = -_config.Map.LoadRadius; dx <= _config.Map.LoadRadius; dx++)
            {
                for (int dy = -_config.Map.LoadRadius; dy <= _config.Map.LoadRadius; dy++)
                {
                    var coord = new ChunkCoord(cx + dx, cy + dy);

                    if (!_staticWorldManager.IsChunkLoaded(coord))
                        _worldGenerator.GenerateChunk(coord);

                    var chunkObjects = _staticWorldManager.GetChunkObjects(coord);
                    var chunkItems   = _itemManager.GetItemsInChunk(coord);

                    var chunkData = new ChunkData
                    {
                        CX = coord.CX,
                        CY = coord.CY,
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

                    _worldEvents.OnChunkLoaded(connectionId, chunkData);
                }
            }
        }
    }
}

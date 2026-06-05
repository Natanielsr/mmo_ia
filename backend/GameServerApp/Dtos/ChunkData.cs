using GameServerApp.Contracts.Types;
using System.Collections.Generic;

namespace GameServerApp.Dtos
{
    public class ChunkData
    {
        public int Cx { get; set; }
        public int Cy { get; set; }
        public List<MapObjectData> Objects { get; set; } = new();
        public List<ItemData> Items { get; set; } = new();
    }
}

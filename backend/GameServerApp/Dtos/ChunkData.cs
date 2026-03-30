using GameServerApp.Contracts.Types;
using System.Collections.Generic;

namespace GameServerApp.Dtos
{
    public class ChunkData
    {
        public int CX { get; set; }
        public int CY { get; set; }
        public List<MapObjectData> Objects { get; set; } = new();
    }
}

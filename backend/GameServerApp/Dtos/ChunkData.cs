using GameServerApp.Contracts.Types;
using System.Collections.Generic;

namespace GameServerApp.Dtos
{
    public class ChunkData
    {
        public int CX { get; set; }
        public int CY { get; set; }
        public string BiomeTag { get; set; } = "green_field";
        public List<string> TileBiomeTags { get; set; } = new();
        public List<MapObjectData> Objects { get; set; } = new();
        public List<ItemData> Items { get; set; } = new();
    }
}

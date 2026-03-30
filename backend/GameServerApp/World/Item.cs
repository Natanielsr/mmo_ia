using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class Item : IItem
    {
        public string Id { get; }
        public string Name { get; }
        public Position Position { get; set; }
        public float Weight { get; }
        public ItemType Type { get; }

        public Item(string id, string name, float weight, Position position = null!, ItemType type = ItemType.Generic)
        {
            Id = id;
            Name = name;
            Weight = weight;
            Position = position ?? new Position(0, 0);
            Type = type;
        }
    }
}

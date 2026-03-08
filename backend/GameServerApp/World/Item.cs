using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class Item : IItem
    {
        public string Id { get; }
        public string Name { get; }
        public Position Position { get; set; }
        public float Weight { get; }

        public Item(string id, string name, float weight, Position position = null!)
        {
            Id = id;
            Name = name;
            Weight = weight;
            Position = position ?? new Position(0, 0);
        }
    }
}

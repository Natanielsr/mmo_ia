using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class WorldObject : IWorldObject
    {
        public string Id { get; }
        public ObjectType Type { get; }
        public bool IsPassable { get; }
        public Position Position { get; set; }
        public Size Size { get; }
        public float Rotation { get; }

        public WorldObject(string id, ObjectType type, Position position, bool isPassable = false, float rotation = 0)
        {
            Id = id;
            Type = type;
            Position = position;
            IsPassable = isPassable;
            Rotation = rotation;
            Size = new Size { Width = 1, Height = 1 }; // Default tile size
        }
    }
}

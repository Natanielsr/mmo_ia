using System;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class WorldObject : IWorldObject
    {
        public long Id { get; }
        public string Name { get; }
        public string ObjectCode { get; }
        public ObjectType Type { get; }
        public bool IsPassable { get; }
        public Position Position { get; set; }
        public Size Size { get; }
        public float Rotation { get; }

        public WorldObject(long id, string name, ObjectType type, Position position, bool isPassable = false, float rotation = 0)
        {
            Id = id;
            Name = name;
            Type = type;
            Position = position;
            IsPassable = isPassable;
            Rotation = rotation;
            Size = new Size { Width = 1, Height = 1 }; // Default tile size
        }
    }
}

using System;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public enum ObjectType
    {
        Wall,
        Obstacle,
        Door,
        Chest,
        NPC,
        Player
    }

    public interface IWorldObject
    {
        long Id { get; }
        string Name { get; }
        ObjectType Type { get; }
        bool IsPassable { get; }
        Position Position { get; }
        Size Size { get; }
        float Rotation { get; }
    }
}

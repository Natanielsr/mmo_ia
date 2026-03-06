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
        string Id { get; }
        ObjectType Type { get; }
        bool IsPassable { get; }
        Position Position { get; }
        Size Size { get; }
        float Rotation { get; }
    }
}

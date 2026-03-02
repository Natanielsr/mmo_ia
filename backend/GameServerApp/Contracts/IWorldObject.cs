using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts
{
    public enum ObjectType
    {
        Wall,
        Obstacle,
        Door,
        Chest,
        NPC
    }

    public interface IWorldObject : ISceneObject
    {
        string Id { get; }
        ObjectType Type { get; }
        bool IsPassable { get; }
    }

    public interface ICollisionManager
    {
        void RegisterObject(IWorldObject worldObject);
        void RemoveObject(string objectId);
        bool CanPlayerMove(Position from, Position to);
        IWorldObject GetObjectAt(Position position);
        bool IsPositionBlocked(Position position);
    }
}

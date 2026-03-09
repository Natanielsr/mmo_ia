using System;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Managers
{
    public interface ICollisionManager
    {
        void RegisterDynamicObject(IDynamicWorldObject dynamicWorldObject);
        void RegisterStaticObject(IStaticWorldObject staticWorldObject);
        void RegisterObject(IWorldObject worldObject);
        void RemoveObject(long objectId);
        void UpdateObjectPosition(IWorldObject worldObject, Position oldPosition);
        bool CanPlayerMove(Position from, Position to);
        IWorldObject? GetObjectAt(Position position);
        bool IsPositionBlocked(Position position);
    }
}

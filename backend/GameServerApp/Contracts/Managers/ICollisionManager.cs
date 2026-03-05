using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Managers
{
    public interface ICollisionManager
    {
        void RegisterObject(IWorldObject worldObject);
        void RemoveObject(string objectId);
        bool CanPlayerMove(Position from, Position to);
        IWorldObject GetObjectAt(Position position);
        bool IsPositionBlocked(Position position);
    }
}

using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class CollisionManager : ICollisionManager
    {
        private readonly Dictionary<Position, IWorldObject> _objects = new();
        private readonly Dictionary<string, IWorldObject> _objectsById = new();

        public void RegisterObject(IWorldObject worldObject)
        {
            if (worldObject == null) return;
            
            _objects[worldObject.Position] = worldObject;
            _objectsById[worldObject.Id] = worldObject;
        }

        public void RemoveObject(string objectId)
        {
            if (_objectsById.TryGetValue(objectId, out var obj))
            {
                _objects.Remove(obj.Position);
                _objectsById.Remove(objectId);
            }
        }

        public void UpdateObjectPosition(IWorldObject worldObject, Position oldPosition)
        {
            if (worldObject == null) return;
            
            _objects.Remove(oldPosition);
            _objects[worldObject.Position] = worldObject;
            _objectsById[worldObject.Id] = worldObject; // Ensure dictionary is updated just in case
        }

        public bool IsPositionBlocked(Position position)
        {
            if (_objects.TryGetValue(position, out var obj))
            {
                return !obj.IsPassable;
            }
            return false;
        }

        public bool CanPlayerMove(Position from, Position to)
        {
            // For now, only checks destination
            return !IsPositionBlocked(to);
        }

        public IWorldObject GetObjectAt(Position position)
        {
            _objects.TryGetValue(position, out var obj);
            return obj;
        }
    }
}

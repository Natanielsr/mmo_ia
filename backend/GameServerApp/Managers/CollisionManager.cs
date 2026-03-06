using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class CollisionManager : ICollisionManager
    {
        private readonly Dictionary<Position, List<IWorldObject>> _objects = new();
        private readonly Dictionary<string, IWorldObject> _objectsById = new();

        public void RegisterObject(IWorldObject worldObject)
        {
            if (worldObject == null) return;
            
            if (!_objects.TryGetValue(worldObject.Position, out var list))
            {
                list = new List<IWorldObject>();
                _objects[worldObject.Position] = list;
            }
            
            if (!list.Contains(worldObject))
            {
                list.Add(worldObject);
            }
            
            _objectsById[worldObject.Id] = worldObject;
        }

        public void RemoveObject(string objectId)
        {
            if (_objectsById.TryGetValue(objectId, out var obj))
            {
                if (_objects.TryGetValue(obj.Position, out var list))
                {
                    list.Remove(obj);
                    if (list.Count == 0)
                    {
                        _objects.Remove(obj.Position);
                    }
                }
                _objectsById.Remove(objectId);
            }
        }

        public void UpdateObjectPosition(IWorldObject worldObject, Position oldPosition)
        {
            if (worldObject == null) return;
            
            // Remove from old position list
            if (_objects.TryGetValue(oldPosition, out var oldList))
            {
                oldList.Remove(worldObject);
                if (oldList.Count == 0)
                {
                    _objects.Remove(oldPosition);
                }
            }

            // Add to new position list
            if (!_objects.TryGetValue(worldObject.Position, out var newList))
            {
                newList = new List<IWorldObject>();
                _objects[worldObject.Position] = newList;
            }
            
            if (!newList.Contains(worldObject))
            {
                newList.Add(worldObject);
            }

            _objectsById[worldObject.Id] = worldObject;
        }

        public bool IsPositionBlocked(Position position)
        {
            if (_objects.TryGetValue(position, out var list))
            {
                return list.Any(obj => !obj.IsPassable);
            }
            return false;
        }

        public bool CanPlayerMove(Position from, Position to)
        {
            return !IsPositionBlocked(to);
        }

        public IWorldObject? GetObjectAt(Position position)
        {
            if (_objects.TryGetValue(position, out var list))
            {
                return list.FirstOrDefault();
            }
            return null;
        }
    }
}

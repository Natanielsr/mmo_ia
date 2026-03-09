// backend/GameServerApp/Managers/CollisionManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class CollisionManager : ICollisionManager
    {
        private readonly Dictionary<Position, List<IWorldObject>> _dynamicObjects = new();
        private readonly Dictionary<long, IWorldObject> _objectsById = new();
        private readonly IStaticWorldManager _staticWorld;

        public CollisionManager(IStaticWorldManager staticWorld)
        {
            _staticWorld = staticWorld ?? throw new ArgumentNullException(nameof(staticWorld));
        }

        public void RegisterObject(IWorldObject worldObject)
        {
            if (worldObject is IDynamicWorldObject dynamicWorldObject)
            {
                RegisterDynamicObject(dynamicWorldObject);
            }
            else if (worldObject is IStaticWorldObject staticWorldObject)
            {
                RegisterStaticObject(staticWorldObject);
            }
            else
            {
                throw new Exception("Invalid object type must be Dynamic or Static world object");
            }
        }

        public void RegisterDynamicObject(IDynamicWorldObject dynamicWorldObject)
        {
            if (dynamicWorldObject == null) return;

            if (!_dynamicObjects.TryGetValue(dynamicWorldObject.Position, out var list))
            {
                list = new List<IWorldObject>();
                _dynamicObjects[dynamicWorldObject.Position] = list;
            }

            if (!list.Contains(dynamicWorldObject))
            {
                list.Add(dynamicWorldObject);
            }

            _objectsById[dynamicWorldObject.Id] = dynamicWorldObject;
        }

        public void RegisterStaticObject(IStaticWorldObject staticWorldObject)
        {
            _staticWorld.AddStaticObject(staticWorldObject);
        }

        public void RemoveObject(long objectId)
        {
            if (_objectsById.TryGetValue(objectId, out var obj))
            {
                if (_dynamicObjects.TryGetValue(obj.Position, out var list))
                {
                    list.Remove(obj);
                    if (list.Count == 0)
                    {
                        _dynamicObjects.Remove(obj.Position);
                    }
                }
                _objectsById.Remove(objectId);
            }
        }

        public void UpdateObjectPosition(IWorldObject worldObject, Position oldPosition)
        {
            if (worldObject == null) return;

            // Remove from old position list
            if (_dynamicObjects.TryGetValue(oldPosition, out var oldList))
            {
                oldList.Remove(worldObject);
                if (oldList.Count == 0)
                {
                    _dynamicObjects.Remove(oldPosition);
                }
            }

            // Add to new position list
            if (!_dynamicObjects.TryGetValue(worldObject.Position, out var newList))
            {
                newList = new List<IWorldObject>();
                _dynamicObjects[worldObject.Position] = newList;
            }

            if (!newList.Contains(worldObject))
            {
                newList.Add(worldObject);
            }

            _objectsById[worldObject.Id] = worldObject;
        }

        public bool IsPositionBlocked(Position position)
        {
            // Primeiro verifica objetos estáticos (mais rápido)
            if (_staticWorld.IsBlocked(position))
                return true;

            // Depois verifica objetos dinâmicos (players, items móveis)
            if (_dynamicObjects.TryGetValue(position, out var list))
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
            // Primeiro verifica objetos estáticos
            var staticObj = _staticWorld.GetObjectAt(position);
            if (staticObj != null) return staticObj;

            // Depois verifica objetos dinâmicos
            if (_dynamicObjects.TryGetValue(position, out var list))
            {
                return list.FirstOrDefault();
            }
            return null;
        }

        public IEnumerable<IWorldObject> GetDynamicObjectsInArea(Position topLeft, Position bottomRight)
        {
            // Retorna apenas objetos dinâmicos na área
            foreach (var kvp in _dynamicObjects)
            {
                var pos = kvp.Key;
                if (pos.X >= topLeft.X && pos.X <= bottomRight.X &&
                    pos.Y >= topLeft.Y && pos.Y <= bottomRight.Y)
                {
                    foreach (var obj in kvp.Value)
                    {
                        yield return obj;
                    }
                }
            }
        }

    }
}
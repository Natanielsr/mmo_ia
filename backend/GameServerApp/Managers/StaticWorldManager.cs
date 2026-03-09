// backend/GameServerApp/Managers/StaticWorldManager.cs
using System.Collections.Generic;
using System.Linq;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServerApp.Managers
{
    /// <summary>
    /// Implementação simples de StaticWorldManager usando Dictionary esparso
    /// Para mapa procedural sem tamanho fixo
    /// </summary>
    public class StaticWorldManager : IStaticWorldManager
    {
        private readonly HashSet<Position> _blockedPositions = new();
        private readonly Dictionary<Position, StaticObject> _staticObjects = new();

        public bool IsBlocked(Position position) => _blockedPositions.Contains(position);

        public bool IsPassable(Position position) => !_blockedPositions.Contains(position);

        public StaticObject? GetObjectAt(Position position)
        {
            return _staticObjects.TryGetValue(position, out var obj) ? obj : null;
        }

        public IEnumerable<StaticObject> GetObjectsInArea(Position topLeft, Position bottomRight)
        {
            // Implementação simples - percorre todos os objetos
            // Otimização futura: se muitos objetos, usar spatial index ou chunks
            foreach (var obj in _staticObjects.Values)
            {
                if (obj.Position.X >= topLeft.X && obj.Position.X <= bottomRight.X &&
                    obj.Position.Y >= topLeft.Y && obj.Position.Y <= bottomRight.Y)
                {
                    yield return obj;
                }
            }
        }

        public void AddObstacle(Position position, bool isBlocking = true)
        {
            if (isBlocking)
            {
                _blockedPositions.Add(position);
            }
            else
            {
                _blockedPositions.Remove(position);
            }
        }

        public void AddStaticObject(StaticObject staticObject)
        {
            if (staticObject == null) return;

            _staticObjects[staticObject.Position] = staticObject;

            if (!staticObject.IsPassable)
            {
                _blockedPositions.Add(staticObject.Position);
            }
        }

        public bool RemoveObjectAt(Position position)
        {
            if (_staticObjects.TryGetValue(position, out var obj))
            {
                _staticObjects.Remove(position);

                if (!obj.IsPassable)
                {
                    _blockedPositions.Remove(position);
                }

                return true;
            }
            return false;
        }

        public void Clear()
        {
            _blockedPositions.Clear();
            _staticObjects.Clear();
        }
    }
}
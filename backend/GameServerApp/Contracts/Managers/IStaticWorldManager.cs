// backend/GameServerApp/Contracts/Managers/IStaticWorldManager.cs
using GameServerApp.Contracts.Types;
using GameServerApp.World;

namespace GameServerApp.Contracts.Managers
{
    /// <summary>
    /// Gerencia objetos estáticos do mundo (terreno, obstáculos, estruturas)
    /// Objetos estáticos não se movem e são carregados no initialization
    /// </summary>
    public interface IStaticWorldManager
    {
        bool IsBlocked(Position position);
        bool IsPassable(Position position);
        StaticObject? GetObjectAt(Position position);
        IEnumerable<StaticObject> GetObjectsInArea(Position topLeft, Position bottomRight);
        void AddObstacle(Position position, bool isBlocking = true);
        void AddStaticObject(StaticObject staticObject);
        bool RemoveObjectAt(Position position);
        void Clear();
    }
}
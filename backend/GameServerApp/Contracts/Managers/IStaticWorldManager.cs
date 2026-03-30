// backend/GameServerApp/Contracts/Managers/IStaticWorldManager.cs
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
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
        IReadOnlyDictionary<Position, IStaticWorldObject> GetAllObjects();
        IStaticWorldObject? GetObjectAt(Position position);
        IEnumerable<IStaticWorldObject> GetObjectsInArea(Position topLeft, Position bottomRight);
    bool IsChunkLoaded(ChunkCoord coord);
    IEnumerable<IStaticWorldObject> GetChunkObjects(ChunkCoord coord);
    void AddStaticObject(IStaticWorldObject staticObject);
    bool RemoveObjectAt(Position position);
    void Clear();
    }
}
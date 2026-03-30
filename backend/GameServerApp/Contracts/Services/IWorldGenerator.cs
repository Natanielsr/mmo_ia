using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services
{
    public interface IWorldGenerator
    {
        /// <summary>
        /// Gera objetos estáticos para um chunk específico.
        /// </summary>
        void GenerateChunk(ChunkCoord coord);
    }
}

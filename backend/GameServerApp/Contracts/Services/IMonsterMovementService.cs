using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services
{
    public interface IMonsterMovementService
    {
        /// <summary>
        /// Atualiza a posição de todos os monstros no mundo
        /// </summary>
        void UpdateAllMonsters();
        
        /// <summary>
        /// Atualiza a posição de um monstro específico
        /// </summary>
        void UpdateMonster(IMonster monster);
        
        /// <summary>
        /// Calcula uma nova posição para o monstro baseado em comportamento
        /// </summary>
        Position CalculateNewPosition(IMonster monster);
    }
}
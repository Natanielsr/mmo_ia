using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Services
{
    public interface ILootTableService
    {
        /// <summary>
        /// Rola a tabela de loot e retorna um item ou null.
        /// O parâmetro rng permite injetar um gerador determinístico nos testes.
        /// </summary>
        IItem? RollLoot(Position position, string itemId, Func<double>? rng = null);
    }
}

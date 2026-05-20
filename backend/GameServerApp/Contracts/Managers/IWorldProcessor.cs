using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Managers
{
    public interface IWorldManager
    {
        /// <summary>
        /// Processes a movement request from a player.
        /// </summary>
        bool ProcessPlayerMovement(IPlayer player, string direction);

        /// <summary>
        /// Processes an attack action between a player and a target.
        /// </summary>
        void ProcessPlayerAttack(IPlayer player, IPlayer target);
        
        void ProcessPlayerAttackMonster(IPlayer player, string monsterId);

        /// <summary>
        /// Executes a global world tick (processing monsters, respawns, etc.)
        /// </summary>
        void Tick();

        void InstantiateObject(IWorldObject worldObject);
        void ProcessChunkLoading(IPlayer player, string connectionId);
        void ProcessUseItem(IPlayer player, string itemId);
        void ProcessDropItem(IPlayer player, string itemId, Position targetPos);
        bool ProcessEquipItem(IPlayer player, string itemId);
        bool ProcessUnequipItem(IPlayer player, EquipmentSlot slot);
    }
}

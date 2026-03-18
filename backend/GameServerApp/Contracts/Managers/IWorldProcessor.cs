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
    }
}

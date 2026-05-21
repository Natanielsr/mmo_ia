using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Processors
{
    public interface ICombatProcessor
    {
        void ProcessPlayerAttack(IPlayer player, IPlayer target);
        void ProcessPlayerAttackMonster(IPlayer player, string monsterId);
        void ProcessMonsterCombat();
    }
}

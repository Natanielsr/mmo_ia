using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;

namespace GameServerApp.World
{
    public class CombatService : ICombatService
    {
        public int Attack(int hp, int damage)
        {
            return Math.Max(0, hp - damage);
        }
    }
}

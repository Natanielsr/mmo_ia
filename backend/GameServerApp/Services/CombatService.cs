using GameServerApp.Contracts.Services;

namespace GameServerApp.Services
{
    public class CombatService : ICombatService
    {
        public int Attack(int hp, int damage)
        {
            return Math.Max(0, hp - damage);
        }
    }
}

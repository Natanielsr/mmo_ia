using GameServerApp.Contracts;

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

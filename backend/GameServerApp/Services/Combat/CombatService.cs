using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Services.Combat;
using GameServerApp.Contracts.Services.Items;
using GameServerApp.Contracts.Services.Movement;
using GameServerApp.Contracts.Services.Ranking;
using GameServerApp.Contracts.Services.Repositories;
using GameServerApp.Contracts.Services.World;

namespace GameServerApp.Services.Combat
{
    public class CombatService : ICombatService
    {
        public int Attack(int hp, int damage)
        {
            return Math.Max(0, hp - damage);
        }
    }
}

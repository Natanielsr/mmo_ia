using GameServerApp.Contracts.World;
namespace GameServerApp.Contracts.Services
{
    public interface ICombatService
    {
        int Attack(int hp, int damage);
    }
}

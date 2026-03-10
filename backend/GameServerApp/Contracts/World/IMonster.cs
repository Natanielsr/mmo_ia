using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World;

public interface IMonster : IDynamicWorldObject
{
    int Hp { get; }
    int MaxHp { get; }
    int AttackPower { get; }
    bool IsDead { get; }

    void MoveTo(Position newPosition);
    void TakeDamage(int damage);
    void Heal(int amount);
}

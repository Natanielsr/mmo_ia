using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World;

public interface IMonster : IDynamicWorldObject
{
    int Hp { get; }
    int MaxHp { get; }
    int AttackPower { get; }
    bool IsDead { get; }
    Position SpawnPosition { get; }
    MonsterState State { get; }
    MonsterBehavior Behavior { get; }
    int ExperienceReward { get; }
    DateTime LastMovementTime { get; set; }
    DateTime LastAttackTime { get; set; }

    void Move(Position newPosition);
    void TakeDamage(int damage);
    void Heal(int amount);
    void SetBehavior(MonsterBehavior behavior);
    bool CanMove();
    bool CanAttack();
    void Attack(IPlayer target);
}

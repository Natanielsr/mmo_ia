using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.World;

public class Monster : IMonster
{
    public long Id { get; }
    public string Name { get; }
    public string ObjectCode { get; }
    public ObjectType Type => ObjectType.NPC;
    public bool IsPassable => false;
    public Position Position { get; private set; }
    public Position SpawnPosition { get; }
    public Size Size { get; } = new() { Width = 1, Height = 1 };
    public float Rotation { get; private set; }
    public MonsterState State => IsDead ? MonsterState.Dead : MonsterState.Alive;
    public MonsterBehavior Behavior { get; private set; }
    public DateTime LastMovementTime { get; set; }
    public DateTime LastAttackTime { get; set; }

    public int Hp { get; private set; }
    public int MaxHp { get; }
    public int AttackPower { get; }
    public bool IsDead => Hp <= 0;

    public Monster(
        long id,
        string name,
        string objectCode,
        Position spawnPosition,
        int maxHp = 50,
        int attackPower = 8,
        MonsterBehavior behavior = MonsterBehavior.Patrolling)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Monster name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(objectCode))
            throw new ArgumentException("Monster objectCode is required", nameof(objectCode));

        if (maxHp <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHp));

        if (attackPower < 0)
            throw new ArgumentOutOfRangeException(nameof(attackPower));

        Id = id;
        Name = name;
        ObjectCode = objectCode;
        Position = spawnPosition;
        SpawnPosition = spawnPosition;
        MaxHp = maxHp;
        Hp = maxHp;
        AttackPower = attackPower;
        Behavior = behavior;
        Rotation = 0f;
        LastMovementTime = DateTime.UtcNow;
        LastAttackTime = DateTime.UtcNow;
    }

    public void Move(Position newPosition)
    {
        if (IsDead) return;
        Position = newPosition;
        LastMovementTime = DateTime.UtcNow;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;
        Hp = Math.Max(0, Hp - damage);
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        Hp = Math.Min(MaxHp, Hp + amount);
    }

    public void SetRotation(float rotation)
    {
        Rotation = rotation;
    }

    public void SetBehavior(MonsterBehavior behavior)
    {
        Behavior = behavior;
    }

    public bool CanMove()
    {
        return !IsDead && (DateTime.UtcNow - LastMovementTime).TotalSeconds >= 1.0;
    }

    public bool CanAttack()
    {
        return !IsDead && (DateTime.UtcNow - LastAttackTime).TotalSeconds >= 2.0;
    }

    public void Attack(IPlayer target)
    {
        if (IsDead || !CanAttack()) return;

        target.TakeDamage(AttackPower);
        LastAttackTime = DateTime.UtcNow;
    }
}

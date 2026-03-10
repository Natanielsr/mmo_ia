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
    public Size Size { get; } = new() { Width = 1, Height = 1 };
    public float Rotation { get; private set; }

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
        int attackPower = 8)
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
        MaxHp = maxHp;
        Hp = maxHp;
        AttackPower = attackPower;
        Rotation = 0f;
    }

    public void MoveTo(Position newPosition)
    {
        if (IsDead) return;
        Position = newPosition;
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
}

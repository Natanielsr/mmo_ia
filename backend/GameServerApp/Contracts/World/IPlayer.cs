using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public enum PlayerState
    {
        Alive,
        Dead,
        InCombat,
        Resting
    }

    public interface IPlayer
    {
        string Name { get; }
        Position Position { get; }
        int Hp { get; }
        int MaxHp { get; }
        int Level { get; }
        long Experience { get; }
        PlayerState State { get; }

        void Move(Position newPosition);
        void Attack(IPlayer target);
        void TakeDamage(int damage);
        void Die();
        void Revive();
        void Heal(int amount);
        void Rest();
        void StopResting();
        void GainExperience(long amount);
        void EquipItem(string itemId);
        void UnequipItem(string itemId);
    }
}

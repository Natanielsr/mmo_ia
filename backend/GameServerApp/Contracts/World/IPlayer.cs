using GameServerApp.Contracts.Types;
using System.Collections.Generic;

namespace GameServerApp.Contracts.World
{
    public enum PlayerState
    {
        Alive,
        Dead,
        InCombat,
        Resting
    }

    public interface IPlayer : IDynamicWorldObject
    {
        int Hp { get; }
        int MaxHp { get; }
        bool IsDead { get; }
        int Level { get; }
        long Experience { get; }
        double Speed { get; }
        int AttackPoints { get; }
        int TotalAttackPower { get; }
        int TotalDefense { get; }
        DateTime JoinedAt { get; }
        DateTime LastMoveTime { get; }
        DateTime LastAttackTime { get; }
        PlayerState State { get; }
        bool IsGodMode { get; }
        bool IsNoClip { get; }
        HashSet<ChunkCoord> SentChunks { get; }

        void Move(Position newPosition);
        void Attack(IWorldObject target);
        void TakeDamage(int damage);
        void Die();
        void Revive();
        void Heal(int amount);
        void Rest();
        void StopResting();
        void GainExperience(long amount);
        void EquipItem(string itemId);
        void UnequipItem(string itemId);
        void ApplyEquipmentBonuses(int attackBonus, int defenseBonus);
        void SetSpeed(double speed);
        void SetGodMode(bool enabled);
        void SetNoClip(bool enabled);
        void SetLevel(int targetLevel);
    }
}

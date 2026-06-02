using System;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class Player : IPlayer
    {
        public long Id { get; }
        public string Name { get; }
        public string ObjectCode { get; } = "player";
        public ObjectType Type => ObjectType.Player;
        public bool IsPassable => false;
        public Size Size { get; } = new Size { Width = 1, Height = 1 };
        public float Rotation { get; } = 0f;

        public Position Position { get; private set; }
        public int Hp { get; private set; }
        public int MaxHp { get; private set; }
        public int Level { get; private set; }
        public long Experience { get; private set; }
        public double Speed { get; private set; }

        public int AttackPoints { get; private set; }
        public int TotalAttackPower { get; private set; }
        public int TotalDefense { get; private set; }
        public DateTime JoinedAt { get; } = DateTime.UtcNow;
        public DateTime LastMoveTime { get; private set; }
        public DateTime LastAttackTime { get; private set; }
        public PlayerState State { get; private set; }
        public bool IsDead => State == PlayerState.Dead;
        public bool IsGodMode { get; private set; }

        private int _equipmentAttackBonus = 0;
        private int _equipmentDefenseBonus = 0;
        private int _baseDefense = 0;

        public Player(long id, string name, Position startPosition, int attackPoints = 10, int maxHp = 100)
        {
            if (startPosition == null)
            {
                throw new NullReferenceException("Player Start position can´t be null!");
            }

            Id = id;
            Name = name;
            Position = startPosition;
            MaxHp = maxHp;
            Hp = maxHp;
            AttackPoints = attackPoints;
            TotalAttackPower = attackPoints;
            TotalDefense = 0;
            Level = 1;
            Experience = 0;
            Speed = 2.0;
            LastMoveTime = DateTime.MinValue;
            LastAttackTime = DateTime.MinValue;
            State = PlayerState.Alive;
        }

        public void Move(Position newPosition)
        {
            if (State == PlayerState.Dead)
                return;

            Position = newPosition;
            LastMoveTime = DateTime.UtcNow;
        }

        public void Attack(IWorldObject target)
        {
            if (State == PlayerState.Dead)
                return;

            State = PlayerState.InCombat;
            LastAttackTime = DateTime.UtcNow;
        }

        public void TakeDamage(int damage)
        {
            if (State == PlayerState.Dead || IsGodMode) return;

            int effective = Math.Max(0, damage - TotalDefense);
            Hp = Math.Max(0, Hp - effective);

            if (Hp == 0) Die();
        }

        public void ApplyEquipmentBonuses(int attackBonus, int defenseBonus)
        {
            _equipmentAttackBonus = attackBonus;
            _equipmentDefenseBonus = defenseBonus;
            TotalAttackPower = AttackPoints + _equipmentAttackBonus;
            TotalDefense = _baseDefense + _equipmentDefenseBonus;
        }

        public void Die()
        {
            State = PlayerState.Dead;
            Hp = 0;
        }

        public void Revive()
        {
            if (State != PlayerState.Dead)
                return;

            State = PlayerState.Alive;
            Hp = MaxHp;
        }

        public void Heal(int amount)
        {
            if (State == PlayerState.Dead)
                return;

            Hp = Math.Min(MaxHp, Hp + amount);
        }

        public void Rest()
        {
            if (State == PlayerState.Dead)
                return;

            State = PlayerState.Resting;
        }

        public void StopResting()
        {
            if (State == PlayerState.Resting)
                State = PlayerState.Alive;
        }

        public void GainExperience(long amount)
        {
            if (State == PlayerState.Dead)
                return;

            Experience += amount;
            CheckLevelUp();
        }

        public void EquipItem(string itemId)
        {
            if (State == PlayerState.Dead)
                return;

            // Will be enhanced when inventory system is implemented
        }

        public void UnequipItem(string itemId)
        {
            if (State == PlayerState.Dead)
                return;

            // Will be enhanced when inventory system is implemented
        }

        private static long ExperienceThreshold(int level)
            => (long)(10000.0 * (Math.Pow(1.1, level) - 1.0));

        private void CheckLevelUp()
        {
            while (Experience >= ExperienceThreshold(Level))
            {
                Level++;
                MaxHp += 10;
                Hp = MaxHp;
                AttackPoints += 2;
                _baseDefense += 1;
                TotalAttackPower = AttackPoints + _equipmentAttackBonus;
                TotalDefense = _baseDefense + _equipmentDefenseBonus;
            }
        }

        public void SetGodMode(bool enabled)
        {
            IsGodMode = enabled;
        }

        public void SetLevel(int targetLevel)
        {
            Level = targetLevel;
            Experience = 0;
            MaxHp = 100 + (10 * (targetLevel - 1));
            Hp = MaxHp;
            AttackPoints = 10 + (2 * (targetLevel - 1));
            _baseDefense = targetLevel - 1;
            TotalAttackPower = AttackPoints + _equipmentAttackBonus;
            TotalDefense = _baseDefense + _equipmentDefenseBonus;
        }

        private record HoTEffect(string Source, int HealPerTick, int TicksRemaining);
        private readonly List<HoTEffect> _activeHoTs = [];

        public void ApplyHoT(string source, int healPerTick, int totalTicks)
        {
            _activeHoTs.RemoveAll(h => h.Source == source);
            _activeHoTs.Add(new HoTEffect(source, healPerTick, totalTicks));
        }

        public void AccumulateHoT(string source, int healPerTick, int additionalTicks)
        {
            var idx = _activeHoTs.FindIndex(h => h.Source == source);
            if (idx >= 0)
                _activeHoTs[idx] = _activeHoTs[idx] with { TicksRemaining = _activeHoTs[idx].TicksRemaining + additionalTicks };
            else
                _activeHoTs.Add(new HoTEffect(source, healPerTick, additionalTicks));
        }

        public bool TickHoT()
        {
            if (_activeHoTs.Count == 0 || State == PlayerState.Dead) return false;
            for (int i = _activeHoTs.Count - 1; i >= 0; i--)
            {
                var h = _activeHoTs[i];
                Heal(h.HealPerTick);
                _activeHoTs[i] = h with { TicksRemaining = h.TicksRemaining - 1 };
                if (_activeHoTs[i].TicksRemaining <= 0) _activeHoTs.RemoveAt(i);
            }
            return true;
        }
    }
}

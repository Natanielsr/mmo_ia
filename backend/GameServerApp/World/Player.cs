using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.World
{
    public class Player : IPlayer
    {
        public string Name { get; }
        public string Id => Name;
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
        public DateTime LastMoveTime { get; private set; }
        public PlayerState State { get; private set; }

        private const long ExperiencePerLevel = 1000;

        public Player(string name, Position startPosition, int maxHp = 100)
        {
            Name = name;
            Position = startPosition;
            MaxHp = maxHp;
            Hp = maxHp;
            Level = 1;
            Experience = 0;
            Speed = 4.0; // Default speed: 4 positions per second
            LastMoveTime = DateTime.MinValue;
            State = PlayerState.Alive;
        }

        public void Move(Position newPosition)
        {
            if (State == PlayerState.Dead)
                return;

            Position = newPosition;
            LastMoveTime = DateTime.UtcNow;
        }

        public void Attack(IPlayer target)
        {
            if (State == PlayerState.Dead)
                return;

            State = PlayerState.InCombat;
        }

        public void TakeDamage(int damage)
        {
            if (State == PlayerState.Dead)
                return;

            Hp = Math.Max(0, Hp - damage);

            if (Hp == 0)
                Die();
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

        private void CheckLevelUp()
        {
            while (Experience >= Level * ExperiencePerLevel)
            {
                Level++;
                MaxHp += 10;
                Hp = MaxHp;
            }
        }
    }
}

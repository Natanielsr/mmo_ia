using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public interface IWorldEvents
    {
        /// <summary>
        /// Triggered when a player moves to a new position.
        /// </summary>
        void OnPlayerMoved(string playerId, Position newPosition);

        /// <summary>
        /// Triggered when a player attacks another player/target.
        /// </summary>
        void OnPlayerAttacked(string attackerId, string targetId, int damage);

        /// <summary>
        /// Triggered when a player dies.
        /// </summary>
        void OnPlayerDied(string playerId);

        /// <summary>
        /// Triggered when a player gains experience.
        /// </summary>
        void OnPlayerExperienceGained(string playerId, long amount, long totalExperience);

        /// <summary>
        /// Triggered when a player levels up.
        /// </summary>
        void OnPlayerLevelUp(string playerId, int newLevel);
    }
}

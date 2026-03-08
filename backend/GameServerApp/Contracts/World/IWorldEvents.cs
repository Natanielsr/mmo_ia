using System;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.World
{
    public interface IWorldEvents
    {
        /// <summary>
        /// Triggered when a player moves to a new position.
        /// </summary>
        void OnPlayerMoved(Guid playerId, Position newPosition);
        
        /// <summary>
        /// Triggered when a player joins the world.
        /// </summary>
        void OnPlayerJoined(Guid playerId, Position spawnPosition);

        /// <summary>
        /// Triggered when a player attacks another player/target.
        /// </summary>
        void OnPlayerAttacked(Guid attackerId, Guid targetId, int damage);

        /// <summary>
        /// Triggered when a player dies.
        /// </summary>
        void OnPlayerDied(Guid playerId);

        /// <summary>
        /// Triggered when a player gains experience.
        /// </summary>
        void OnPlayerExperienceGained(Guid playerId, long amount, long totalExperience);

        /// <summary>
        /// Triggered when a player levels up.
        /// </summary>
        void OnPlayerLevelUp(Guid playerId, int newLevel);

        /// <summary>
        /// Triggered when a player leaves the world.
        /// </summary>
        void OnPlayerLeft(Guid playerId);
    }
}

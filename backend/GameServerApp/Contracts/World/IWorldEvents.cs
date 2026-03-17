using System;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.World
{
    public interface IWorldEvents
    {
        /// <summary>
        /// Triggered when a player moves to a new position.
        /// </summary>
        void OnPlayerMoved(PlayerPositionData playerPositionData);
        void OnPlayerStatusUpdated(PlayerStatusData playerStatusData);

        /// <summary>
        /// Triggered when a player joins the world.
        /// </summary>
        void OnPlayerJoined(PlayerPositionData playerPositionData);

        /// <summary>
        /// Triggered when a player attacks another player/target.
        /// </summary>
        void OnPlayerAttacked(PlayerAttackData attackData);

        /// <summary>
        /// Triggered when a player dies.
        /// </summary>
        void OnPlayerDied(long playerId);

        /// <summary>
        /// Triggered when a player gains experience.
        /// </summary>
        void OnPlayerExperienceGained(long playerId, long amount, long totalExperience);

        /// <summary>
        /// Triggered when a player levels up.
        /// </summary>
        void OnPlayerLevelUp(long playerId, int newLevel);

        /// <summary>
        /// Triggered when a player leaves the world.
        /// </summary>
        void OnPlayerLeft(long playerId);

        /// <summary>
        /// Triggered when a monster spawns.
        /// </summary>
        void OnMonsterSpawned(MonsterData monsterData);

        /// <summary>
        /// Triggered when a monster moves.
        /// </summary>
        void OnMonsterMoved(MonsterData monsterData);

        /// <summary>
        /// Triggered when a monster dies.
        /// </summary>
        void OnMonsterDied(string monsterId);

        /// <summary>
        /// Triggered when a monster takes damage.
        /// </summary>
        void OnMonsterDamaged(string monsterId, int damage, int currentHp);
    }
}

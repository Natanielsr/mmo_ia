using System;
using System.Collections.Generic;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.World
{
    public interface IWorldEvents
    {
        /// <summary>
        /// Triggered when a player moves to a new position.
        /// </summary>
        void OnPlayerMoved(PlayerData playerPositionData);

        /// <summary>
        /// Triggered when player HP changes (damage, regen, heal). Does not carry level/stats.
        /// </summary>
        void OnPlayerHpChanged(PlayerHpData playerHpData);

        /// <summary>
        /// Triggered when player status changes due to level-up or equipment change.
        /// </summary>
        void OnPlayerStatusUpdated(PlayerStatusData playerStatusData);

        /// <summary>
        /// Triggered when a player joins the world.
        /// </summary>
        void OnPlayerJoined(PlayerData playerPositionData);

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
        /// Triggered when a monster is removed without dying (e.g., distance-based removal).
        /// </summary>
        void OnMonsterRemoved(string monsterId);

        /// <summary>
        /// Triggered when a monster takes damage.
        /// </summary>
        void OnMonsterDamaged(string monsterId, int damage, int currentHp);

        /// <summary>
        /// Triggered when an item is dropped in the world.
        /// </summary>
        void OnItemDropped(IItem item);

        /// <summary>
        /// Triggered when an item is picked up by a player.
        /// </summary>
        void OnItemPickedUp(string itemId, long playerId);

        /// <summary>
        /// Triggered when a player's inventory changes.
        /// </summary>
        void OnInventoryUpdated(long playerId, IReadOnlyList<(int SlotIndex, IItem Item)> slots);

        /// <summary>
        /// Triggered when a player's equipment changes.
        /// </summary>
        void OnEquipmentUpdated(long playerId, IReadOnlyDictionary<EquipmentSlot, IItem?> slots);

        /// <summary>
        /// Triggered when a chunk is loaded for a player.
        /// </summary>
        void OnChunkLoaded(string connectionId, ChunkData chunkData);

        /// <summary>
        /// Triggered when a player equips or unequips any item. Broadcast to all other players.
        /// </summary>
        void OnPlayerItemEquipped(string playerId, string slot, string? itemName);
    }
}

using System;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.World;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class GameStateManager : IGameStateManager
    {
        private readonly ICollisionManager _collisionManager;
        private readonly Position _hospitalSpawnPoint = new Position(50, 50);

        public GameStateManager(ICollisionManager collisionManager)
        {
            _collisionManager = collisionManager;
        }

        public void SpawnPlayer(IPlayer player, Position spawnPoint)
        {
            player.Move(spawnPoint);
            _collisionManager.RegisterDynamicObject(player);
        }

        public void PlayerDied(IPlayer player)
        {
            player.Die();
            _collisionManager.RemoveObject(player.Id);
        }

        public void SendPlayerToHospital(IPlayer player)
        {
            player.Revive();
            Position oldPos = player.Position;
            player.Move(_hospitalSpawnPoint);
            _collisionManager.UpdateObjectPosition(player, oldPos);
        }

        public void MonsterKilled(IPlayer killer, Guid monsterId)
        {
        }

        public void DropItem(string itemId, Position dropPosition)
        {
        }

        public void PlayerPicksUpItem(IPlayer player, string itemId)
        {
            player.EquipItem(itemId);
        }

        public void AddPlayerExperience(IPlayer player, long experienceAmount)
        {
            player.GainExperience(experienceAmount);
        }

        public void CheckForLevelUp(IPlayer player)
        {
        }

        public Position GetHospitalSpawnPoint()
        {
            return _hospitalSpawnPoint;
        }
    }
}

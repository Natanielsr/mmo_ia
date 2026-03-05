using GameServerApp.Contracts;
using GameServerApp.Contracts.Types;

namespace GameServerApp.Managers
{
    public class GameStateManager : IGameStateManager
    {
        private readonly Position _hospitalSpawnPoint = new Position(50, 50);

        public void SpawnPlayer(IPlayer player, Position spawnPoint)
        {
            player.Move(spawnPoint);
        }

        public void PlayerDied(IPlayer player)
        {
            player.Die();
        }

        public void SendPlayerToHospital(IPlayer player)
        {
            player.Revive();
            player.Move(_hospitalSpawnPoint);
        }

        public void MonsterKilled(IPlayer killer, string monsterId)
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

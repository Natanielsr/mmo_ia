using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts
{
    public interface IGameStateManager
    {
        void SpawnPlayer(IPlayer player, Position spawnPoint);
        void PlayerDied(IPlayer player);
        void SendPlayerToHospital(IPlayer player);
        void MonsterKilled(IPlayer killer, string monsterId);
        void DropItem(string itemId, Position dropPosition);
        void PlayerPicksUpItem(IPlayer player, string itemId);
        void AddPlayerExperience(IPlayer player, long experienceAmount);
        void CheckForLevelUp(IPlayer player);
        Position GetHospitalSpawnPoint();
    }
}

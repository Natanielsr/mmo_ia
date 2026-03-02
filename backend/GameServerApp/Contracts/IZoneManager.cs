using System;

namespace GameServerApp.Contracts
{
    public interface IZoneManager
    {
        void PlayerEnterZone(IPlayer player, string zoneId);
        void PlayerLeaveZone(IPlayer player, string zoneId);
        event Action<IPlayer, string> OnPlayerEnter;
        event Action<IPlayer, string> OnPlayerLeave;
    }
}
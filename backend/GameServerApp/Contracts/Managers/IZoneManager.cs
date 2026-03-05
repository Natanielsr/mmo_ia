using GameServerApp.Contracts.World;
using System;

namespace GameServerApp.Contracts.Managers
{
    public interface IZoneManager
    {
        void PlayerEnterZone(IPlayer player, string zoneId);
        void PlayerLeaveZone(IPlayer player, string zoneId);
        event Action<IPlayer, string> OnPlayerEnter;
        event Action<IPlayer, string> OnPlayerLeave;
    }
}
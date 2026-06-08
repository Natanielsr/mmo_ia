using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services;

public interface IPlayerSpawnService
{
    Position FindSafePosition();
}

using GameServerApp.Contracts.Types;

namespace GameServerApp.Contracts.Services.Movement;

public interface IPlayerSpawnService
{
    Position FindSafePosition();
}

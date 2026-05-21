using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Processors
{
    public interface IPlayerMovementProcessor
    {
        bool ProcessPlayerMovement(IPlayer player, string direction);
        bool IsPlayerMoving(IPlayer player);
    }
}

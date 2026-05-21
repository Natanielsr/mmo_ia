using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;

namespace GameServerApp.Contracts.Processors
{
    public interface IItemProcessor
    {
        void ProcessItemPickup(IPlayer player);
        void ProcessUseItem(IPlayer player, string itemId);
        void ProcessDropItem(IPlayer player, string itemId, Position targetPos);
    }
}

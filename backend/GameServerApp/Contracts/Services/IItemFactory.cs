using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.Dtos;

namespace GameServerApp.Contracts.Services;

public interface IItemFactory
{
    IItem Create(ItemDefinition def, string instanceId, Position position);
}

using GameServerApp.Services.World.MapGenerator;

namespace GameServerApp.Contracts.Services.World;

public interface IBiomeMap
{
    Biome GetBiomeAt(int tileX, int tileY);
}

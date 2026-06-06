using FractalRiver;

namespace GameServerApp.Contracts.Services;

public interface IBiomeMap
{
    Biome GetBiomeAt(int tileX, int tileY);
}

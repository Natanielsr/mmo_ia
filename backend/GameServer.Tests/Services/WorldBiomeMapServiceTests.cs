using FractalRiver;
using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Services;
using GameServerApp.Services;
using Microsoft.Extensions.Options;

namespace GameServer.Tests.Services;

public class WorldBiomeMapServiceTests
{
    private const int Width     = 32;
    private const int Height    = 32;
    private const int ChunkSize = 16;

    private static IBiomeMap Create(int seed = 42)
    {
        var config = Options.Create(new WorldConfig
        {
            Map = new MapConfig { Width = Width, Height = Height, ChunkSize = ChunkSize, GeneratorSeed = seed }
        });
        return new WorldBiomeMapService(config);
    }

    [Fact]
    public void GetBiomeAt_NorthernmostTile_ReturnsSand()
    {
        // row=0 → northness=1.0 → always Sand regardless of noise
        var sut = Create();
        Assert.Equal(Biome.Sand, sut.GetBiomeAt(0, 0));
    }

    [Fact]
    public void GetBiomeAt_SouthernmostTile_ReturnsSnow()
    {
        // row=rows-1 → southness≈1.0 → always Snow regardless of noise
        var sut = Create();
        Assert.Equal(Biome.Snow, sut.GetBiomeAt(0, Height - 1));
    }

    [Fact]
    public void GetBiomeAt_CenterTile_ReturnsGrass()
    {
        // row=rows/2 → northness=southness=0.50 → never Sand or Snow
        var sut = Create();
        Assert.Equal(Biome.Grass, sut.GetBiomeAt(0, Height / 2));
    }

    [Fact]
    public void GetBiomeAt_OutOfBoundsNorth_ClampsToNorthEdge()
    {
        var sut = Create();
        Assert.Equal(sut.GetBiomeAt(0, 0), sut.GetBiomeAt(0, -1));
    }

    [Fact]
    public void GetBiomeAt_OutOfBoundsSouth_ClampsToSouthEdge()
    {
        var sut = Create();
        Assert.Equal(sut.GetBiomeAt(0, Height - 1), sut.GetBiomeAt(0, 9999));
    }

    [Fact]
    public void GetBiomeAt_SameSeed_IsDeterministic()
    {
        var sut1 = Create(seed: 123);
        var sut2 = Create(seed: 123);
        for (int y = 0; y < Height; y += 3)
            Assert.Equal(sut1.GetBiomeAt(0, y), sut2.GetBiomeAt(0, y));
    }

    [Fact]
    public void GetBiomeAt_DifferentSeeds_ProduceDifferentNoiseMaps()
    {
        var sut1 = Create(seed: 1);
        var sut2 = Create(seed: 9999);
        // Verificar zona central onde o ruído causa diferença (evitando bordas que são sempre Sand/Snow)
        bool any = false;
        for (int y = Height / 2 - 8; y <= Height / 2 + 8 && !any; y += 2)
        {
            var biome1 = sut1.GetBiomeAt(0, y);
            var biome2 = sut2.GetBiomeAt(0, y);
            if (biome1 != biome2)
                any = true;
        }
        Assert.True(any, "Seeds diferentes devem gerar ao menos alguma diferença no mapa de biomas");
    }

    [Fact]
    public void GetBiomeAt_FarEastTile_ReturnsDarkForest()
    {
        // col clamped to Width-1 (east edge), latitude=center → not Sand/Snow → DarkForest
        var sut = Create();
        Assert.Equal(Biome.DarkForest, sut.GetBiomeAt(9999, Height / 2));
    }

    [Fact]
    public void GetBiomeAt_FarWestTile_ReturnsGrass()
    {
        // col clamped to 0 (west edge), latitude=center → not Sand/Snow/DarkForest → Grass
        var sut = Create();
        Assert.Equal(Biome.Grass, sut.GetBiomeAt(-1, Height / 2));
    }

    [Fact]
    public void GetBiomeAt_EastNorthTile_ReturnsSand()
    {
        // Sand (north) has priority over DarkForest (east)
        var sut = Create();
        Assert.Equal(Biome.Sand, sut.GetBiomeAt(9999, -1));
    }

    [Fact]
    public void GetBiomeAt_EastSouthTile_ReturnsSnow()
    {
        // Snow (south) has priority over DarkForest (east)
        var sut = Create();
        Assert.Equal(Biome.Snow, sut.GetBiomeAt(9999, 9999));
    }
}

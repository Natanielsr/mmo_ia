using GameServerApp.Services.World.MapGenerator;
using GameServerApp.Contracts.Config;
using GameServerApp.Dtos;
using GameServerApp.Services.Combat;
using GameServerApp.Services.Items;
using GameServerApp.Services.Movement;
using GameServerApp.Services.Ranking;
using GameServerApp.Services.Repositories;
using GameServerApp.Services.World;
using GameServerApp.Services.World.WorldFormations;
using GameServerApp.Contracts.Types;
using Microsoft.Extensions.Options;

namespace GameServer.Tests.Services;

/// <summary>
/// Valida que o sistema de biomas usa coordenadas top-left consistentes em toda a stack:
/// WorldBiomeMapService → BiomeSelector → ChunkObjectGenerator / MonsterLifecycleProcessor.
///
/// O bug corrigido: WorldBiomeMapService aplicava offset +halfCols/halfRows,
/// esperando coordenadas de centro-origem, enquanto todo o jogo usa top-left [0, Width).
/// </summary>
public class BiomeCoordinateConsistencyTests
{
    private const int Width     = 32;
    private const int Height    = 32;
    private const int ChunkSize = 16;
    private const int Seed      = 42;

    private static IOptions<WorldConfig> Config() => Options.Create(new WorldConfig
    {
        Map = new MapConfig
        {
            Width            = Width,
            Height           = Height,
            ChunkSize        = ChunkSize,
            GeneratorSeed    = Seed,
            GeneratorFrequency = 0.05f
        }
    });

    private static readonly string BiomesJson = """
        {
          "biomes": [
            { "id": 1, "tagName": "grass",       "name": "Green Field", "groundTilePrefix": "grass",      "groundTileCount": 1, "objects": [],"monsterTags": [], "isDefault": true  },
            { "id": 2, "tagName": "dark_forest",  "name": "Dark Forest", "groundTilePrefix": "dark_grass", "groundTileCount": 1, "objects": [],"monsterTags": [], "isDefault": false },
            { "id": 3, "tagName": "sand",         "name": "Desert",      "groundTilePrefix": "sand",       "groundTileCount": 1, "objects": [],"monsterTags": [], "isDefault": false },
            { "id": 4, "tagName": "snow",         "name": "Snowlands",   "groundTilePrefix": "snow",       "groundTileCount": 1, "objects": [],"monsterTags": [], "isDefault": false }
          ]
        }
        """;

    // ── WorldBiomeMapService ──────────────────────────────────────────────────

    [Fact]
    public void GetBiomeAt_MatchesRawBiomeArray_ForAllTiles()
    {
        // WorldData.Biomes[x, y] é a fonte de verdade; GetBiomeAt deve retornar o mesmo valor.
        var worldSvc = new WorldTerrainService(Config());
        var biomeSvc = new WorldBiomeMapService(Config());

        for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            var expected = worldSvc.WorldData.Biomes[x, y];
            var actual   = biomeSvc.GetBiomeAt(x, y);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void GetBiomeAt_OutOfBoundsNegative_ClampsToTopLeftEdge()
    {
        var svc = new WorldBiomeMapService(Config());
        Assert.Equal(svc.GetBiomeAt(0, 0), svc.GetBiomeAt(-1, -1));
    }

    [Fact]
    public void GetBiomeAt_OutOfBoundsPositive_ClampsToBottomRightEdge()
    {
        var svc = new WorldBiomeMapService(Config());
        Assert.Equal(svc.GetBiomeAt(Width - 1, Height - 1), svc.GetBiomeAt(9999, 9999));
    }

    // ── FractalRiver chunk tiles vs WorldBiomeMapService ─────────────────────

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void ChunkTilesBiomes_MatchGetBiomeAt_ForEachTileInChunk(int cx, int cy)
    {
        // O que o cliente recebe via GetChunkTiles deve ser idêntico ao que o servidor
        // usa para lógica de spawn via GetBiomeAt — ambos em coordenadas top-left.
        var worldSvc = new WorldTerrainService(Config());
        var biomeSvc = new WorldBiomeMapService(Config());

        var (_, biomeRows) = worldSvc.GetChunkTiles(cx, cy, ChunkSize);

        for (int row = 0; row < ChunkSize; row++)
        for (int col = 0; col < ChunkSize; col++)
        {
            int tileX = cx * ChunkSize + col;
            int tileY = cy * ChunkSize + row;

            var fromChunk   = (Biome)biomeRows[row][col];
            var fromService = biomeSvc.GetBiomeAt(tileX, tileY);

            Assert.Equal(fromChunk, fromService);
        }
    }

    // ── BiomeSelector ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0)]
    [InlineData(15, 15)]
    [InlineData(8, 24)]
    [InlineData(31, 31)]
    public void GetBiomeForPosition_TagName_MatchesRawBiomeAtTileXY(int x, int y)
    {
        var worldSvc   = new WorldTerrainService(Config());
        var biomeSvc   = new WorldBiomeMapService(Config());
        var repo       = new BiomeDefinitionRepository(BiomesJson);
        var selector   = new BiomeSelector(repo, biomeSvc, Config());

        var rawBiome      = worldSvc.WorldData.Biomes[x, y];
        var expectedTag   = BiomeEnumToTagName(rawBiome);
        var actualBiome   = selector.GetBiomeForPosition(new Position(x, y));

        Assert.Equal(expectedTag, actualBiome.TagName);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void GetBiomeForChunk_TagName_MatchesRawBiomeAtChunkCenterTile(int cx, int cy)
    {
        var worldSvc = new WorldTerrainService(Config());
        var biomeSvc = new WorldBiomeMapService(Config());
        var repo     = new BiomeDefinitionRepository(BiomesJson);
        var selector = new BiomeSelector(repo, biomeSvc, Config());

        int centerX     = cx * ChunkSize + ChunkSize / 2;
        int centerY     = cy * ChunkSize + ChunkSize / 2;
        var rawBiome    = worldSvc.WorldData.Biomes[centerX, centerY];
        var expectedTag = BiomeEnumToTagName(rawBiome);

        var actual = selector.GetBiomeForChunk(new ChunkCoord(cx, cy));

        Assert.Equal(expectedTag, actual.TagName);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(20, 10)]
    [InlineData(0, 0)]
    public void GetBiomeForTile_And_GetBiomeForPosition_ReturnSame(int x, int y)
    {
        var biomeSvc = new WorldBiomeMapService(Config());
        var repo     = new BiomeDefinitionRepository(BiomesJson);
        var selector = new BiomeSelector(repo, biomeSvc, Config());

        var fromTile = selector.GetBiomeForTile(x, y);
        var fromPos  = selector.GetBiomeForPosition(new Position(x, y));

        Assert.Equal(fromTile.TagName, fromPos.TagName);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string BiomeEnumToTagName(Biome biome) => biome switch
    {
        Biome.Sand       => "sand",
        Biome.Snow       => "snow",
        Biome.DarkForest => "dark_forest",
        _                => "grass"
    };
}

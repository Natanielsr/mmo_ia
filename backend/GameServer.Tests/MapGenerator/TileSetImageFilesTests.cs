using System.Reflection;
using Xunit;

namespace FractalRiver.Tests;

#if RENDERING_TESTS

public class TileSetImageFilesTests
{
    private static string FindImagesRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "images");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            $"Diretório 'images' não encontrado a partir de {AppContext.BaseDirectory}");
    }

    private static Dictionary<TileType, string> GetPrivateStaticDict(Type type, string fieldName)
    {
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<TileType, string>)field!.GetValue(null)!;
    }

    public static IEnumerable<object[]> GrassTileData() =>
        GetPrivateStaticDict(typeof(GrassTileSet), "TileFiles")
            .Select(kv => new object[] { kv.Key, kv.Value });

    public static IEnumerable<object[]> SnowTileData() =>
        GetPrivateStaticDict(typeof(SnowTileSet), "TileFiles")
            .Select(kv => new object[] { kv.Key, kv.Value });

    public static IEnumerable<object[]> SandTileData() =>
        GetPrivateStaticDict(typeof(SandTileSet), "TileFiles")
            .Select(kv => new object[] { kv.Key, kv.Value });

    public static IEnumerable<object[]> SharedTileData() =>
        GetPrivateStaticDict(typeof(BiomeTileSet), "SharedTileFiles")
            .Select(kv => new object[] { kv.Key, kv.Value });

    [Theory]
    [MemberData(nameof(GrassTileData))]
    public void GrassTile_ArquivoExiste(TileType type, string filename)
    {
        string path = Path.Combine(FindImagesRoot(), "grass_biome", filename);
        Assert.True(File.Exists(path), $"{type}: arquivo não encontrado → {path}");
    }

    [Theory]
    [MemberData(nameof(SnowTileData))]
    public void SnowTile_ArquivoExiste(TileType type, string filename)
    {
        string path = Path.Combine(FindImagesRoot(), "snow_biome", filename);
        Assert.True(File.Exists(path), $"{type}: arquivo não encontrado → {path}");
    }

    [Theory]
    [MemberData(nameof(SandTileData))]
    public void SandTile_ArquivoExiste(TileType type, string filename)
    {
        string path = Path.Combine(FindImagesRoot(), "sand_biome", filename);
        Assert.True(File.Exists(path), $"{type}: arquivo não encontrado → {path}");
    }

    [Theory]
    [MemberData(nameof(SharedTileData))]
    public void SharedTile_ArquivoExiste(TileType type, string filename)
    {
        string path = Path.Combine(FindImagesRoot(), filename);
        Assert.True(File.Exists(path), $"{type}: arquivo não encontrado → {path}");
    }
}

#endif

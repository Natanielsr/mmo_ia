using Xunit;

namespace FractalRiver.Tests;

public class ConfigTests
{
    [Fact]
    public void Parse_NoArgs_ReturnsDefaults()
    {
        var cfg = Config.Parse([]);

        Assert.NotNull(cfg);
        // Seed é aleatório por padrão; apenas verifica que não lança
        Assert.Equal("world.png",  cfg.Output);
        Assert.Equal("images/grass_biome",cfg.TilesDir);
        Assert.Equal(100,          cfg.GridCols);
        Assert.Equal(100,          cfg.GridRows);
        Assert.Equal(0.05f,        cfg.NoiseFrequency);
    }

    [Fact]
    public void Parse_Help_ReturnsNull()
    {
        var cfg = Config.Parse(["--help"]);
        Assert.Null(cfg);
    }

    [Theory]
    [InlineData("-s",     "7",   7)]
    [InlineData("--seed", "123", 123)]
    public void Parse_Seed(string flag, string value, int expected)
    {
        var cfg = Config.Parse([flag, value]);
        Assert.Equal(expected, cfg!.Seed);
    }

    [Theory]
    [InlineData("-o",       "out.png")]
    [InlineData("--output", "mapa.png")]
    public void Parse_Output(string flag, string value)
    {
        var cfg = Config.Parse([flag, value]);
        Assert.Equal(value, cfg!.Output);
    }

    [Fact]
    public void Parse_TilesDir()
    {
        var cfg = Config.Parse(["--tiles", "desert_biome"]);
        Assert.Equal("desert_biome", cfg!.TilesDir);
    }

    [Fact]
    public void Parse_GridDimensions()
    {
        var cfg = Config.Parse(["--cols", "200", "--rows", "150"]);
        Assert.Equal(200, cfg!.GridCols);
        Assert.Equal(150, cfg!.GridRows);
    }

    [Theory]
    [InlineData("0.03")]
    [InlineData("0.1")]
    [InlineData("0.01")]
    public void Parse_Frequency_UsesInvariantCulture(string value)
    {
        var cfg = Config.Parse(["--freq", value]);
        Assert.Equal(float.Parse(value, System.Globalization.CultureInfo.InvariantCulture),
                     cfg!.NoiseFrequency);
    }

    [Fact]
    public void Parse_AllArgs_Together()
    {
        string[] args = ["--seed","99","--output","test.png","--tiles","t","--cols","50","--rows","60","--freq","0.02"];
        var cfg = Config.Parse(args);

        Assert.NotNull(cfg);
        Assert.Equal(99,     cfg.Seed);
        Assert.Equal("test.png", cfg.Output);
        Assert.Equal("t",    cfg.TilesDir);
        Assert.Equal(50,     cfg.GridCols);
        Assert.Equal(60,     cfg.GridRows);
        Assert.Equal(0.02f,  cfg.NoiseFrequency, precision: 5);
    }

    [Fact]
    public void Parse_InvalidSeed_Throws()
    {
        Assert.ThrowsAny<Exception>(() => Config.Parse(["--seed", "abc"]));
    }

    [Fact]
    public void Parse_MissingValue_Throws()
    {
        Assert.ThrowsAny<Exception>(() => Config.Parse(["--seed"]));
    }
}

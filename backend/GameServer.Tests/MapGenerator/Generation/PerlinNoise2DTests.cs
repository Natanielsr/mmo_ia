using FractalRiver.Generation;
using Xunit;

namespace FractalRiver.Tests;

public class PerlinNoise2DTests
{
    [Theory]
    [InlineData(  0f,   0f)]
    [InlineData(1.5f, 2.3f)]
    [InlineData(100f, 200f)]
    public void Sample_OutputInRange(float x, float y)
    {
        float v = new PerlinNoise2D(seed: 0).Sample(x, y);
        Assert.InRange(v, -1f, 1f);
    }

    [Fact]
    public void Sample_SameSeed_Deterministic()
    {
        var a = new PerlinNoise2D(seed: 42);
        var b = new PerlinNoise2D(seed: 42);
        Assert.Equal(a.Sample(1.5f, 2.3f), b.Sample(1.5f, 2.3f));
    }

    [Fact]
    public void Sample_DifferentSeeds_DifferentOutput()
    {
        var a = new PerlinNoise2D(seed: 1);
        var b = new PerlinNoise2D(seed: 2);
        Assert.NotEqual(a.Sample(1.5f, 2.3f), b.Sample(1.5f, 2.3f));
    }

    [Fact]
    public void Sample_NearbyPoints_AreSmooth()
    {
        var noise = new PerlinNoise2D(seed: 0);
        float diff = MathF.Abs(noise.Sample(1.0f, 1.0f) - noise.Sample(1.01f, 1.0f));
        Assert.True(diff < 0.1f, $"Pontos próximos devem ter valores próximos, diff={diff:F4}");
    }
}

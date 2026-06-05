namespace FractalRiver.Generation;

internal sealed class PerlinNoise2D
{
    private readonly int[] _p;

    internal PerlinNoise2D(int seed)
    {
        var rng      = new Random(seed);
        var base256  = Enumerable.Range(0, 256).ToArray();

        for (int i = 255; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (base256[i], base256[j]) = (base256[j], base256[i]);
        }

        _p = new int[512];
        for (int i = 0; i < 512; i++) _p[i] = base256[i & 255];
    }

    internal float Sample(float x, float y)
    {
        int   xi = (int)MathF.Floor(x) & 255;
        int   yi = (int)MathF.Floor(y) & 255;
        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = Fade(xf), v = Fade(yf);

        int aa = _p[_p[xi    ] + yi    ];
        int ab = _p[_p[xi    ] + yi + 1];
        int ba = _p[_p[xi + 1] + yi    ];
        int bb = _p[_p[xi + 1] + yi + 1];

        return Lerp(v,
            Lerp(u, Grad(aa, xf,     yf    ),
                    Grad(ba, xf - 1, yf    )),
            Lerp(u, Grad(ab, xf,     yf - 1),
                    Grad(bb, xf - 1, yf - 1)));
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float t, float a, float b) => a + t * (b - a);

    private static float Grad(int hash, float x, float y)
    {
        int   h = hash & 3;
        float u = h < 2 ? x : y;
        float v = h < 2 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}

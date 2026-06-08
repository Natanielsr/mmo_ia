using System.Globalization;

namespace GameServerApp.Services.World.MapGenerator;

public record Config(
    int    Seed,
    string Output,
    string TilesDir,
    int    GridCols,
    int    GridRows,
    float  NoiseFrequency)
{
    public static Config? Parse(string[] args)
    {
        int    seed      = Random.Shared.Next();
        string output    = "world.png";
        string tilesDir  = "images/grass_biome";
        int    gridCols  = 100;
        int    gridRows  = 100;
        float  noiseFreq = 0.05f;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-s" or "--seed":
                    seed = int.Parse(args[++i]);
                    break;
                case "-o" or "--output":
                    output = args[++i];
                    break;
                case "--tiles":
                    tilesDir = args[++i];
                    break;
                case "--cols":
                    gridCols = int.Parse(args[++i]);
                    break;
                case "--rows":
                    gridRows = int.Parse(args[++i]);
                    break;
                case "--freq":
                    noiseFreq = float.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--help":
                    PrintHelp();
                    return null;
            }
        }

        return new Config(seed, output, tilesDir, gridCols, gridRows, noiseFreq);
    }

    private static void PrintHelp() => Console.WriteLine("""
        Mundo Dual-Grid — geração de mapa por ruído de Perlin

        Uso: dotnet run -- [opções]

          -s, --seed   <int>    Semente aleatória           (padrão: 42)
          -o, --output <path>   Arquivo PNG de saída        (padrão: world.png)
              --tiles  <dir>    Diretório com tiles         (padrão: images/grass_biome)
              --cols   <int>    Colunas da grade            (padrão: 100)
              --rows   <int>    Linhas da grade             (padrão: 100)
              --freq   <float>  Frequência do ruído 0.01–0.2 (padrão: 0.05, menor = rios mais longos)
              --help            Exibe esta ajuda

        Exemplos:
          dotnet run
          dotnet run -- --seed 7
          dotnet run -- --seed 7 --cols 150 --rows 150 --freq 0.03 --output ilhas.png
        """);
}

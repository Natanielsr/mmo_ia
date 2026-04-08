namespace GameServerApp.Contracts.Config;

public class WorldConfig
{
    public MapConfig Map { get; set; } = new();
    public MonsterConfig Monsters { get; set; } = new();
    public ObjectConfig Objects { get; set; } = new();
    public CombatConfig Combat { get; set; } = new();
}

public class MapConfig
{
    public int Width { get; set; } = 32;
    public int Height { get; set; } = 32;
    public int ChunkSize { get; set; } = 16;
    public int LoadRadius { get; set; } = 1;
    public int SafeSpawnRadius { get; set; } = 4;
}

public class MonsterConfig
{
    public int MaxGlobal { get; set; } = 50;
    public int PerPlayer { get; set; } = 10;
    public int MinSpawnDistance { get; set; } = 12;
    public int SpawnRadius { get; set; } = 18;
    public int DespawnRadius { get; set; } = 30;
    public double RespawnTimeSec { get; set; } = 2.0;
}

public class ObjectConfig
{
    public int MinPerChunk { get; set; } = 1;
    public int MaxPerChunk { get; set; } = 4;
}

public class CombatConfig
{
    public double AttackSpeedSec { get; set; } = 0.5;
}

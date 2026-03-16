using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using GameServerApp.World;
using GameServerApp.Dtos;

namespace GameServerApp.Managers;

public class MonsterManager : IMonsterManager
{
    private static readonly (string Name, string ObjectCode, int Hp, int Attack)[] MonsterTemplates =
    [
        ("Rat", "rat", 30, 4),
        ("Wolf", "wolf", 60, 10),
        ("Orc", "orc", 90, 14),
        ("Spider", "spider", 45, 7)
    ];

    private readonly Dictionary<long, IMonster> _monsters = new();
    private readonly ICollisionManager _collisionManager;
    private readonly IIdGeneratorService _idGeneratorService;

    public MonsterManager(ICollisionManager collisionManager, IIdGeneratorService idGeneratorService)
    {
        _collisionManager = collisionManager;
        _idGeneratorService = idGeneratorService;
    }

    public IReadOnlyCollection<IMonster> SpawnRandomMonsters(
        int count,
        int width,
        int height,
        int safeSpawnRadius,
        int? seed = null)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (safeSpawnRadius < 0) throw new ArgumentOutOfRangeException(nameof(safeSpawnRadius));

        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        var minX = -(width / 2);
        var maxX = minX + width - 1;
        var minY = -(height / 2);
        var maxY = minY + height - 1;

        var usedPositions = new HashSet<Position>(_monsters.Values.Select(m => m.Position));
        var spawned = new List<IMonster>();
        var attempts = 0;
        var maxAttempts = count * 50;

        while (spawned.Count < count && attempts < maxAttempts)
        {
            attempts++;

            var x = rng.Next(minX, maxX + 1);
            var y = rng.Next(minY, maxY + 1);
            var position = new Position(x, y);

            var insideSafeZone = Math.Abs(x) <= safeSpawnRadius && Math.Abs(y) <= safeSpawnRadius;
            if (insideSafeZone) continue;

            if (usedPositions.Contains(position)) continue;

            if (_collisionManager.IsPositionBlocked(position)) continue;

            var template = MonsterTemplates[rng.Next(MonsterTemplates.Length)];

            // Gera ID e verifica duplicação
            var monsterId = _idGeneratorService.GenerateId();
            if (_monsters.ContainsKey(monsterId))
            {
                throw new InvalidOperationException(
                    $"Duplicate monster ID generated: {monsterId}. " +
                    $"This indicates a serious issue with the ID generator service.");
            }

            var monster = new Monster(
                id: monsterId,
                name: template.Name,
                objectCode: template.ObjectCode,
                spawnPosition: position,
                maxHp: template.Hp,
                attackPower: template.Attack);

            _monsters[monster.Id] = monster;
            _collisionManager.RegisterDynamicObject(monster);

            usedPositions.Add(position);
            spawned.Add(monster);
        }

        return spawned;
    }

    public IReadOnlyCollection<IMonster> GetAllMonsters() => _monsters.Values.ToList();

    public IReadOnlyCollection<MonsterData> GetAllMonstersAsData()
    {
        return _monsters.Values.Select(monster => ToMonsterData(monster)).ToList();
    }

    public IMonster? GetMonsterById(long id)
    {
        return _monsters.TryGetValue(id, out var monster) ? monster : null;
    }

    public bool RemoveMonster(long monsterId)
    {
        if (!_monsters.Remove(monsterId))
        {
            return false;
        }

        _collisionManager.RemoveObject(monsterId);
        return true;
    }

    private MonsterData ToMonsterData(IMonster monster)
    {
        return new MonsterData
        {
            Id = monster.Id.ToString(),
            Name = monster.Name,
            ObjectCode = monster.ObjectCode,
            Position = monster.Position,
            Hp = monster.Hp,
            MaxHp = monster.MaxHp,
            AttackPower = monster.AttackPower,
            IsDead = monster.IsDead
        };
    }

    public MonsterData? GetMonsterDataById(long id)
    {
        return GetMonsterById(id) is { } monster ? ToMonsterData(monster) : null;
    }
}

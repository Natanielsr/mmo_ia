using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Dtos;
using GameServerApp.Services.WorldFormations;
using GameServerApp.World;
using GameServerApp.Contracts.World;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace GameServerApp.Services
{
    public class WorldGenerator : IWorldGenerator
    {
        private readonly IStaticWorldManager _staticWorldManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly IItemManager _itemManager;
        private readonly IWorldEvents _worldEvents;
        private readonly WorldConfig _config;
        private readonly IItemDefinitionRepository _itemRepo;
        private readonly IItemFactory _itemFactory;
        private readonly IBiomeSelector _biomeSelector;
        private readonly List<(IWorldFormation Formation, double Weight)> _formations;

        public WorldGenerator(
            IStaticWorldManager staticWorldManager,
            IIdGeneratorService idGeneratorService,
            IItemManager itemManager,
            IWorldEvents worldEvents,
            IOptions<WorldConfig> config,
            IItemDefinitionRepository itemRepo,
            IItemFactory itemFactory,
            IBiomeSelector biomeSelector)
        {
            _staticWorldManager = staticWorldManager;
            _idGeneratorService = idGeneratorService;
            _itemManager = itemManager;
            _worldEvents = worldEvents;
            _config = config.Value;
            _itemRepo = itemRepo;
            _itemFactory = itemFactory;
            _biomeSelector = biomeSelector;

            // Inicializa as formações disponíveis com seus respectivos pesos/probabilidades
            _formations = new List<(IWorldFormation, double)>
            {
                (new OrganicNoiseFormation(), 0.85), // 85% clareiras/florestas
                (new StoneCircleFormation(), 0.04),  // 4% Stonehenge
                (new RowFormation(), 0.04),         // 4% Pomares/Grades
                (new ClusterFormation(), 0.04),     // 4% Aglomerados densos
                (new MazeFormation(), 0.03)          // 3% Labirintos
            };
        }

        public void GenerateChunk(ChunkCoord coord)
        {
            // Seed determinística para o chunk
            int chunkSeed = HashCode.Combine(coord.CX, coord.CY);
            var rng = new Random(chunkSeed);

            // Coordenadas mundo do início do chunk
            int startX = coord.CX * _config.Map.ChunkSize;
            int startY = coord.CY * _config.Map.ChunkSize;

            // Escolhe uma formação baseada nos pesos
            IWorldFormation selectedFormation = PickFormation(rng);

            selectedFormation.Generate(
                startX,
                startY,
                _config.Map.ChunkSize,
                rng,
                (x, y, type) => SpawnObject(x, y, rng, type, _biomeSelector.GetBiomeForTile(x, y)));
        }

        private IWorldFormation PickFormation(Random rng)
        {
            double roll = rng.NextDouble();
            double cumulative = 0;

            foreach (var item in _formations)
            {
                cumulative += item.Weight;
                if (roll < cumulative)
                {
                    return item.Formation;
                }
            }

            return _formations[0].Formation; // Default
        }

        private void SpawnObject(int x, int y, Random rng, string? forcedType = null, BiomeDefinition? biome = null)
        {
            var pos = new Position(x, y);

            // Items passam direto sem mapeamento de bioma
            if (forcedType != null && forcedType.StartsWith("item:"))
            {
                string tagName = forcedType.Substring(5);
                var def = _itemRepo.GetByTagName(tagName);
                if (def != null)
                {
                    var item = _itemFactory.Create(def, _idGeneratorService.GenerateId().ToString(), pos);
                    _itemManager.DropItem(item);
                    _worldEvents.OnItemDropped(item);
                }
                return;
            }

            if (_staticWorldManager.GetObjectAt(pos) != null) return;

            // Escolhe tipo semântico (null = aleatório dos defaults)
            string[] semanticDefaults = { "tree", "rock", "bush", "pillar" };
            string semanticType = forcedType ?? semanticDefaults[rng.Next(semanticDefaults.Length)];

            // Resolve objectCode via mapa semântico do bioma
            string code = biome != null && biome.SemanticObjectMap.TryGetValue(semanticType, out var mapped)
                ? mapped
                : semanticType;

            string name = char.ToUpper(code[0]) + code.Substring(1);

            var obj = new StaticObject(
                id: _idGeneratorService.GenerateId(),
                position: pos,
                name: name,
                objectCode: code,
                isPassable: false
            );

            _staticWorldManager.AddStaticObject(obj);
        }
    }
}

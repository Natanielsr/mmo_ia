using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.Services.WorldFormations;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace GameServerApp.Services
{
    public class WorldGenerator : IWorldGenerator
    {
        private readonly IStaticWorldManager _staticWorldManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly WorldConfig _config;
        private readonly List<(IWorldFormation Formation, double Weight)> _formations;

        public WorldGenerator(
            IStaticWorldManager staticWorldManager,
            IIdGeneratorService idGeneratorService,
            IOptions<WorldConfig> config)
        {
            _staticWorldManager = staticWorldManager;
            _idGeneratorService = idGeneratorService;
            _config = config.Value;

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
                (x, y, type) => SpawnObject(x, y, rng, type));
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

        private void SpawnObject(int x, int y, Random rng, string forcedType = null)
        {
            var pos = new Position(x, y);
            if (_staticWorldManager.GetObjectAt(pos) != null) return;

            string[] types = { "tree", "rock", "bush", "pillar" };
            string code = forcedType ?? types[rng.Next(types.Length)];
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

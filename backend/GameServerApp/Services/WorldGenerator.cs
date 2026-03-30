using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Services;
using GameServerApp.Contracts.Types;
using GameServerApp.World;
using Microsoft.Extensions.Options;
using System;

namespace GameServerApp.Services
{
    public class WorldGenerator : IWorldGenerator
    {
        private readonly IStaticWorldManager _staticWorldManager;
        private readonly IIdGeneratorService _idGeneratorService;
        private readonly WorldConfig _config;

        public WorldGenerator(
            IStaticWorldManager staticWorldManager,
            IIdGeneratorService idGeneratorService,
            IOptions<WorldConfig> config)
        {
            _staticWorldManager = staticWorldManager;
            _idGeneratorService = idGeneratorService;
            _config = config.Value;
        }

        public void GenerateChunk(ChunkCoord coord)
        {
            // Nota: IsChunkLoaded deve ser verificado antes para evitar regeneração desnecessária
            // mas o StaticWorldManager já lida com isso parcialmente no AddStaticObject (subscreve se na mesma posição)
            // No entanto, para performance, conferimos no WorldManager antes de chamar.

            // Seed determinística baseada na coordenação do chunk para consistência
            int seed = HashCode.Combine(coord.CX, coord.CY);
            var rng = new Random(seed);

            // Coordenadas mundo do início do chunk
            int startX = coord.CX * _config.ChunkSize;
            int startY = coord.CY * _config.ChunkSize;

            // Gera entre 1 e 4 obstáculos por chunk para não encher demais
            int obstacleCount = rng.Next(1, 5);
            for (int i = 0; i < obstacleCount; i++)
            {
                // Posição relativa dentro do chunk
                int rx = rng.Next(0, _config.ChunkSize);
                int ry = rng.Next(0, _config.ChunkSize);
                
                var pos = new Position(startX + rx, startY + ry);

                // Evita spawnar no centro absoluto (0,0) que costuma ser o ponto de partida seguro
                if (pos.X == 0 && pos.Y == 0) continue;

                if (_staticWorldManager.GetObjectAt(pos) != null) continue;

                // Escolhe um tipo de obstáculo baseado nos assets disponíveis no PreloadScene.ts
                string[] types = { "tree", "rock", "bush", "pillar" };
                string code = types[rng.Next(types.Length)];
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
}

using GameServerApp.Contracts.Config;
using GameServerApp.Contracts.Managers;
using GameServerApp.Contracts.Types;
using GameServerApp.Contracts.World;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServerApp.Managers
{
    /// <summary>
    /// Implementação de StaticWorldManager usando chunks para otimização de busca em área
    /// </summary>
    public class StaticWorldManager : IStaticWorldManager
    {
        private readonly HashSet<Position> _blockedPositions = new();
        private readonly Dictionary<Position, IStaticWorldObject> _staticObjects = new();
        private readonly Dictionary<ChunkCoord, List<IStaticWorldObject>> _chunks = new();
        private readonly WorldConfig _config;

        public StaticWorldManager(IOptions<WorldConfig> config)
        {
            _config = config.Value;
        }

        public bool IsBlocked(Position position) => _blockedPositions.Contains(position);

        public bool IsPassable(Position position) => !_blockedPositions.Contains(position);

        public IStaticWorldObject? GetObjectAt(Position position)
        {
            return _staticObjects.TryGetValue(position, out var obj) ? obj : null;
        }

        public bool IsChunkLoaded(ChunkCoord coord) => _chunks.ContainsKey(coord);

        public IEnumerable<IStaticWorldObject> GetChunkObjects(ChunkCoord coord)
        {
            return _chunks.TryGetValue(coord, out var list) ? list : Enumerable.Empty<IStaticWorldObject>();
        }

        public IEnumerable<IStaticWorldObject> GetObjectsInArea(Position topLeft, Position bottomRight)
        {
            // Otimizado para usar chunks
            int minCx = (int)Math.Floor((double)topLeft.X / _config.Map.ChunkSize);
            int maxCx = (int)Math.Floor((double)bottomRight.X / _config.Map.ChunkSize);
            int minCy = (int)Math.Floor((double)bottomRight.Y / _config.Map.ChunkSize);
            int maxCy = (int)Math.Floor((double)topLeft.Y / _config.Map.ChunkSize);

            // Nota: coordY no sistema do jogo costuma subir (Y+ pra cima) ou descer (Y- pra baixo).
            // No MapLoader.ts: worldY = -object.position.y * GRID_SIZE
            // Vamos assumir que topLeft.Y > bottomRight.Y se estivermos usando coordenadas de tela invertidas ou similar.
            // Para garantir:
            var realMinX = Math.Min(topLeft.X, bottomRight.X);
            var realMaxX = Math.Max(topLeft.X, bottomRight.X);
            var realMinY = Math.Min(topLeft.Y, bottomRight.Y);
            var realMaxY = Math.Max(topLeft.Y, bottomRight.Y);

            minCx = (int)Math.Floor((double)realMinX / _config.Map.ChunkSize);
            maxCx = (int)Math.Floor((double)realMaxX / _config.Map.ChunkSize);
            minCy = (int)Math.Floor((double)realMinY / _config.Map.ChunkSize);
            maxCy = (int)Math.Floor((double)realMaxY / _config.Map.ChunkSize);

            for (int cx = minCx; cx <= maxCx; cx++)
            {
                for (int cy = minCy; cy <= maxCy; cy++)
                {
                    var coord = new ChunkCoord(cx, cy);
                    if (_chunks.TryGetValue(coord, out var objects))
                    {
                        foreach (var obj in objects)
                        {
                            if (obj.Position.X >= realMinX && obj.Position.X <= realMaxX &&
                                obj.Position.Y >= realMinY && obj.Position.Y <= realMaxY)
                            {
                                yield return obj;
                            }
                        }
                    }
                }
            }
        }

        public void AddStaticObject(IStaticWorldObject staticObject)
        {
            if (staticObject == null) return;

            var pos = staticObject.Position;
            _staticObjects[pos] = staticObject;

            if (!staticObject.IsPassable)
            {
                _blockedPositions.Add(pos);
            }

            // Adiciona ao chunk
            var cx = (int)Math.Floor((double)pos.X / _config.Map.ChunkSize);
            var cy = (int)Math.Floor((double)pos.Y / _config.Map.ChunkSize);
            var coord = new ChunkCoord(cx, cy);

            if (!_chunks.TryGetValue(coord, out var list))
            {
                list = new List<IStaticWorldObject>();
                _chunks[coord] = list;
            }
            list.Add(staticObject);
        }

        public bool RemoveObjectAt(Position position)
        {
            if (_staticObjects.TryGetValue(position, out var obj))
            {
                _staticObjects.Remove(position);

                if (!obj.IsPassable)
                {
                    _blockedPositions.Remove(position);
                }

                // Remove do chunk
                var cx = (int)Math.Floor((double)position.X / _config.Map.ChunkSize);
                var cy = (int)Math.Floor((double)position.Y / _config.Map.ChunkSize);
                var coord = new ChunkCoord(cx, cy);

                if (_chunks.TryGetValue(coord, out var list))
                {
                    list.Remove(obj);
                    if (list.Count == 0) _chunks.Remove(coord);
                }

                return true;
            }
            return false;
        }

        public void Clear()
        {
            _blockedPositions.Clear();
            _staticObjects.Clear();
            _chunks.Clear();
        }

        public IReadOnlyDictionary<Position, IStaticWorldObject> GetAllObjects() => _staticObjects;
    }
}
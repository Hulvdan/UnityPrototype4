using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public static class ItemTransportationGraph {
    public static List<GraphSegment> BuildGraphSegments(
        List<List<ElementTile>> elementTiles, IMapSize mapSize, List<Building> buildings
    ) {
        var graphSegments = new List<GraphSegment>();

        var cityHall = buildings.Find(
            i => i.scriptableBuilding.type == BuildingType.SpecialCityHall
        );
        if (cityHall == null) {
            return graphSegments;
        }

        var bigFukenQueue = new Queue<Tuple<int, Vector2Int>>();
        bigFukenQueue.Enqueue(new((int)Direction.Right, cityHall.pos));
        bigFukenQueue.Enqueue(new((int)Direction.Up, cityHall.pos));
        bigFukenQueue.Enqueue(new((int)Direction.Left, cityHall.pos));
        bigFukenQueue.Enqueue(new((int)Direction.Down, cityHall.pos));

        var queue = new Queue<Tuple<int, Vector2Int>>();

        var visited = GetVisited(mapSize);

        while (bigFukenQueue.Count > 0) {
            var p = bigFukenQueue.Dequeue();
            queue.Enqueue(p);

            var vertexes = new List<GraphVertex>();
            var segmentTiles = new List<Vector2Int> { p.Item2 };

            while (queue.Count > 0) {
                var (dir, pos) = queue.Dequeue();

                var tile = elementTiles[pos.y][pos.x];
                var isFlag = tile.Type == ElementTileType.Flag;
                var isBuilding = tile.Type == ElementTileType.Building;
                var isCityHall = isBuilding
                                 && tile.Building.scriptableBuilding.type ==
                                 BuildingType.SpecialCityHall;
                if (isFlag || isBuilding) {
                    vertexes.Add(new(new(), pos));
                }

                for (var dirIndex = 0; dirIndex < 4; dirIndex++) {
                    if (isCityHall && dirIndex != dir) {
                        continue;
                    }

                    var offset = DirectionOffsets.Offsets[dirIndex];
                    if (visited[pos.y][pos.x][dirIndex]) {
                        continue;
                    }

                    var newPos = pos + offset;
                    if (!mapSize.Contains(newPos)) {
                        continue;
                    }

                    var oppositeDirIndex = (dirIndex + 2) % 4;
                    if (visited[newPos.y][newPos.x][oppositeDirIndex]) {
                        continue;
                    }

                    var newTile = elementTiles[newPos.y][newPos.x];
                    if (newTile.Type == ElementTileType.None) {
                        continue;
                    }

                    var newIsBuilding = newTile.Type == ElementTileType.Building;
                    if (isBuilding && newIsBuilding) {
                        continue;
                    }

                    var newIsFlag = newTile.Type == ElementTileType.Flag;
                    if (newIsFlag) {
                        bigFukenQueue.Enqueue(new(0, newPos));
                    }

                    visited[pos.y][pos.x][dirIndex] = true;
                    visited[newPos.y][newPos.x][oppositeDirIndex] = true;
                    segmentTiles.Add(newPos);

                    if (newIsBuilding || newIsFlag) {
                        vertexes.Add(new(new(), newPos));
                    }
                    else {
                        queue.Enqueue(new(0, newPos));
                    }
                }
            }

            if (vertexes.Count > 1) {
                graphSegments.Add(new(vertexes, segmentTiles));
            }
        }

        return graphSegments;
    }

    static List<List<bool[]>> GetVisited(IMapSize size) {
        var visited = new List<List<bool[]>>();
        for (var y = 0; y < size.sizeY; y++) {
            var row = new List<bool[]>();
            for (var x = 0; x < size.sizeX; x++) {
                row.Add(new bool[4]);
            }

            visited.Add(row);
        }

        return visited;
    }
}
}

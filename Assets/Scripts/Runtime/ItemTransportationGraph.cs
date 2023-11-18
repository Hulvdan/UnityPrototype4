using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public static class ItemTransportationGraph {
    public static List<GraphSegment> BuildGraphSegments(
        List<List<ElementTile>> elementTiles, IMapSize mapSize, List<Building> buildings
    ) {
        var graphSegments = new List<GraphSegment>();

        var cityHall = buildings.Find(
            i => i.scriptable.type == BuildingType.SpecialCityHall
        );
        Assert.IsNotNull(cityHall);

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
            var graph = new Graph();

            while (queue.Count > 0) {
                var (dir, pos) = queue.Dequeue();

                var tile = elementTiles[pos.y][pos.x];
                var isFlag = tile.Type == ElementTileType.Flag;
                var isBuilding = tile.Type == ElementTileType.Building;
                var isCityHall = isBuilding
                                 && tile.Building.scriptable.type ==
                                 BuildingType.SpecialCityHall;
                if (isFlag || isBuilding) {
                    AddWithoutDuplication(vertexes, pos);
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
                    var newIsFlag = newTile.Type == ElementTileType.Flag;

                    if (isBuilding && newIsBuilding) {
                        continue;
                    }

                    if (newIsFlag) {
                        bigFukenQueue.Enqueue(new(0, newPos));
                    }

                    visited[pos.y][pos.x][dirIndex] = true;
                    visited[newPos.y][newPos.x][oppositeDirIndex] = true;

                    graph.SetDirection(pos, (Direction)dirIndex);
                    graph.SetDirection(newPos, (Direction)oppositeDirIndex);

                    AddWithoutDuplication(segmentTiles, newPos);

                    if (newIsBuilding || newIsFlag) {
                        AddWithoutDuplication(vertexes, newPos);
                    }
                    else {
                        queue.Enqueue(new(0, newPos));
                    }
                }
            }

            if (vertexes.Count > 1) {
                graphSegments.Add(new(vertexes, segmentTiles, graph));
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

    static void AddWithoutDuplication(List<Vector2Int> list, Vector2Int pos) {
        var found = false;
        foreach (var v in list) {
            if (v == pos) {
                found = true;
            }
        }

        if (!found) {
            list.Add(pos);
        }
    }

    static void AddWithoutDuplication(List<GraphVertex> list, Vector2Int pos) {
        var found = false;
        foreach (var v in list) {
            if (v.Pos == pos) {
                found = true;
            }
        }

        if (!found) {
            list.Add(new(new(), pos));
        }
    }
}
}

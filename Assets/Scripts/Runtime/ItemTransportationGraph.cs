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

        var bigFukenQueue = new Queue<Vector2Int>();
        bigFukenQueue.Enqueue(cityHall.pos);

        var queue = new Queue<Vector2Int>();

        while (bigFukenQueue.Count > 0) {
            queue.Enqueue(bigFukenQueue.Dequeue());

            var vertexes = new List<GraphVertex>();
            var segmentTiles = new List<Vector2Int>();

            var visited = GetVisited(mapSize);
            visited[cityHall.posY][cityHall.posX] = true;

            while (queue.Count > 0) {
                var pos = queue.Dequeue();
                visited[pos.y][pos.x] = true;
                segmentTiles.Add(pos);

                var tile = elementTiles[pos.y][pos.x];
                var isFlag = tile.Type == ElementTileType.Flag;
                var isBuilding = tile.Type == ElementTileType.Building;
                if (isFlag || isBuilding) {
                    vertexes.Add(new(new(), pos));
                }

                if (isFlag) {
                    bigFukenQueue.Enqueue(pos);
                    continue;
                }

                var isBuilding_ButNot_CityHall = isBuilding
                                                 && tile.Building.scriptableBuilding.type !=
                                                 BuildingType.SpecialCityHall;
                if (isBuilding_ButNot_CityHall) {
                    continue;
                }

                foreach (var offset in DirectionOffsets.Offsets) {
                    var newPos = pos + offset;
                    if (!mapSize.Contains(newPos)) {
                        continue;
                    }

                    if (visited[newPos.y][newPos.x]) {
                        continue;
                    }

                    var newTile = elementTiles[pos.y][pos.x];
                    if (newTile.Type == ElementTileType.None) {
                        continue;
                    }

                    var newIsBuilding = newTile.Type == ElementTileType.Building;
                    if (isBuilding && newIsBuilding) {
                        continue;
                    }

                    segmentTiles.Add(newPos);

                    queue.Enqueue(newPos);
                }
            }

            if (vertexes.Count <= 1) {
                return new();
            }

            graphSegments.Add(new(vertexes, segmentTiles));
            queue.Clear();
        }

        return graphSegments;
    }

    static List<List<bool>> GetVisited(IMapSize size) {
        var visited = new List<List<bool>>();
        for (var y = 0; y < size.sizeY; y++) {
            var row = new List<bool>();
            for (var x = 0; x < size.sizeX; x++) {
                row.Add(false);
            }

            visited.Add(row);
        }

        return visited;
    }
}
}

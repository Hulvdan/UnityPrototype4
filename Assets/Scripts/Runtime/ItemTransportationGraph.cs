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

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(cityHall.pos);

        var vertexes = new List<GraphVertex>();
        var segmentTiles = new List<Vector2Int>();

        var visited = GetVisited(mapSize);
        visited[cityHall.posY][cityHall.posX] = true;

        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            segmentTiles.Add(pos);

            var tile = elementTiles[pos.y][pos.x];
            if (tile.Type == ElementTileType.Building || tile.Type == ElementTileType.Flag) {
                vertexes.Add(new(new(), cityHall.pos));
            }

            var isBuilding_ButNot_CityHall = tile.Type == ElementTileType.Building
                                             && tile.Building.scriptableBuilding.type !=
                                             BuildingType.SpecialCityHall;
            var isFlag = tile.Type == ElementTileType.Flag;
            if (isBuilding_ButNot_CityHall || isFlag) {
                continue;
            }

            foreach (var offset in DirectionOffsets.Offsets) {
                var newPos = pos + offset;
                if (!mapSize.Contains(newPos) || visited[newPos.y][newPos.x]) {
                    continue;
                }

                visited[newPos.y][newPos.x] = true;
                if (tile.Type == ElementTileType.None) {
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

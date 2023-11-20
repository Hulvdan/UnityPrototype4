using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Core;
using BFG.Graphs;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public enum TileUpdatedType {
    RoadPlaced,
    FlagPlaced,
    FlagRemoved,
    RoadRemoved,
    BuildingPlaced,
    BuildingRemoved,
}

public static class ItemTransportationGraph {
    public static List<GraphSegment> BuildGraphSegments(
        List<List<ElementTile>> elementTiles, IMapSize mapSize, List<Building> buildings
    ) {
        var graphSegments = new List<GraphSegment>();

        var cityHall = buildings.Find(
            i => i.scriptable.type == BuildingType.SpecialCityHall
        );
        Assert.IsNotNull(cityHall);

        var bigFukenQueue = new Queue<Tuple<Direction, Vector2Int>>();
        bigFukenQueue.Enqueue(new(Direction.Right, cityHall.pos));
        bigFukenQueue.Enqueue(new(Direction.Up, cityHall.pos));
        bigFukenQueue.Enqueue(new(Direction.Left, cityHall.pos));
        bigFukenQueue.Enqueue(new(Direction.Down, cityHall.pos));

        var queue = new Queue<Tuple<Direction, Vector2Int>>();

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

                for (Direction dirIndex = 0; dirIndex < (Direction)4; dirIndex++) {
                    if ((isCityHall || isFlag) && dirIndex != dir) {
                        continue;
                    }

                    if (GraphNode.Has(visited[pos.y][pos.x], dirIndex)) {
                        continue;
                    }

                    var newPos = pos + dirIndex.AsOffset();
                    if (!mapSize.Contains(newPos)) {
                        continue;
                    }

                    var oppositeDirIndex = dirIndex.Opposite();
                    if (GraphNode.Has(visited[newPos.y][newPos.x], oppositeDirIndex)) {
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
                        bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    visited[pos.y][pos.x] = GraphNode.MarkAs(visited[pos.y][pos.x], dirIndex);
                    visited[newPos.y][newPos.x] = GraphNode.MarkAs(
                        visited[newPos.y][newPos.x], oppositeDirIndex
                    );
                    graph.SetDirection(pos, dirIndex);
                    graph.SetDirection(newPos, oppositeDirIndex);

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

    /// <remarks>
    ///     Remember to provide all tiles of buildings in case they were placed / removed.
    /// </remarks>
    public static OnTilesUpdatedResult OnTilesUpdated(
        List<List<ElementTile>> elementTiles,
        IMapSize mapSize,
        List<Building> buildings,
        List<Tuple<TileUpdatedType, Vector2Int>> tiles,
        List<GraphSegment> segments
    ) {
        var segmentsToDelete = new List<GraphSegment>();
        foreach (var (updatedType, tilePos) in tiles) {
            foreach (var segment in segments) {
                var segmentShouldBeDeleted = ShouldSegmentBeDeleted(
                    elementTiles,
                    mapSize,
                    updatedType,
                    tilePos,
                    segment
                );
                if (segmentShouldBeDeleted) {
                    AddWithoutDuplication(segmentsToDelete, segment);
                }
            }
        }

        var graphSegments = new List<GraphSegment>();

        var bigFukenQueue = new Queue<Tuple<Direction, Vector2Int>>();
        foreach (var (updatedType, tilePos) in tiles) {
            switch (updatedType) {
                case TileUpdatedType.RoadPlaced:
                case TileUpdatedType.FlagRemoved:
                case TileUpdatedType.FlagPlaced:
                    bigFukenQueue.Enqueue(new(Direction.Right, tilePos));
                    bigFukenQueue.Enqueue(new(Direction.Up, tilePos));
                    bigFukenQueue.Enqueue(new(Direction.Left, tilePos));
                    bigFukenQueue.Enqueue(new(Direction.Down, tilePos));
                    break;
                case TileUpdatedType.RoadRemoved:
                case TileUpdatedType.BuildingPlaced:
                case TileUpdatedType.BuildingRemoved:
                    for (Direction dir = 0; dir < (Direction)4; dir++) {
                        var newPos = tilePos + dir.AsOffset();
                        if (!mapSize.Contains(newPos)) {
                            continue;
                        }

                        if (elementTiles[newPos.y][newPos.x].Type == ElementTileType.None) {
                            continue;
                        }

                        bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var queue = new Queue<Tuple<Direction, Vector2Int>>();

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

                for (Direction dirIndex = 0; dirIndex < (Direction)4; dirIndex++) {
                    if ((isCityHall || isFlag) && dirIndex != dir) {
                        continue;
                    }

                    if (GraphNode.Has(visited[pos.y][pos.x], dirIndex)) {
                        continue;
                    }

                    var newPos = pos + dirIndex.AsOffset();
                    if (!mapSize.Contains(newPos)) {
                        continue;
                    }

                    var oppositeDirIndex = dirIndex.Opposite();
                    if (GraphNode.Has(visited[newPos.y][newPos.x], oppositeDirIndex)) {
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
                        // bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    visited[pos.y][pos.x] = GraphNode.MarkAs(visited[pos.y][pos.x], dirIndex);
                    visited[newPos.y][newPos.x] = GraphNode.MarkAs(
                        visited[newPos.y][newPos.x], oppositeDirIndex
                    );
                    graph.SetDirection(pos, dirIndex);
                    graph.SetDirection(newPos, oppositeDirIndex);

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

        return new() {
            AddedSegments = graphSegments,
            DeletedSegments = segmentsToDelete.ToList(),
        };
    }

    static bool ShouldSegmentBeDeleted(
        IReadOnlyList<List<ElementTile>> elementTiles,
        IMapSize mapSize,
        TileUpdatedType updatedType,
        Vector2Int tilePos,
        GraphSegment segment
    ) {
        switch (updatedType) {
            case TileUpdatedType.RoadPlaced:
            case TileUpdatedType.BuildingPlaced:
                for (Direction dir = 0; dir < (Direction)4; dir++) {
                    var newPos = tilePos + dir.AsOffset();
                    if (!mapSize.Contains(newPos)) {
                        continue;
                    }

                    if (!segment.Graph.ContainsNode(newPos)) {
                        continue;
                    }

                    var tile = elementTiles[newPos.y][newPos.x];
                    if (tile.Type == ElementTileType.Road) {
                        return true;
                    }
                }

                break;
            case TileUpdatedType.FlagPlaced:
            case TileUpdatedType.FlagRemoved:
            case TileUpdatedType.RoadRemoved:
            case TileUpdatedType.BuildingRemoved:
                if (segment.Graph.ContainsNode(tilePos)) {
                    return true;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    static byte[][] GetVisited(IMapSize mapSize) {
        var visited = new byte[mapSize.sizeY][];
        for (var index = 0; index < mapSize.sizeY; index++) {
            visited[index] = new byte[mapSize.sizeX];
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

    static void AddWithoutDuplication(List<GraphSegment> list, GraphSegment segment) {
        var found = false;
        foreach (var v in list) {
            if (ReferenceEquals(v, segment)) {
                found = true;
            }
        }

        if (!found) {
            list.Add(segment);
        }
    }

    public struct OnTilesUpdatedResult {
        public List<GraphSegment> AddedSegments;
        public List<GraphSegment> DeletedSegments;
    }
}
}

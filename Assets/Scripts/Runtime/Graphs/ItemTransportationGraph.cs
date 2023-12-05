using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Graphs {
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
        List<List<ElementTile>> elementTiles,
        IMapSize mapSize,
        List<Building> buildings
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

            var vertices = new List<GraphVertex>();
            var segmentTiles = new List<Vector2Int> { p.Item2 };
            var graph = new Graph();

            while (queue.Count > 0) {
                var (dir, pos) = queue.Dequeue();

                var tile = elementTiles[pos.y][pos.x];
                var isFlag = tile.type == ElementTileType.Flag;
                var isBuilding = tile.type == ElementTileType.Building;
                var isCityHall = isBuilding
                                 && tile.building.scriptable.type == BuildingType.SpecialCityHall;
                if (isFlag || isBuilding) {
                    AddWithoutDuplication(vertices, pos);
                }

                foreach (var dirIndex in Utils.Directions) {
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
                    if (newTile.type == ElementTileType.None) {
                        continue;
                    }

                    var newIsBuilding = newTile.type == ElementTileType.Building;
                    var newIsFlag = newTile.type == ElementTileType.Flag;

                    if (isBuilding && newIsBuilding) {
                        continue;
                    }

                    if (newIsFlag) {
                        bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    visited[pos.y][pos.x] = GraphNode.Mark(visited[pos.y][pos.x], dirIndex);
                    visited[newPos.y][newPos.x] = GraphNode.Mark(
                        visited[newPos.y][newPos.x], oppositeDirIndex
                    );
                    graph.Mark(pos, dirIndex);
                    graph.Mark(newPos, oppositeDirIndex);

                    AddWithoutDuplication(segmentTiles, newPos);

                    if (newIsBuilding || newIsFlag) {
                        AddWithoutDuplication(vertices, newPos);
                    }
                    else {
                        queue.Enqueue(new(0, newPos));
                    }
                }
            }

            if (vertices.Count > 1) {
                graph.FinishBuilding();
                graphSegments.Add(new(vertices, segmentTiles, graph));
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
                    foreach (var dir in Utils.Directions) {
                        var newPos = tilePos + dir.AsOffset();
                        if (!mapSize.Contains(newPos)) {
                            continue;
                        }

                        if (elementTiles[newPos.y][newPos.x].type == ElementTileType.None) {
                            continue;
                        }

                        bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    break;
                case TileUpdatedType.BuildingPlaced:
                    foreach (var dir in Utils.Directions) {
                        var newPos = tilePos + dir.AsOffset();
                        if (!mapSize.Contains(newPos)) {
                            continue;
                        }

                        if (elementTiles[newPos.y][newPos.x].type == ElementTileType.None) {
                            continue;
                        }

                        if (elementTiles[newPos.y][newPos.x].type == ElementTileType.Building) {
                            continue;
                        }

                        if (elementTiles[newPos.y][newPos.x].type == ElementTileType.Flag) {
                            bigFukenQueue.Enqueue(new(dir.Opposite(), newPos));
                        }
                        else {
                            bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                            bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                            bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                            bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                        }
                    }

                    break;
                case TileUpdatedType.BuildingRemoved:
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

            var vertices = new List<GraphVertex>();
            var segmentTiles = new List<Vector2Int> { p.Item2 };
            var graph = new Graph();

            while (queue.Count > 0) {
                var (dir, pos) = queue.Dequeue();

                var tile = elementTiles[pos.y][pos.x];
                var isFlag = tile.type == ElementTileType.Flag;
                var isBuilding = tile.type == ElementTileType.Building;
                var isCityHall = isBuilding
                                 && tile.building.scriptable.type ==
                                 BuildingType.SpecialCityHall;
                if (isFlag || isBuilding) {
                    AddWithoutDuplication(vertices, pos);
                }

                foreach (var dirIndex in Utils.Directions) {
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
                    if (newTile.type == ElementTileType.None) {
                        continue;
                    }

                    var newIsBuilding = newTile.type == ElementTileType.Building;
                    var newIsFlag = newTile.type == ElementTileType.Flag;

                    if (isBuilding && newIsBuilding) {
                        continue;
                    }

                    if (newIsFlag) {
                        // bigFukenQueue.Enqueue(new(Direction.Up, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Right, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Left, newPos));
                        // bigFukenQueue.Enqueue(new(Direction.Down, newPos));
                    }

                    visited[pos.y][pos.x] = GraphNode.Mark(visited[pos.y][pos.x], dirIndex);
                    visited[newPos.y][newPos.x] = GraphNode.Mark(
                        visited[newPos.y][newPos.x], oppositeDirIndex
                    );
                    graph.Mark(pos, dirIndex);
                    graph.Mark(newPos, oppositeDirIndex);

                    AddWithoutDuplication(segmentTiles, newPos);

                    if (newIsBuilding || newIsFlag) {
                        AddWithoutDuplication(vertices, newPos);
                    }
                    else {
                        queue.Enqueue(new(0, newPos));
                    }
                }
            }

            if (vertices.Count > 1) {
                graph.FinishBuilding();
                graphSegments.Add(new(vertices, segmentTiles, graph));
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
                foreach (var dir in Utils.Directions) {
                    var newPos = tilePos + dir.AsOffset();
                    if (!mapSize.Contains(newPos) || !segment.Graph.Contains(newPos)) {
                        continue;
                    }

                    var graphPos = newPos - segment.Graph.offset;
                    if (segment.Graph.nodes[graphPos.y][graphPos.x] == 0) {
                        continue;
                    }

                    var tile = elementTiles[newPos.y][newPos.x];
                    if (tile.type == ElementTileType.Road) {
                        return true;
                    }
                }

                break;
            case TileUpdatedType.FlagPlaced:
            case TileUpdatedType.FlagRemoved:
            case TileUpdatedType.RoadRemoved:
            case TileUpdatedType.BuildingRemoved:
                if (!segment.Graph.Contains(tilePos)) {
                    break;
                }

                var graphPos1 = tilePos - segment.Graph.offset;
                if (segment.Graph.nodes[graphPos1.y][graphPos1.x] == 0) {
                    break;
                }

                return true;

            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    static byte[][] GetVisited(IMapSize mapSize) {
        var visited = new byte[mapSize.height][];
        for (var index = 0; index < mapSize.height; index++) {
            visited[index] = new byte[mapSize.width];
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
            list.Add(new(pos));
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

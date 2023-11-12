using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HorseMovementSystem {
    public readonly Subject<E_TrainReachedDestination> OnReachedDestination = new();

    List<List<MovementGraphTile>> _graph;
    IMapSize _mapSize;

    public bool DebugMode;

    public void Init(IMapSize mapSize, List<List<MovementGraphTile>> movementGraph) {
        _mapSize = mapSize;
        _graph = movementGraph;
    }

    public static void NormalizeNodeDistances(TrainNode node, TrainNode previousNode) {
        node.Progress = previousNode.Progress - previousNode.Width / 2 - node.Width / 2;
        node.SegmentIndex = previousNode.SegmentIndex;
        while (node.Progress < 0) {
            node.Progress += 1;
            node.SegmentIndex -= 1;
        }
    }

    public void AdvanceHorse(HorseTrain horse, float dt) {
        horse.NormalisedSpeed = 1;
        var locomotive = horse.nodes[0];

        locomotive.Progress += dt * horse.Speed;
        while (locomotive.Progress >= 1) {
            locomotive.SegmentIndex += 1;
            locomotive.Progress -= 1;
        }

        var couldReachTheEnd = false;
        if (locomotive.SegmentIndex >= horse.SegmentsCount) {
            locomotive.SegmentIndex = horse.SegmentsCount - 1;
            locomotive.Progress = 1;

            couldReachTheEnd = true;
        }

        var v0 = horse.segmentVertexes[locomotive.SegmentIndex];
        var v1 = horse.segmentVertexes[locomotive.SegmentIndex + 1];
        if (v0 != v1) {
            horse.Direction = DirectionFromTiles(v0, v1);
        }

        if (couldReachTheEnd) {
            var pair = new Tuple<Vector2Int, Vector2Int>(v0, v1);
            if (!Equals(horse.LastReachedSegmentVertexes, pair)) {
                horse.LastReachedSegmentVertexes = pair;

                var destination = horse.CurrentDestination;
                if (destination.HasValue && destination.Value.Pos == v1) {
                    horse.NormalisedSpeed = 0;
                    TrainReachedDestination(horse, destination.Value);
                }
            }
        }

        for (var i = 0; i < horse.nodes.Count - 1; i++) {
            NormalizeNodeDistances(horse.nodes[i + 1], horse.nodes[i]);
        }
    }

    void TrainReachedDestination(HorseTrain train, TrainDestination destination) {
        if (DebugMode) {
            Debug.Log($"TrainReachedDestination {destination}");
        }

        OnReachedDestination.OnNext(new() {
            train = train,
            destination = destination,
        });
    }

    static Direction DirectionFromTiles(Vector2Int source, Vector2Int destination) {
        if (destination.x > source.x) {
            return Direction.Right;
        }

        if (destination.x < source.x) {
            return Direction.Left;
        }

        if (destination.y > source.y) {
            return Direction.Up;
        }

        if (destination.y < source.y) {
            return Direction.Down;
        }

        Debug.LogError("WTF?");
        return Direction.Right;
    }

    public void RecalculateNodePositions(HorseTrain horse) {
        foreach (var node in horse.nodes) {
            var v0 = Math.Min(node.SegmentIndex, horse.segmentVertexes.Count - 1);
            var v1 = Math.Min(node.SegmentIndex + 1, horse.segmentVertexes.Count - 1);

            var vertex0 = horse.segmentVertexes[v0];
            var vertex1 = horse.segmentVertexes[v1];
            node.Position = Vector2.Lerp(vertex0, vertex1, node.Progress);
            var dv = vertex1 - vertex0;

            if (vertex0 != vertex1) {
                node.Rotation = Mathf.Atan2(dv.y, dv.x) * Mathf.Rad2Deg;
            }
        }
    }

    /// <summary>
    ///     Returns a path of cells from source to destination.
    /// </summary>
    /// <remarks>
    ///     If source == destination, returns an empty list.
    ///     If source != destination, returns a list of cells without source, but with destination.
    ///     If could not find path, return Success = false.
    /// </remarks>
    public PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        Direction startingDirection
    ) {
        if (source == destination) {
            return new() {
                Path = new() { Capacity = 0 },
                Success = true,
            };
        }

        foreach (var row in _graph) {
            foreach (var node in row) {
                if (node != null) {
                    node.BFS_Parent = null;
                    node.BFS_Visited = false;
                }
            }
        }

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new(source.x, source.y));
        _graph[source.y][source.x].BFS_Visited = true;

        var isStartingTile = true;
        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            var tile = _graph[pos.y][pos.x];
            if (tile == null) {
                continue;
            }

            for (var i = 0; i < 4; i++) {
                if (!tile.Directions[i]) {
                    continue;
                }

                if (isStartingTile && ((int)startingDirection + 2) % 4 == i) {
                    continue;
                }

                var offset = DirectionOffsets.Offsets[i];
                var newY = pos.y + offset.y;
                var newX = pos.x + offset.x;

                if (!_mapSize.Contains(newX, newY)) {
                    continue;
                }

                var mTile = _graph[newY][newX];
                Assert.IsNotNull(mTile);

                if (mTile.BFS_Visited) {
                    continue;
                }

                var newPos = new Vector2Int(newX, newY);
                VisitTile(ref mTile, pos);
                if (newPos == destination) {
                    return BuildPath(_graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            isStartingTile = false;
        }

        return new(false, null);
    }

    static void VisitTile(ref MovementGraphTile tile, Vector2Int oldPos) {
        tile.BFS_Parent = oldPos;
        tile.BFS_Visited = true;
    }

    static PathFindResult BuildPath(List<List<MovementGraphTile>> graph, Vector2Int destination) {
        var res = new List<Vector2Int> { destination };

        while (graph[destination.y][destination.x].BFS_Parent != null) {
            res.Add(graph[destination.y][destination.x].BFS_Parent.Value);
            destination = graph[destination.y][destination.x].BFS_Parent.Value;
        }

        res.Reverse();
        return new(true, res);
    }

    public void TrySetNextDestinationAndBuildPath(HorseTrain horse) {
        horse.SwitchToTheNextDestination();
        var newDestination = horse.CurrentDestination;
        if (newDestination == null) {
            Debug.LogError("No new destination found");
            return;
        }

        var path = FindPath(horse.segmentVertexes[^1], newDestination.Value.Pos, horse.Direction);

        if (!path.Success) {
            Debug.LogError("Could not find the path");
            return;
        }

        var first = true;
        foreach (var vertex in path.Path) {
            if (first) {
                first = false;
                continue;
            }

            horse.AddSegmentVertex(vertex);
        }
    }
}
}

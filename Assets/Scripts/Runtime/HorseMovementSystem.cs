using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HorseMovementSystem {
    public readonly Subject<OnReachedDestinationData> OnReachedDestination = new();
    Map _map;

    List<List<MovementGraphCell>> _movementGraph;

    float TrainLoadingDuration = 1f;
    float TrainUnloadingDuration = 1f;

    public void Init(Map map, List<List<MovementGraphCell>> movementGraph) {
        _map = map;
        _movementGraph = movementGraph;
    }

    public static void NormalizeNodeDistances(TrainNode node, TrainNode previousNode) {
        node.Progress = previousNode.Progress - previousNode.Width / 2 - node.Width / 2;
        node.SegmentIndex = previousNode.SegmentIndex;
        while (node.Progress < 0) {
            node.Progress += 1;
            node.SegmentIndex -= 1;
        }
    }

    public void AdvanceHorse(HorseTrain horse) {
        var locomotive = horse.nodes[0];

        locomotive.Progress += Time.deltaTime * horse.Speed;
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
        var dir = DirectionFromCells(v0, v1);
        horse.Direction = dir;

        if (couldReachTheEnd) {
            var pair = new Tuple<Vector2Int, Vector2Int>(v0, v1);
            if (!Equals(horse.LastReachedSegmentVertexes, pair)) {
                horse.LastReachedSegmentVertexes = pair;

                var destination = horse.CurrentDestination;
                if (destination.HasValue && destination.Value.Pos == v1) {
                    TrainReachedDestination(horse, destination.Value);
                }
            }
        }

        for (var i = 0; i < horse.nodes.Count - 1; i++) {
            NormalizeNodeDistances(horse.nodes[i + 1], horse.nodes[i]);
        }
    }

    void TrainReachedDestination(HorseTrain train, TrainDestination destination) {
        OnReachedDestination.OnNext(new() {
            train = train,
            destination = destination,
        });
    }

    static Direction DirectionFromCells(Vector2Int source, Vector2Int destination) {
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
            node.CalculatedPosition = Vector2.Lerp(vertex0, vertex1, node.Progress);

            // TODO: Calculate node.Rotation
        }
    }

    /// <summary>
    ///     Returns a list of Vector2Int including starting and ending cells.
    /// </summary>
    public PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        List<List<MovementGraphCell>> graph,
        Direction startingDirection
    ) {
        foreach (var row in graph) {
            foreach (var node in row) {
                if (node != null) {
                    node.BFS_Parent = null;
                    node.BFS_Visited = false;
                }
            }
        }

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new(source.x, source.y));
        graph[source.y][source.x].BFS_Visited = true;

        var isStartingCell = true;
        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            var cell = graph[pos.y][pos.x];
            if (cell == null) {
                continue;
            }

            for (var i = 0; i < 4; i++) {
                if (!cell.Directions[i]) {
                    continue;
                }

                if (isStartingCell && (Direction)i != startingDirection) {
                    continue;
                }

                var offset = DirectionOffsets.Offsets[i];
                var newY = pos.y + offset.y;
                var newX = pos.x + offset.x;

                if (!_map.Contains(newX, newY)) {
                    continue;
                }

                var mCell = graph[newY][newX];
                Assert.IsNotNull(mCell);

                if (mCell.BFS_Visited) {
                    continue;
                }

                var newPos = new Vector2Int(newX, newY);
                VisitCell(ref mCell, pos);
                if (newPos == destination) {
                    return BuildPath(graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            isStartingCell = false;
        }

        return new(false, null);
    }

    static void VisitCell(ref MovementGraphCell cell, Vector2Int oldPos) {
        cell.BFS_Parent = oldPos;
        cell.BFS_Visited = true;
    }

    static PathFindResult BuildPath(List<List<MovementGraphCell>> graph, Vector2Int destination) {
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

        var path = FindPath(
            horse.segmentVertexes[^1], newDestination.Value.Pos, _movementGraph, horse.Direction
        );

        if (!path.Success) {
            Debug.LogError("Could not find the path");
            return;
        }

        foreach (var vertex in path.Path) {
            horse.AddSegmentVertex(vertex);
        }
    }
}
}

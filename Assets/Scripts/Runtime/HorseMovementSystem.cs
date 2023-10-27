using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public struct PathFindResult {
    public bool Success;
    public List<Vector2Int> Path;

    public PathFindResult(bool success, List<Vector2Int> path) {
        Success = success;
        Path = path;
    }
}

public class HorseMovementSystem {
    static readonly Vector2Int[] Offsets = {
        new(1, 0),
        new(0, 1),
        new(-1, 0),
        new(0, -1)
    };

    public event Action<Direction> OnReachedTarget = delegate { };

    public static void NormalizeNodeDistances(TrainNode node, TrainNode previousNode) {
        node.Progress = previousNode.Progress - previousNode.Width / 2 - node.Width / 2;
        node.SegmentIndex = previousNode.SegmentIndex;
        while (node.Progress < 0) {
            node.Progress += 1;
            node.SegmentIndex -= 1;
        }
    }

    public void AdvanceTrain(HorseTrain horse) {
        var locomotive = horse.nodes[0];

        locomotive.Progress += Time.deltaTime * horse.Speed;
        while (locomotive.Progress >= 1) {
            locomotive.SegmentIndex += 1;
            locomotive.Progress -= 1;
        }

        if (locomotive.SegmentIndex >= horse.SegmentsCount) {
            locomotive.SegmentIndex = horse.SegmentsCount - 1;
            locomotive.Progress = 1;

            var source = horse.segmentVertexes[locomotive.SegmentIndex];
            var destination = horse.segmentVertexes[locomotive.SegmentIndex + 1];

            var dir = DirectionFromCells(source, destination);
            OnReachedTarget?.Invoke(dir);
        }

        for (var i = 0; i < horse.nodes.Count - 1; i++) {
            NormalizeNodeDistances(horse.nodes[i + 1], horse.nodes[i]);
        }
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
        ref MovementGraphCell[,] graph,
        Direction startingDirection
    ) {
        foreach (var node in graph) {
            if (node != null) {
                node.BFS_Parent = null;
                node.BFS_Visited = false;
            }
        }

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(source.x, source.y));
        graph[source.y, source.x].BFS_Visited = true;

        var isStartingCell = true;
        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            var cell = graph[pos.y, pos.x];
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

                var offset = Offsets[i];
                var newY = pos.y + offset.y;
                var newX = pos.x + offset.x;

                if (newX < 0
                    || newY < 0
                    || newY >= graph.GetLength(0)
                    || newX >= graph.GetLength(1)) {
                    continue;
                }

                var mCell = graph[newY, newX];
                Assert.IsNotNull(mCell);

                if (mCell.BFS_Visited) {
                    continue;
                }

                var newPos = new Vector2Int(newX, newY);
                VisitCell(ref mCell, pos);
                if (newPos == destination) {
                    return BuildPath(ref graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            isStartingCell = false;
        }

        return new PathFindResult(false, null);
    }

    static void VisitCell(ref MovementGraphCell cell, Vector2Int oldPos) {
        cell.BFS_Parent = oldPos;
        cell.BFS_Visited = true;
    }

    static PathFindResult BuildPath(ref MovementGraphCell[,] graph, Vector2Int destination) {
        var res = new List<Vector2Int> { destination };

        while (graph[destination.y, destination.x].BFS_Parent != null) {
            res.Add(graph[destination.y, destination.x].BFS_Parent.Value);
            destination = graph[destination.y, destination.x].BFS_Parent.Value;
        }

        res.Reverse();
        return new PathFindResult(true, res);
    }
}
}

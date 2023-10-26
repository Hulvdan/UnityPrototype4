using System;
using System.Collections.Generic;
using UnityEngine;

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
        }

        for (var i = 0; i < horse.nodes.Count - 1; i++) {
            NormalizeNodeDistances(horse.nodes[i + 1], horse.nodes[i]);
        }
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

    public PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        ref MovementGraphCell[,] graph
    ) {
        foreach (var node in graph) {
            if (node != null) {
                node.BFS_Parent = null;
                node.BFS_Visited = false;
            }
        }

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(source.y, source.x));
        graph[source.y, source.x].BFS_Visited = true;

        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            var cell = graph[pos.y, pos.x];
            if (cell == null) {
                continue;
            }

            if (cell.Up && !graph[pos.y + 1, pos.x].BFS_Visited) {
                var newPos = new Vector2Int(pos.x, pos.y + 1);
                VisitCell(graph, newPos, pos);
                if (newPos == destination) {
                    return BuildPath(ref graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            if (cell.Down && !graph[pos.y - 1, pos.x].BFS_Visited) {
                var newPos = new Vector2Int(pos.x, pos.y - 1);
                VisitCell(graph, newPos, pos);
                if (newPos == destination) {
                    return BuildPath(ref graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            if (cell.Right && !graph[pos.y, pos.x + 1].BFS_Visited) {
                var newPos = new Vector2Int(pos.x + 1, pos.y);
                VisitCell(graph, newPos, pos);
                if (newPos == destination) {
                    return BuildPath(ref graph, newPos);
                }

                queue.Enqueue(newPos);
            }

            if (cell.Left && !graph[pos.y, pos.x - 1].BFS_Visited) {
                var newPos = new Vector2Int(pos.x - 1, pos.y);
                VisitCell(graph, newPos, pos);
                if (newPos == destination) {
                    return BuildPath(ref graph, newPos);
                }

                queue.Enqueue(newPos);
            }
        }

        return new PathFindResult(false, null);
    }

    static void VisitCell(MovementGraphCell[,] graph, Vector2Int newPos, Vector2Int oldPos) {
        var newCell = graph[newPos.y, newPos.x];
        newCell.BFS_Parent = oldPos;
        newCell.BFS_Visited = true;
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

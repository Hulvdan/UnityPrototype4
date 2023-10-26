using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class Train {
    public readonly float Speed;

    public Train(float speed) {
        Assert.IsTrue(speed >= 0);
        Speed = speed;
    }

    public List<TrainNode> nodes { get; } = new();
    public List<Vector2Int> segmentVertexes { get; } = new();

    public void AddLocomotive(TrainNode node, int segmentIndex, float segmentProgress) {
        nodes.Add(node);
        node.Progress = segmentProgress;
        node.SegmentIndex = segmentIndex;
    }

    public void AddNode(TrainNode node) {
        var lastNode = nodes[^1];
        node.Progress = lastNode.Progress - lastNode.Width / 2 - node.Width / 2;
        node.SegmentIndex = lastNode.SegmentIndex;
        while (node.Progress < 0) {
            node.Progress += 1;
            node.SegmentIndex -= 1;
        }

        nodes.Add(node);
    }

    public void AddSegmentVertex(Vector2Int vertex) {
        segmentVertexes.Add(vertex);
    }

    void PopBackSegmentVertex() {
        segmentVertexes.RemoveAt(0);
        foreach (var node in nodes) {
            node.SegmentIndex -= 1;
        }
    }
}

public class TrainNode {
    public Vector2 CalculatedPosition;
    public float CalculatedRotation;
    public float Progress;
    public int SegmentIndex;
    public float Width;

    public TrainNode(float width) {
        Width = width;
    }
}

public struct PathFindResult {
    public bool Success;
    public List<Vector2Int> Path;

    public PathFindResult(bool success, List<Vector2Int> path) {
        Success = success;
        Path = path;
    }
}

public class HorseMovementSystem {
    public void AdvanceTrain(Train train) {
        var deltaProgress = Time.deltaTime * train.Speed;
        foreach (var node in train.nodes) {
            node.Progress += deltaProgress;
            while (node.Progress >= 1) {
                node.SegmentIndex += 1;
                node.Progress -= 1;
            }
        }
    }

    public void RecalculateNodePositions(Train train) {
        foreach (var node in train.nodes) {
            var v0 = Math.Min(node.SegmentIndex, train.segmentVertexes.Count - 1);
            var v1 = Math.Min(node.SegmentIndex + 1, train.segmentVertexes.Count - 1);

            var vertex0 = train.segmentVertexes[v0];
            var vertex1 = train.segmentVertexes[v1];
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

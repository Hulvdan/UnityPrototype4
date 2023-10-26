using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class TrainNode {
    public Vector2 Position {
        get => Vector2.zero;
        set { }
    }
}

public enum ChainNodeType {
    Horse,
    Cart
}

public class MovementChainNode {
    public MovementChainNode Previous;
    public ChainNodeType Type;
    public float Width = 0.8f; // Max is 1!
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
    float _duration = 1f;
    float _speed = 1f;
    List<TrainNode> _trainNodes;

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

    void Update() {
        foreach (var node in _trainNodes) {
            // node.Position +=asdasdasdasd
        }
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

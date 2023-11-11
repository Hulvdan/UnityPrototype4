using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HorseTrain {
    public Guid ID;
    public readonly float Speed;

    int? _currentDestination;
    public List<TrainDestination> Destinations = new();

    public Direction Direction;

    [CanBeNull]
    public Tuple<Vector2Int, Vector2Int> LastReachedSegmentVertexes;

    public TrainState State = TrainState.Idle;

    public float TrainLoadingUnloadingElapsed;
    public float NormalisedSpeed = 1f;

    public HorseTrain(Guid id, float speed, Direction direction) {
        Assert.IsTrue(speed >= 0);
        ID = id;
        Speed = speed;
        Direction = direction;
    }

    public List<TrainNode> nodes { get; } = new();
    public List<Vector2Int> segmentVertexes { get; } = new();

    public int SegmentsCount => segmentVertexes.Count - 1;

    public TrainDestination? CurrentDestination {
        get {
            if (_currentDestination == null) {
                return null;
            }

            return Destinations[_currentDestination.Value];
        }
    }

    public void SwitchToTheNextDestination() {
        if (_currentDestination == null) {
            _currentDestination = 0;
        }
        else {
            _currentDestination += 1;
        }

        NormalizeDestinationIndex();
    }

    public void AddLocomotive(TrainNode node, int segmentIndex, float segmentProgress) {
        nodes.Add(node);
        node.Progress = segmentProgress;
        node.SegmentIndex = segmentIndex;
    }

    public void AddDestination(TrainDestination destination) {
        Destinations.Add(destination);
    }

    public void RemoveDestination(int index) {
        Destinations.RemoveAt(index);
    }

    public bool IsThereANodeThatCanStoreItem() {
        foreach (var node in nodes) {
            if (node.canStoreResourceCount > node.storedResources.Count) {
                return true;
            }
        }

        return false;
    }

    void NormalizeDestinationIndex() {
        if (Destinations.Count == 0) {
            _currentDestination = null;
            return;
        }

        if (_currentDestination >= Destinations.Count) {
            _currentDestination = 0;
            return;
        }

        if (_currentDestination == null) {
            _currentDestination = 0;
        }
    }

    public void AddWagon(TrainNode node) {
        HorseMovementSystem.NormalizeNodeDistances(node, nodes[^1]);
        nodes.Add(node);
    }

    public void AddSegmentVertex(Vector2Int vertex) {
        // if (segmentVertexes.Count > 0 && segmentVertexes[^1] == vertex) {
        //     Debug.LogError("Vertex duplicated!");
        // }

        segmentVertexes.Add(vertex);
    }

    void PopBackSegmentVertex() {
        segmentVertexes.RemoveAt(0);
        foreach (var node in nodes) {
            node.SegmentIndex -= 1;
        }
    }

    public bool IsThereANodeThatCanUnloadItem() {
        foreach (var node in nodes) {
            if (node.storedResources.Count > 0) {
                return true;
            }
        }

        return false;
    }
}
}

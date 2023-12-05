#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using Foundation.Architecture;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Systems {
public class ResourceTransportation {
    // ReSharper disable once InconsistentNaming
    const int DEV_MAX_ITERATIONS = 256;

    readonly byte[,] _visitedTiles;
    readonly Queue<(Direction, Vector2Int)> _queue = new();
    readonly List<(ResourceToBook, MapResource, List<Vector2Int>)> _foundPairs = new();

    public ResourceTransportation(IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
        _visitedTiles = new byte[_mapSize.height, _mapSize.width];
    }

    public void Add_ResourcesToBook(List<ResourceToBook> resources) {
        using var _ = Tracing.Scope();

        foreach (var resource in resources) {
            _resourcesToBook.Enqueue(resource, resource.Priority);
        }
    }

    #region ResourcesPathfinding

    public void PathfindItemsInQueue() {
        if (_resourcesToBook.Count == 0) {
            return;
        }

        FindPairs();
        foreach (var (resourceToBook, foundResource, path) in _foundPairs) {
            BookResource(resourceToBook, foundResource, path);
        }

        _foundPairs.Clear();
    }

    void FindPairs() {
        var bookedResources = new HashSet<Guid>();

        var iterationWarningEmitted = false;
        var iteration2WarningEmitted = false;

        foreach (var resourceToBook in _resourcesToBook) {
            if (resourceToBook.BookingType != MapResourceBookingType.Construction) {
                continue;
            }

            var destinationPos = resourceToBook.Building.pos;
            foreach (var dir in Utils.Directions) {
                _queue.Enqueue(new(dir, destinationPos));
            }

            var minX = destinationPos.x;
            var maxX = destinationPos.x;
            var minY = destinationPos.y;
            var maxY = destinationPos.y;
            _visitedTiles[destinationPos.y, destinationPos.x] = GraphNode.All;

            MapResource? foundResource = null;

            var iteration = 0;
            while (
                iteration++ < 10 * DEV_MAX_ITERATIONS
                && foundResource == null
                && _queue.Count > 0
            ) {
                var (dir, pos) = _queue.Dequeue();

                var newPos = pos + dir.AsOffset();
                if (!_mapSize.Contains(newPos)) {
                    continue;
                }

                if (GraphNode.Has(_visitedTiles[newPos.y, newPos.x], dir.Opposite())) {
                    continue;
                }

                (minX, maxX) = Utils.MinMax(minX, newPos.x, maxX);
                (minY, maxY) = Utils.MinMax(minY, newPos.y, maxY);
                _visitedTiles[newPos.y, newPos.x] = GraphNode.Mark(
                    _visitedTiles[newPos.y, newPos.x], dir.Opposite()
                );
                _visitedTiles[pos.y, pos.x] = GraphNode.Mark(_visitedTiles[pos.y, pos.x], dir);

                var tile = _map.elementTiles[pos.y][pos.x];
                var newTile = _map.elementTiles[newPos.y][newPos.x];
                if (newTile.type == ElementTileType.Building
                    && newTile.building.scriptable.type != BuildingType.SpecialCityHall) {
                    continue;
                }

                if (tile.type == ElementTileType.Building
                    && newTile.type == ElementTileType.Building) {
                    continue;
                }

                if (tile.type == ElementTileType.None
                    || newTile.type == ElementTileType.None) {
                    continue;
                }

                var elementTile = _map.elementTiles[newPos.y][newPos.x];
                elementTile.BFS_Parent = pos;
                _map.elementTiles[newPos.y][newPos.x] = elementTile;

                var tileResources = _map.mapResources[newPos.y][newPos.x];
                foreach (var tileResource in tileResources) {
                    if (tileResource.Scriptable != resourceToBook.Scriptable) {
                        continue;
                    }

                    if (tileResource.Booking != null || bookedResources.Contains(tileResource.ID)) {
                        continue;
                    }

                    foundResource = tileResource;
                    bookedResources.Add(tileResource.ID);
                    break;
                }

                foreach (var queueDir in Utils.Directions) {
                    if (queueDir == dir.Opposite()) {
                        continue;
                    }

                    if (GraphNode.Has(_visitedTiles[newPos.y, newPos.x], queueDir)) {
                        continue;
                    }

                    _queue.Enqueue(new(queueDir, newPos));
                }
            }

            Assert.IsTrue(iteration < 10 * DEV_MAX_ITERATIONS);
            if (iteration >= DEV_MAX_ITERATIONS && !iterationWarningEmitted) {
                iterationWarningEmitted = true;
                Debug.LogWarning("WTF?");
            }

            _queue.Clear();

            if (foundResource != null) {
                var path = new List<Vector2Int> { foundResource.Pos };
                var destination = foundResource.Pos;

                var iteration2 = 0;
                while (
                    iteration2++ < 10 * DEV_MAX_ITERATIONS
                    && _map.elementTiles[destination.y][destination.x].BFS_Parent != null
                ) {
                    iteration2++;
                    path.Add(_map.elementTiles[destination.y][destination.x].BFS_Parent!.Value);

                    Assert.AreNotEqual(
                        destination,
                        _map.elementTiles[destination.y][destination.x].BFS_Parent!.Value
                    );

                    destination = _map.elementTiles[destination.y][destination.x].BFS_Parent!.Value;
                }

                Assert.IsTrue(iteration2 < 10 * DEV_MAX_ITERATIONS);
                if (iteration2 >= DEV_MAX_ITERATIONS && !iteration2WarningEmitted) {
                    Debug.LogWarning("WTF?");
                    iteration2WarningEmitted = true;
                }

                if (_map.elementTiles[destination.y][destination.x].BFS_Parent == null) {
                    _foundPairs.Add(new(resourceToBook, foundResource, path));
                }
            }

            for (var y = minY; y <= maxY; y++) {
                for (var x = minX; x <= maxX; x++) {
                    _visitedTiles[y, x] = 0;

                    var elementTile = _map.elementTiles[y][x];
                    elementTile.BFS_Parent = null;
                    _map.elementTiles[y][x] = elementTile;
                }
            }
        }
    }

    void BookResource(
        ResourceToBook resourceToBook,
        MapResource res,
        IReadOnlyList<Vector2Int> path
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(
            0, res.TransportationSegments.Count,
            "0 == mapResource.TransportationSegments.Count"
        );
        Assert.AreEqual(
            0, res.TransportationVertices.Count,
            "0 == mapResource.TransportationVertices.Count"
        );
        for (var i = 0; i < path.Count - 1; i++) {
            var a = path[i];
            var b = path[i + 1];

            var dir = Utils.Direction(a, b);
            foreach (var segment in _map.segments) {
                if (!segment.Graph.Contains(a)) {
                    continue;
                }

                var node = segment.Graph.Node(a);
                if (!GraphNode.Has(node, dir)) {
                    continue;
                }

                // Skipping vertices as in this example:
                //     CrrFrrB
                //       rrr
                //
                // We don't wanna see the F (flag) in our vertices list.
                // We need only ending vertex per segment.
                // In this example there should only be B (building) added in the list of vertices
                if (i + 2 <= path.Count - 1) {
                    var c = path[i + 2];
                    if (segment.Graph.Contains(c)) {
                        var nodeC = segment.Graph.Node(c);
                        // Checking if b is an intermediate vertex that should be skipped
                        if (GraphNode.Has(nodeC, Utils.Direction(c, b))) {
                            continue;
                        }
                    }
                }

                foreach (var vertex in segment.Vertices) {
                    if (vertex.Pos == b) {
                        // Hulvdan: TransportationSegments can contain duplicates.
                        // Consider the case:
                        //     CrffrB
                        //      rrrr
                        res.TransportationSegments.Add(segment);
                        res.TransportationVertices.Add(b);
                        break;
                    }
                }
            }
        }

        Assert.AreEqual(
            res.TransportationVertices.Count,
            res.TransportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        foreach (var seg in res.TransportationSegments) {
            Assert.IsFalse(seg.ResourcesToTransport.Contains(res));
            Assert.IsFalse(seg.LinkedResources.Contains(res));
        }

        res.Booking = MapResourceBooking.FromResourceToBook(resourceToBook);
        res.TransportationSegments[0].ResourcesToTransport.Enqueue(res, res.Booking.Value.Priority);

        foreach (var segment in res.TransportationSegments) {
            if (!segment.LinkedResources.Contains(res)) {
                segment.LinkedResources.Add(res);
            }
        }

        _resourcesToBook.Remove(resourceToBook);
    }

    #endregion

    #region Events

    public void OnSegmentDeleted(GraphSegment segment) {
        foreach (var res in segment.LinkedResources) {
            Assert.IsTrue(res.Booking != null, "res.Booking != null");

            var carrier = res.CarryingHuman;
            var targeter = res.TargetedHuman;
            var rebookImmediately = carrier == null;

            // TODO: Experiment with priority to ensure that the first resource
            // the human goes to after placing is this one (if it was booked)
            ClearBooking(res, rebookImmediately, segment);

            if (segment.ResourcesToTransport.Contains(res)) {
                Assert.AreEqual(null, res.CarryingHuman);

                segment.ResourcesToTransport.Remove(res);
                Assert.IsFalse(segment.ResourcesToTransport.Contains(res));
            }

            if (targeter != null) {
                Assert.IsTrue(ReferenceEquals(res, targeter.stateMovingResource_targetedResource));
            }

            if (carrier != null) {
                Assert.IsTrue(ReferenceEquals(carrier, targeter));
                carrier.movingPath.Clear();
            }
            else if (targeter != null) {
                res.TargetedHuman = null;
                targeter.stateMovingResource_targetedResource = null;
            }
        }

        segment.LinkedResources.Clear();
    }

    public void OnHumanStartedPickingUpResource(MapResource res) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(res.Booking, null);

        _map.mapResources[res.Pos.y][res.Pos.x].Remove(res);
    }

    public void OnHumanPlacedResource(Vector2Int pos, GraphSegment? seg, MapResource res) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(
            res.TransportationVertices.Count, res.TransportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        res.CarryingHuman = null;
        res.TargetedHuman = null;

        var placedInsideBuilding = res.Booking != null
                                   && res.Booking.Value.Building.pos == pos;
        bool movedToTheNextSegmentInPath;
        if (res.TransportationVertices.Count > 0) {
            var vertex = res.TransportationVertices[0];
            res.TransportationSegments.RemoveAt(0);
            res.TransportationVertices.RemoveAt(0);

            movedToTheNextSegmentInPath = pos == vertex
                                          && res.TransportationSegments.Count > 0;
        }
        else {
            movedToTheNextSegmentInPath = false;
        }

        res.Pos = pos;

        if (seg != null) {
            seg.LinkedResources.Remove(res);

            if (seg.ResourcesToTransport.Contains(res)) {
                seg.ResourcesToTransport.Remove(res);
                Assert.IsFalse(seg.ResourcesToTransport.Contains(res));
            }
        }

        if (placedInsideBuilding) {
            Tracing.Log("movedInsideBuilding");

            var building = res.Booking!.Value.Building;
            Assert.IsTrue(res.Booking != null, "res.Booking != null");

            ClearBooking(res, false);

            building.ResourcesForConstruction.Add(res);

            DomainEvents<E_ResourcePlacedInsideBuilding>.Publish(new());
        }
        else if (movedToTheNextSegmentInPath) {
            Tracing.Log("movedToTheNextSegmentInPath");

            Assert.AreNotEqual(null, res.Booking, "res.Booking != null");
            Assert.IsFalse(res.TransportationSegments[0].ResourcesToTransport.Contains(res));

            res.TransportationSegments[0]
                .ResourcesToTransport.Enqueue(res, res.Booking!.Value.Priority);
        }
        else {
            Tracing.Log("Resource was placed on the map");

            if (res.Booking != null) {
                ClearBooking(res, true);
            }

            _map.mapResources[res.Pos.y][res.Pos.x].Add(res);
        }
    }

    void ClearBooking(MapResource res, bool needToRebook, GraphSegment? excludedSegment = null) {
        Assert.IsTrue(res.Booking != null);

        var building = res.Booking!.Value.Building;
        if (needToRebook) {
            foreach (var resourceToBook in building.ResourcesToBook) {
                if (resourceToBook.Debug_PreviousResource != null) {
                    Assert.IsFalse(ReferenceEquals(res, resourceToBook.Debug_PreviousResource));
                }
            }

            building.ResourcesToBook.Add(ResourceToBook.FromMapResource(res));
            res.Booking = null;
        }

        foreach (var segment in res.TransportationSegments) {
            if (!ReferenceEquals(segment, excludedSegment)) {
                segment.LinkedResources.Remove(res);

                if (segment.ResourcesToTransport.Contains(res)) {
                    segment.ResourcesToTransport.Remove(res);
                }
            }
        }

        res.TransportationSegments.Clear();
        res.TransportationVertices.Clear();
    }

    #endregion

    readonly IMap _map;
    readonly IMapSize _mapSize;

    readonly SimplePriorityQueue<ResourceToBook> _resourcesToBook = new();
}
}

#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Systems {
public class ResourceTransportation {
    const int _DEV_MAX_ITERATIONS = 256;

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
            _resourcesToBook.Enqueue(resource, resource.priority);
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
            if (resourceToBook.bookingType != MapResourceBookingType.Construction) {
                continue;
            }

            var destinationPos = resourceToBook.building.pos;
            foreach (var dir in Utils.DIRECTIONS) {
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
                iteration++ < 10 * _DEV_MAX_ITERATIONS
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
                elementTile.bfs_parent = pos;
                _map.elementTiles[newPos.y][newPos.x] = elementTile;

                var tileResources = _map.mapResources[newPos.y][newPos.x];
                foreach (var tileResource in tileResources) {
                    if (tileResource.scriptable != resourceToBook.scriptable) {
                        continue;
                    }

                    if (tileResource.booking != null || bookedResources.Contains(tileResource.id)) {
                        continue;
                    }

                    foundResource = tileResource;
                    bookedResources.Add(tileResource.id);
                    break;
                }

                foreach (var queueDir in Utils.DIRECTIONS) {
                    if (queueDir == dir.Opposite()) {
                        continue;
                    }

                    if (GraphNode.Has(_visitedTiles[newPos.y, newPos.x], queueDir)) {
                        continue;
                    }

                    _queue.Enqueue(new(queueDir, newPos));
                }
            }

            Assert.IsTrue(iteration < 10 * _DEV_MAX_ITERATIONS);
            if (iteration >= _DEV_MAX_ITERATIONS && !iterationWarningEmitted) {
                iterationWarningEmitted = true;
                Debug.LogWarning("WTF?");
            }

            _queue.Clear();

            if (foundResource != null) {
                var path = new List<Vector2Int> { foundResource.pos };
                var destination = foundResource.pos;

                var iteration2 = 0;
                while (
                    iteration2++ < 10 * _DEV_MAX_ITERATIONS
                    && _map.elementTiles[destination.y][destination.x].bfs_parent != null
                ) {
                    iteration2++;
                    path.Add(_map.elementTiles[destination.y][destination.x].bfs_parent!.Value);

                    Assert.AreNotEqual(
                        destination,
                        _map.elementTiles[destination.y][destination.x].bfs_parent!.Value
                    );

                    destination = _map.elementTiles[destination.y][destination.x].bfs_parent!.Value;
                }

                Assert.IsTrue(iteration2 < 10 * _DEV_MAX_ITERATIONS);
                if (iteration2 >= _DEV_MAX_ITERATIONS && !iteration2WarningEmitted) {
                    Debug.LogWarning("WTF?");
                    iteration2WarningEmitted = true;
                }

                if (_map.elementTiles[destination.y][destination.x].bfs_parent == null) {
                    _foundPairs.Add(new(resourceToBook, foundResource, path));
                }
            }

            for (var y = minY; y <= maxY; y++) {
                for (var x = minX; x <= maxX; x++) {
                    _visitedTiles[y, x] = 0;

                    var elementTile = _map.elementTiles[y][x];
                    elementTile.bfs_parent = null;
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
            0, res.transportationSegments.Count,
            "0 == mapResource.TransportationSegments.Count"
        );
        Assert.AreEqual(
            0, res.transportationVertices.Count,
            "0 == mapResource.TransportationVertices.Count"
        );
        for (var i = 0; i < path.Count - 1; i++) {
            var a = path[i];
            var b = path[i + 1];

            var dir = Utils.Direction(a, b);
            foreach (var segment in _map.segments) {
                if (!segment.graph.Contains(a)) {
                    continue;
                }

                var node = segment.graph.Node(a);
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
                    if (segment.graph.Contains(c)) {
                        var nodeC = segment.graph.Node(c);
                        // Checking if b is an intermediate vertex that should be skipped
                        if (GraphNode.Has(nodeC, Utils.Direction(c, b))) {
                            continue;
                        }
                    }
                }

                foreach (var vertex in segment.vertices) {
                    if (vertex.pos == b) {
                        // Hulvdan: TransportationSegments can contain duplicates.
                        // Consider the case:
                        //     CrffrB
                        //      rrrr
                        res.transportationSegments.Add(segment);
                        res.transportationVertices.Add(b);
                        break;
                    }
                }
            }
        }

        Assert.AreEqual(
            res.transportationVertices.Count,
            res.transportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        foreach (var seg in res.transportationSegments) {
            Assert.IsFalse(seg.resourcesToTransport.Contains(res));
            Assert.IsFalse(seg.linkedResources.Contains(res));
        }

        res.booking = MapResourceBooking.FromResourceToBook(resourceToBook);
        res.transportationSegments[0].resourcesToTransport.Enqueue(res, res.booking.Value.priority);

        foreach (var segment in res.transportationSegments) {
            if (!segment.linkedResources.Contains(res)) {
                segment.linkedResources.Add(res);
            }
        }

        _resourcesToBook.Remove(resourceToBook);
    }

    #endregion

    #region Events

    public void OnSegmentDeleted(GraphSegment segment) {
        foreach (var res in segment.linkedResources) {
            Assert.IsTrue(res.booking != null, "res.Booking != null");

            var carrier = res.carryingHuman;
            var targeter = res.targetedHuman;
            var rebookImmediately = carrier == null;

            // TODO: Experiment with priority to ensure that the first resource
            // the human goes to after placing is this one (if it was booked)
            ClearBooking(res, rebookImmediately, segment);

            if (segment.resourcesToTransport.Contains(res)) {
                Assert.AreEqual(null, res.carryingHuman);

                segment.resourcesToTransport.Remove(res);
                Assert.IsFalse(segment.resourcesToTransport.Contains(res));
            }

            if (targeter != null) {
                Assert.IsTrue(ReferenceEquals(res, targeter.movingResources_targetedResource));
            }

            if (carrier != null) {
                Assert.IsTrue(ReferenceEquals(carrier, targeter));
                carrier.moving.path.Clear();
            }
            else if (targeter != null) {
                res.targetedHuman = null;
                targeter.movingResources_targetedResource = null;
            }
        }

        segment.linkedResources.Clear();
    }

    public void OnHumanStartedPickingUpResource(MapResource res) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(res.booking, null);

        _map.mapResources[res.pos.y][res.pos.x].Remove(res);
    }

    public void OnHumanPlacedResource(Vector2Int pos, GraphSegment? seg, MapResource res) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(
            res.transportationVertices.Count, res.transportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        res.carryingHuman = null;
        res.targetedHuman = null;

        var placedInsideBuilding = res.booking != null
                                   && res.booking.Value.building.pos == pos;
        bool movedToTheNextSegmentInPath;
        if (res.transportationVertices.Count > 0) {
            var vertex = res.transportationVertices[0];
            res.transportationSegments.RemoveAt(0);
            res.transportationVertices.RemoveAt(0);

            movedToTheNextSegmentInPath = pos == vertex
                                          && res.transportationSegments.Count > 0;
        }
        else {
            movedToTheNextSegmentInPath = false;
        }

        res.pos = pos;

        if (seg != null) {
            seg.linkedResources.Remove(res);

            if (seg.resourcesToTransport.Contains(res)) {
                seg.resourcesToTransport.Remove(res);
                Assert.IsFalse(seg.resourcesToTransport.Contains(res));
            }
        }

        if (placedInsideBuilding) {
            Tracing.Log("movedInsideBuilding");

            var building = res.booking!.Value.building;
            Assert.IsTrue(res.booking != null, "res.Booking != null");

            ClearBooking(res, false);
            building.placedResourcesForConstruction.Add(res);
            _map.OnResourcePlacedInsideBuilding(res, building);
        }
        else if (movedToTheNextSegmentInPath) {
            Tracing.Log("movedToTheNextSegmentInPath");

            Assert.AreNotEqual(null, res.booking, "res.Booking != null");
            Assert.IsFalse(res.transportationSegments[0].resourcesToTransport.Contains(res));

            res.transportationSegments[0]
                .resourcesToTransport.Enqueue(res, res.booking!.Value.priority);
        }
        else {
            Tracing.Log("Resource was placed on the map");

            if (res.booking != null) {
                ClearBooking(res, true);
            }

            _map.mapResources[res.pos.y][res.pos.x].Add(res);
        }
    }

    void ClearBooking(MapResource res, bool needToRebook, GraphSegment? excludedSegment = null) {
        Assert.IsTrue(res.booking != null);

        var building = res.booking!.Value.building;
        if (needToRebook) {
            foreach (var resourceToBook in building.resourcesToBook) {
                if (resourceToBook.debug_previousResource != null) {
                    Assert.IsFalse(ReferenceEquals(res, resourceToBook.debug_previousResource));
                }
            }

            building.resourcesToBook.Add(ResourceToBook.FromMapResource(res));
            res.booking = null;
        }

        foreach (var segment in res.transportationSegments) {
            if (!ReferenceEquals(segment, excludedSegment)) {
                segment.linkedResources.Remove(res);

                if (segment.resourcesToTransport.Contains(res)) {
                    segment.resourcesToTransport.Remove(res);
                }
            }
        }

        res.transportationSegments.Clear();
        res.transportationVertices.Clear();
    }

    #endregion

    readonly IMap _map;
    readonly IMapSize _mapSize;

    readonly SimplePriorityQueue<ResourceToBook> _resourcesToBook = new();
}
}

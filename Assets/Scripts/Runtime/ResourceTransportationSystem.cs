#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class ResourceTransportationSystem {
    // ReSharper disable once InconsistentNaming
    const int DEV_MAX_ITERATIONS = 256;

    readonly byte[,] _visitedTiles;
    readonly Queue<(Direction, Vector2Int)> _queue = new();
    readonly List<(ResourceToBook, MapResource, List<Vector2Int>)> _foundPairs = new();

    public ResourceTransportationSystem(IMap map, IMapSize mapSize) {
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

    public void Remove_ResourcesToBook(List<ResourceToBook> resources) {
        foreach (var resource in resources) {
            _resourcesToBook.Remove(resource);
        }
    }

    public void OnSegmentDeleted(GraphSegment segment) {
        foreach (var resource in segment.linkedResources) {
            Assert.IsTrue(resource.Booking.HasValue);
            var building = resource.Booking.Value.Building;
            building.ResourcesToBook.Add(ResourceToBook.FromMapResource(resource));
        }

        segment.linkedResources.Clear();
    }

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

    public void OnHumanStartedPickingUpResource(MapResource res) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(res.Booking, null);

        _map.mapResources[res.Pos.y][res.Pos.x].Remove(res);
    }

    public void OnHumanPlacedResource(
        Vector2Int pos,
        GraphSegment? seg,
        MapResource res,
        bool segmentWasChanged
    ) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(res.Booking, null, "res.Booking != null");
        Assert.AreEqual(
            res.TransportationVertices.Count, res.TransportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        bool movedToTheNextSegmentInPath;
        bool movedInsideBuilding;
        bool resourceWasPlacedOnMap;

        if (seg == null) {
            Assert.AreEqual(
                res.TransportationVertices.Count, 0, "res.TransportationVertices.Count == 0"
            );
            movedToTheNextSegmentInPath = false;
            movedInsideBuilding = false;
            resourceWasPlacedOnMap = true;
        }
        else {
            Assert.AreNotEqual(
                res.TransportationVertices.Count, 0, "res.TransportationVertices.Count != 0"
            );

            if (pos == res.TransportationVertices[0]) {
                res.TransportationSegments[0]
                    .linkedResources
                    .Remove(res);
            }

            res.Pos = pos;
            var vertex = res.TransportationVertices[0];
            res.TransportationSegments.RemoveAt(0);
            res.TransportationVertices.RemoveAt(0);

            movedToTheNextSegmentInPath = pos == vertex
                                          && res.TransportationSegments.Count > 0;
            movedInsideBuilding = res.Booking != null
                                  && res.Booking.Value.Building.pos == pos;
            resourceWasPlacedOnMap = segmentWasChanged
                                     || (!movedToTheNextSegmentInPath && !movedInsideBuilding);
        }

        if (resourceWasPlacedOnMap) {
            Tracing.Log("Resource was placed on the map");
            if (seg != null) {
                seg.linkedResources.Remove(res);
            }

            foreach (var segment in res.TransportationSegments) {
                segment.linkedResources.Remove(res);
            }

            res.TransportationSegments.Clear();
            res.TransportationVertices.Clear();
            res.Booking = null;

            _map.mapResources[res.Pos.y][res.Pos.x].Add(res);
        }
        else if (movedToTheNextSegmentInPath) {
            Tracing.Log("movedToTheNextSegmentInPath");

            // TODO: Handle duplication of code from ItemTransportationSystem
            res.TransportationSegments[0]
                .resourcesToTransport
                .Enqueue(res);
        }
        else if (movedInsideBuilding) {
            Tracing.Log("movedInsideBuilding");

            var building = res.Booking.Value.Building;
            res.Booking = null;
            building.resourcesForConstruction.Add(res);

            seg.linkedResources.Remove(res);
        }
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
                _visitedTiles[newPos.y, newPos.x] = GraphNode.MarkAs(
                    _visitedTiles[newPos.y, newPos.x], dir.Opposite()
                );
                _visitedTiles[pos.y, pos.x] = GraphNode.MarkAs(_visitedTiles[pos.y, pos.x], dir);

                var tile = _map.elementTiles[pos.y][pos.x];
                var newTile = _map.elementTiles[newPos.y][newPos.x];
                if (tile.Type == ElementTileType.Building
                    && newTile.Type == ElementTileType.Building) {
                    continue;
                }

                if (tile.Type == ElementTileType.None
                    || newTile.Type == ElementTileType.None) {
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
                var path = new List<Vector2Int>();
                var destination = foundResource.Pos;

                var iteration2 = 0;
                while (
                    iteration2++ < 10 * DEV_MAX_ITERATIONS
                    && _map.elementTiles[destination.y][destination.x].BFS_Parent != null
                ) {
                    iteration2++;
                    path.Add(_map.elementTiles[destination.y][destination.x].BFS_Parent.Value);

                    Assert.AreNotEqual(
                        destination,
                        _map.elementTiles[destination.y][destination.x].BFS_Parent.Value
                    );

                    destination = _map.elementTiles[destination.y][destination.x].BFS_Parent.Value;
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
        MapResource mapResource,
        IReadOnlyList<Vector2Int> path
    ) {
        using var _ = Tracing.Scope();

        Assert.IsTrue(mapResource.TransportationSegments.Count == 0);
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

                AddWithoutDuplication(mapResource.TransportationSegments, segment);
                foreach (var vertex in segment.Vertexes) {
                    if (vertex.Pos == b) {
                        mapResource.TransportationVertices.Add(b);
                        break;
                    }
                }
            }
        }

        // Updating booking. Needs to be changed in Map too
        mapResource.Booking = MapResourceBooking.FromResourceToBook(resourceToBook);
        mapResource.TransportationSegments[0].resourcesToTransport.Enqueue(mapResource);

        foreach (var segment in mapResource.TransportationSegments) {
            segment.linkedResources.Add(mapResource);
        }

        var list = _map.mapResources[mapResource.Pos.y][mapResource.Pos.x];
        for (var i = 0; i < list.Count; i++) {
            if (list[i].ID == mapResource.ID) {
                list[i] = mapResource;
                break;
            }
        }

        _resourcesToBook.Remove(resourceToBook);
    }

    void AddWithoutDuplication(List<GraphSegment> segments, GraphSegment segment) {
        foreach (var s in segments) {
            if (s.ID == segment.ID) {
                return;
            }
        }

        segments.Add(segment);
    }

    readonly IMap _map;
    readonly IMapSize _mapSize;

    readonly SimplePriorityQueue<ResourceToBook> _resourcesToBook =
        new();
}
}

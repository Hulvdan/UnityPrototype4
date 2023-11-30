#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using Foundation.Architecture;
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
                _visitedTiles[newPos.y, newPos.x] = GraphNode.MarkAs(
                    _visitedTiles[newPos.y, newPos.x], dir.Opposite()
                );
                _visitedTiles[pos.y, pos.x] = GraphNode.MarkAs(_visitedTiles[pos.y, pos.x], dir);

                var tile = _map.elementTiles[pos.y][pos.x];
                var newTile = _map.elementTiles[newPos.y][newPos.x];
                if (newTile.Type == ElementTileType.Building
                    && newTile.Building.scriptable.type != BuildingType.SpecialCityHall) {
                    continue;
                }

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
        MapResource mapResource,
        IReadOnlyList<Vector2Int> path
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(
            0, mapResource.TransportationSegments.Count,
            "0 == mapResource.TransportationSegments.Count"
        );
        Assert.AreEqual(
            0, mapResource.TransportationVertices.Count,
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

                AddWithoutDuplication(mapResource.TransportationSegments, segment);

                // Skipping vertices as in this example:
                //     CrrFrrB
                //       rrr
                //
                // We don't wanna see this flag in our vertices list.
                // We need only ending vertex per segment.
                // In this example there should only be B added in the list of vertices
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
                        mapResource.TransportationVertices.Add(b);
                        break;
                    }
                }
            }
        }

        Assert.AreEqual(
            mapResource.TransportationVertices.Count,
            mapResource.TransportationSegments.Count,
            "mapResource.TransportationVertices.Count == mapResource.TransportationSegments.Count"
        );

        mapResource.Booking = MapResourceBooking.FromResourceToBook(resourceToBook);
        mapResource.TransportationSegments[0].resourcesToTransport.Enqueue(mapResource);

        foreach (var segment in mapResource.TransportationSegments) {
            if (!segment.linkedResources.Contains(mapResource)) {
                segment.linkedResources.Add(mapResource);
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

    #endregion

    #region Events

    public void OnSegmentDeleted(GraphSegment segment, HumanTransporter? human) {
        foreach (var res in segment.linkedResources) {
            Assert.IsTrue(res.Booking != null, "res.Booking != null");

            if (res.isCarried) {
                Assert.IsTrue(human != null, "human != null");
            }

            if (
                res.isCarried
                && human!.stateMovingResource_targetedResource != null
                && human.stateMovingResource_targetedResource.Equals(res)
            ) {
                human.stateMovingResource_segmentWasChanged = true;
                human.movingPath.Clear();
            }
            else {
                var building = res.Booking!.Value.Building;
                building.ResourcesToBook.Add(ResourceToBook.FromMapResource(res));
                res.TransportationSegments.Clear();
                res.TransportationVertices.Clear();
            }
        }

        segment.linkedResources.Clear();
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

        Assert.AreEqual(
            res.TransportationVertices.Count, res.TransportationSegments.Count,
            "res.TransportationVertices.Count == res.TransportationSegments.Count"
        );

        res.isCarried = false;

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
            seg.linkedResources.Remove(res);
        }

        if (placedInsideBuilding) {
            Tracing.Log("movedInsideBuilding");

            var building = res.Booking!.Value.Building;
            Assert.IsTrue(res.Booking != null);

            ClearBooking(res, false);

            building.resourcesForConstruction.Add(res);

            DomainEvents<E_ResourcePlacedInsideBuilding>.Publish(new() {
                Resource = res, Building = building,
            });
        }
        else if (movedToTheNextSegmentInPath) {
            Tracing.Log("movedToTheNextSegmentInPath");

            res.TransportationSegments[0].resourcesToTransport.Enqueue(res);
        }
        else {
            Tracing.Log("Resource was placed on the map");

            if (res.Booking != null) {
                ClearBooking(res, segmentWasChanged);
            }

            _map.mapResources[res.Pos.y][res.Pos.x].Add(res);
        }
    }

    static void ClearBooking(MapResource res, bool needToRebookResources) {
        var building = res.Booking!.Value.Building;
        if (needToRebookResources) {
            building.ResourcesToBook.Add(ResourceToBook.FromMapResource(res));
        }

        foreach (var segment in res.TransportationSegments) {
            segment.linkedResources.Remove(res);
        }

        res.TransportationSegments.Clear();
        res.TransportationVertices.Clear();
        res.Booking = null;
    }

    #endregion

    readonly IMap _map;
    readonly IMapSize _mapSize;

    readonly SimplePriorityQueue<ResourceToBook> _resourcesToBook = new();
}

public class E_ResourcePlacedInsideBuilding {
    public MapResource Resource;
    public Building Building;
}
}

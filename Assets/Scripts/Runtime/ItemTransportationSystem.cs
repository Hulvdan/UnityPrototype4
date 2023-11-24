﻿using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class ItemTransportationSystem {
    // ReSharper disable once InconsistentNaming
    const int DEV_MAX_ITERATIONS = 256;

    public ItemTransportationSystem(IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
    }

    public void Add_ResourcesToBook(List<ResourceToBook> resources) {
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
        foreach (var resource in segment.resourcesWithThisSegmentInPath) {
            Assert.IsTrue(resource.Booking.HasValue);
            var building = resource.Booking.Value.Building;
            building.ResourcesToBook.Add(ResourceToBook.FromMapResource(resource));
        }

        segment.resourcesWithThisSegmentInPath.Clear();
    }

    public void OnResourceMovedToTheNextSegment() {
    }

    public void PathfindItemsInQueue() {
        if (_resourcesToBook.Count == 0) {
            return;
        }

        var foundPairs = FindPairs();
        foreach (var (resourceToBook, foundResource, path) in foundPairs) {
            BookResource(resourceToBook, foundResource, path);
        }
    }

    List<(ResourceToBook, MapResource, List<Vector2Int>)> FindPairs() {
        var visitedTiles = new byte[_mapSize.height, _mapSize.width];
        var queue = new Queue<Tuple<Direction, Vector2Int>>();

        var foundPairs = new List<(ResourceToBook, MapResource, List<Vector2Int>)>();
        var bookedResources = new HashSet<Guid>();

        var iteration = 0;
        foreach (var resourceToBook in _resourcesToBook) {
            if (resourceToBook.BookingType != MapResourceBookingType.Construction) {
                continue;
            }

            var destinationPos = resourceToBook.Building.pos;
            foreach (var dir in Utils.Directions) {
                queue.Enqueue(new(dir, destinationPos));
            }

            var minX = destinationPos.x;
            var maxX = destinationPos.x;
            var minY = destinationPos.y;
            var maxY = destinationPos.y;
            visitedTiles[destinationPos.y, destinationPos.x] = GraphNode.All;

            MapResource? foundResource = null;
            while (iteration < DEV_MAX_ITERATIONS && foundResource == null && queue.Count > 0) {
                iteration++;
                var (dir, pos) = queue.Dequeue();

                var newPos = pos + dir.AsOffset();
                if (!_mapSize.Contains(newPos)) {
                    continue;
                }

                if (GraphNode.Has(visitedTiles[newPos.y, newPos.x], dir.Opposite())) {
                    continue;
                }

                (minX, maxX) = Utils.MinMax(minX, newPos.x, maxX);
                (minY, maxY) = Utils.MinMax(minY, newPos.y, maxY);
                visitedTiles[newPos.y, newPos.x] = GraphNode.MarkAs(
                    visitedTiles[newPos.y, newPos.x], dir.Opposite()
                );

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

                    if (GraphNode.Has(visitedTiles[newPos.y, newPos.x], queueDir)) {
                        continue;
                    }

                    queue.Enqueue(new(queueDir, newPos));
                }
            }

            Assert.IsTrue(iteration < DEV_MAX_ITERATIONS);

            var iteration2 = 0;
            if (foundResource != null) {
                var path = new List<Vector2Int>();
                var destination = foundResource.Value.Pos;
                while (
                    iteration2 < DEV_MAX_ITERATIONS
                    && _map.elementTiles[destination.y][destination.x].BFS_Parent != null
                ) {
                    iteration2++;
                    path.Add(_map.elementTiles[destination.y][destination.x].BFS_Parent.Value);
                    destination = _map.elementTiles[destination.y][destination.x].BFS_Parent.Value;
                }

                Assert.IsTrue(iteration2 < DEV_MAX_ITERATIONS);

                foundPairs.Add(new(resourceToBook, foundResource.Value, path));
            }

            queue.Clear();
            for (var y = minY; y <= maxY; y++) {
                for (var x = minX; x <= maxX; x++) {
                    visitedTiles[y, x] = 0;

                    var elementTile = _map.elementTiles[y][x];
                    elementTile.BFS_Parent = null;
                    _map.elementTiles[y][x] = elementTile;
                }
            }
        }

        return foundPairs;
    }

    void BookResource(
        ResourceToBook resourceToBook, MapResource mapResource, IReadOnlyList<Vector2Int> path
    ) {
        Assert.IsTrue(mapResource.TravellingSegments.Count == 0);
        mapResource.TravellingSegments.Clear();
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

                if (AddWithoutDuplication(mapResource.TravellingSegments, segment)) {
                    foreach (var vertex in segment.Vertexes) {
                        if (vertex.Pos == b) {
                            mapResource.ItemMovingVertices.Add(b);
                        }
                    }
                }
            }
        }

        foreach (var segment in mapResource.TravellingSegments) {
            segment.resourcesWithThisSegmentInPath.Add(mapResource);
        }

        // Updating booking. Needs to be changed in Map too
        mapResource.Booking = MapResourceBooking.FromResourceToBook(resourceToBook);
        mapResource.TravellingSegments[0].resourcesReadyToBeTransported.Enqueue(mapResource);

        var list = _map.mapResources[mapResource.Pos.y][mapResource.Pos.x];
        for (var i = 0; i < list.Count; i++) {
            if (list[i].Equals(mapResource)) {
                list[i] = mapResource;
                break;
            }
        }

        _resourcesToBook.Remove(resourceToBook);
    }

    bool AddWithoutDuplication(List<GraphSegment> segments, GraphSegment segment) {
        foreach (var s in segments) {
            if (s.ID == segment.ID) {
                return false;
            }
        }

        segments.Add(segment);
        return true;
    }

    readonly IMap _map;
    readonly IMapSize _mapSize;

    readonly SimplePriorityQueue<ResourceToBook> _resourcesToBook = new();
}
}
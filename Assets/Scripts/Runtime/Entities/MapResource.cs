#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Runtime.Graphs;
using UnityEngine;

namespace BFG.Runtime.Entities {
public class MapResource : IEquatable<MapResource> {
    public readonly Guid id;
    public readonly ScriptableResource scriptable;

    public Vector2Int pos;
    public MapResourceType type;

    public MapResourceBooking? booking;

    public readonly List<GraphSegment> transportationSegments;
    public readonly UniqueList<Vector2Int> transportationVertices;

    public Human? targetedHuman;
    public Human? carryingHuman;

    public MapResource(
        Vector2Int pos_,
        ScriptableResource scriptable_,
        MapResourceType type_ = MapResourceType.CityHallItem
    ) {
        id = Guid.NewGuid();
        pos = pos_;
        scriptable = scriptable_;
        booking = null;
        type = type_;
        transportationSegments = new();
        transportationVertices = new();
    }

    public bool Equals(MapResource other) {
        return id.Equals(other.id);
    }

    public override bool Equals(object? obj) {
        return obj is MapResource other && Equals(other);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }
}
}

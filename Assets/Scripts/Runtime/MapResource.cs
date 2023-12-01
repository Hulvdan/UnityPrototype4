#nullable enable
using System;
using System.Collections.Generic;
using BFG.Core;
using UnityEngine;

namespace BFG.Runtime {
public class MapResource : IEquatable<MapResource> {
    public readonly Guid ID;
    public readonly ScriptableResource Scriptable;

    public Vector2Int Pos;
    public MapResourceType Type;

    public MapResourceBooking? Booking;
    public readonly UniqueList<GraphSegment> TransportationSegments;
    public readonly UniqueList<Vector2Int> TransportationVertices;

    public HumanTransporter? TargetedHuman;
    public HumanTransporter? CarryingHuman;

    public MapResource(
        Vector2Int pos,
        ScriptableResource scriptable,
        MapResourceType type = MapResourceType.CityHallItem
    ) {
        ID = Guid.NewGuid();
        Pos = pos;
        Scriptable = scriptable;
        Booking = null;
        Type = type;
        TransportationSegments = new();
        TransportationVertices = new();
    }

    public bool Equals(MapResource other) {
        return ID.Equals(other.ID);
    }

    public override bool Equals(object? obj) {
        return obj is MapResource other && Equals(other);
    }

    public override int GetHashCode() {
        return ID.GetHashCode();
    }
}
}

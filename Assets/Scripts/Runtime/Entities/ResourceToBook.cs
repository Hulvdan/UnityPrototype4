#nullable enable
using System;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public struct ResourceToBook : IEquatable<ResourceToBook> {
    public Guid id;
    public ScriptableResource scriptable;
    public int priority;

    public MapResourceBookingType bookingType;
    public Building building;

    public MapResource? debug_previousResource;

    public static ResourceToBook FromMapResource(MapResource res) {
        Assert.IsTrue(res.booking != null);
        var booking = res.booking.Value;
        return new() {
            id = Guid.NewGuid(),
            scriptable = res.scriptable,
            priority = booking.priority,
            bookingType = booking.type,
            building = booking.building,
            debug_previousResource = res,
        };
    }

    public bool Equals(ResourceToBook other) {
        return id.Equals(other.id);
    }

    public override bool Equals(object? obj) {
        return obj is ResourceToBook other && Equals(other);
    }

    public override int GetHashCode() {
        return id.GetHashCode();
    }
}
}

#nullable enable
using System;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public struct ResourceToBook : IEquatable<ResourceToBook> {
    public Guid ID;
    public ScriptableResource Scriptable;
    public int Priority;

    public MapResourceBookingType BookingType;
    public Building Building;

    public static ResourceToBook FromMapResource(MapResource resource) {
        Assert.IsTrue(resource.Booking != null);
        var booking = resource.Booking.Value;
        return new() {
            ID = Guid.NewGuid(),
            Scriptable = resource.Scriptable,
            Priority = booking.Priority,
            BookingType = booking.Type,
            Building = booking.Building,
        };
    }

    public bool Equals(ResourceToBook other) {
        return ID.Equals(other.ID);
    }

    public override bool Equals(object? obj) {
        return obj is ResourceToBook other && Equals(other);
    }

    public override int GetHashCode() {
        return ID.GetHashCode();
    }
}
}

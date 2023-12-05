#nullable enable
using System;
using BFG.Runtime.Entities;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public struct ResourceToBook : IEquatable<ResourceToBook> {
    public Guid ID;
    public ScriptableResource Scriptable;
    public int Priority;

    public MapResourceBookingType BookingType;
    public Building Building;

    public MapResource? Debug_PreviousResource;

    public static ResourceToBook FromMapResource(MapResource res) {
        Assert.IsTrue(res.Booking != null);
        var booking = res.Booking.Value;
        return new() {
            ID = Guid.NewGuid(),
            Scriptable = res.Scriptable,
            Priority = booking.Priority,
            BookingType = booking.Type,
            Building = booking.Building,
            Debug_PreviousResource = res,
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

using System;

namespace BFG.Runtime.Entities {
public class ResourceObj : IEquatable<ResourceObj> {
    public ResourceObj(Guid id, ScriptableResource script) {
        this.id = id;
        this.script = script;
    }

    public ScriptableResource script { get; }
    public Guid id { get; }

    public static bool operator ==(ResourceObj obj1, ResourceObj obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return true;
        }

        if (ReferenceEquals(null, obj1)) {
            return false;
        }

        return obj1.Equals(obj2);
    }

    public static bool operator !=(ResourceObj obj1, ResourceObj obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return false;
        }

        if (ReferenceEquals(null, obj1)) {
            return true;
        }

        return !obj1.Equals(obj2);
    }

    public bool Equals(ResourceObj other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return id.Equals(other.id);
    }
}
}

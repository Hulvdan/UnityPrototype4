using System;

namespace BFG.Runtime {
public class ResourceObj {
    public ResourceObj(Guid id, ScriptableResource script) {
        this.id = id;
        this.script = script;
    }

    public ScriptableResource script { get; }
    public Guid id { get; }
}
}

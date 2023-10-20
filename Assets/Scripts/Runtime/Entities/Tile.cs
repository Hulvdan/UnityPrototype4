using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public string Name;

    [CanBeNull]
    public ScriptableResource resource;
}
}

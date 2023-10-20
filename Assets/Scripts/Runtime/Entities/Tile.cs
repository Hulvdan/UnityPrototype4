using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public string Name;
    public bool isBooked;

    [CanBeNull]
    public ScriptableResource resource;
}
}

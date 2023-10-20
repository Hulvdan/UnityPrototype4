using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public bool isBooked;
    public string Name;

    [CanBeNull]
    public ScriptableResource resource;
}
}

using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public string Name;
    public int ResourceAmount;

    [CanBeNull]
    public ScriptableResource Resource;

    public bool IsBooked;
}
}

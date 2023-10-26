using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public bool IsBooked;
    public string Name;

    [CanBeNull]
    public ScriptableResource Resource;

    public int ResourceAmount;
}
}

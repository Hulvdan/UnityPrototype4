using JetBrains.Annotations;

namespace BFG.Runtime {
/// <summary>
///     Is it a grass, a cliff?
/// </summary>
public class TerrainTile {
    public int Height;
    public bool IsBooked;
    public string Name;

    [CanBeNull]
    public ScriptableResource Resource;

    public int ResourceAmount;
}
}

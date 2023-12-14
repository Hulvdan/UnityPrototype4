using JetBrains.Annotations;

namespace BFG.Runtime.Entities {
/// <summary>
///     Is it a grass, a cliff?
/// </summary>
public class TerrainTile {
    public int height;
    public bool isBooked;
    public string name;

    [CanBeNull]
    public ScriptableResource resource;

    public int resourceAmount;
}
}

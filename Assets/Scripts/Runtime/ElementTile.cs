using UnityEngine;

namespace BFG.Runtime {
/// <summary>
///     Is it a road or lumberjack's hut?
/// </summary>
public struct ElementTile {
    public ElementTileType Type;
    public int Rotation;

    public ElementTile(ElementTileType type, int rotation) {
        if ((type == ElementTileType.Road || type == ElementTileType.None) && rotation != 0) {
            Debug.LogError("WTF IS GOING ON HERE?");
            rotation = 0;
        }

        Type = type;
        Rotation = rotation;
    }

    public static ElementTile None = new(ElementTileType.None, 0);
    public static ElementTile Road = new(ElementTileType.Road, 0);
}
}

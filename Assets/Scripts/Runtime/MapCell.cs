using UnityEngine;

namespace BFG.Runtime {
public struct MapCell {
    public CellType Type;
    public int Rotation;

    public MapCell(CellType type, int rotation = 0) {
        if ((type == CellType.Road || type == CellType.None) && rotation != 0) {
            Debug.LogError("WTF IS GOING ON HERE?");
            rotation = 0;
        }

        Type = type;
        Rotation = rotation;
    }

    public static MapCell None = new(CellType.None);
    public static MapCell Road = new(CellType.Road);
}
}

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
/// <summary>
///     Is it a road or lumberjack's hut?
/// </summary>
public struct ElementTile {
    public ElementTileType Type;
    public int Rotation;

    [CanBeNull]
    public Building Building;

    // TODO: Remove this code
    public ElementTile(ElementTileType type, int rotation) {
        if ((type == ElementTileType.Road || type == ElementTileType.None) && rotation != 0) {
            Debug.LogError("WTF IS GOING ON HERE?");
            rotation = 0;
        }

        Type = type;
        Rotation = rotation;
        Building = null;
    }

    public ElementTile(ElementTileType type, [CanBeNull] Building building) {
        Assert.IsTrue(
            type is ElementTileType.Building
                or ElementTileType.Road
                or ElementTileType.Flag
                or ElementTileType.None
        );

        if (type == ElementTileType.Building) {
            Assert.IsNotNull(building);
        }
        else {
            Assert.IsNull(building);
        }

        Type = type;
        Rotation = 0;
        Building = building;
    }

    public static ElementTile None = new(ElementTileType.None, null);
    public static ElementTile Road = new(ElementTileType.Road, null);
    public static ElementTile Flag = new(ElementTileType.Flag, null);

    public override string ToString() {
        if (Type == ElementTileType.Building) {
            Assert.IsNotNull(Building);
            return $"ElementTile({Type}, {Building.scriptableBuilding.name})";
        }

        return $"ElementTile({Type})";
    }
}
}

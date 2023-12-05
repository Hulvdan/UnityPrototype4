using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
/// <summary>
///     Is it a road or lumberjack's hut?
/// </summary>
public struct ElementTile {
    public ElementTileType type { get; set; }

    [CanBeNull]
    public Building building { get; }

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

        this.type = type;
        this.building = building;

        BFS_Visited = false;
        BFS_Parent = null;
    }

    ElementTile(ElementTileType type) {
        building = null;
        this.type = type;

        BFS_Visited = false;
        BFS_Parent = null;
    }

    public static readonly ElementTile None = new(ElementTileType.None);
    public static readonly ElementTile Road = new(ElementTileType.Road);
    public static readonly ElementTile Flag = new(ElementTileType.Flag);

    public Vector2Int? BFS_Parent { get; set; }
    public bool BFS_Visited { get; set; }

    public override string ToString() {
        if (type == ElementTileType.Building) {
            Assert.IsNotNull(building);
            return $"ElementTile({type}, {building!.scriptable.name})";
        }

        return $"ElementTile({type})";
    }
}
}

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
/// <summary>
///     Is it a road, a flag, or a lumberjack's hut?
/// </summary>
public struct ElementTile {
    public ElementTileType type { get; set; }

    [CanBeNull]
    public Building building { get; }

    public ElementTile(ElementTileType type_, [CanBeNull] Building building_) {
        Assert.IsTrue(
            type_ is ElementTileType.Building
                or ElementTileType.Road
                or ElementTileType.Flag
                or ElementTileType.None
        );

        if (type_ == ElementTileType.Building) {
            Assert.IsNotNull(building_);
        }
        else {
            Assert.IsNull(building_);
        }

        type = type_;
        building = building_;

        bfs_visited = false;
        bfs_parent = null;
    }

    ElementTile(ElementTileType type_) {
        building = null;
        type = type_;

        bfs_visited = false;
        bfs_parent = null;
    }

    public static readonly ElementTile NONE = new(ElementTileType.None);
    public static readonly ElementTile ROAD = new(ElementTileType.Road);
    public static readonly ElementTile FLAG = new(ElementTileType.Flag);

    public Vector2Int? bfs_parent { get; set; }
    public bool bfs_visited { get; set; }

    public override string ToString() {
        if (type == ElementTileType.Building) {
            Assert.IsNotNull(building);
            return $"ElementTile({type}, {building!.scriptable.name})";
        }

        return $"ElementTile({type})";
    }
}
}

#nullable enable
using UnityEngine;

namespace BFG.Runtime.Graphs {
public class MovementGraphTile {
    /// <summary>
    ///     Right, Up, Left, Down
    /// </summary>
    public readonly bool[] directions = new bool[4];

    // ReSharper disable once InconsistentNaming
    public Vector2Int? BFS_Parent;

    // ReSharper disable once InconsistentNaming
    public bool BFS_Visited;

    public MovementGraphTile(bool right, bool up, bool left, bool down) {
        directions[0] = right;
        directions[1] = up;
        directions[2] = left;
        directions[3] = down;
    }

    public int Count() {
        var i = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var d in directions) {
            if (d) {
                i++;
            }
        }

        return i;
    }

    public bool TwoTypeIsVertical() {
        return (directions[0] && directions[2]) || (directions[1] && directions[3]);
    }

    public int Rotation() {
        switch (Count()) {
            case 0:
                return 0;
            case 1:
                if (directions[0]) {
                    return 0;
                }

                if (directions[1]) {
                    return 1;
                }

                if (directions[2]) {
                    return 2;
                }

                if (directions[3]) {
                    return 3;
                }

                break;
            case 2:
                if (TwoTypeIsVertical()) {
                    return directions[0] ? 0 : 1;
                }

                if (directions[0] && directions[1]) {
                    return 0;
                }

                if (directions[1] && directions[2]) {
                    return 1;
                }

                if (directions[2] && directions[3]) {
                    return 2;
                }

                if (directions[3] && directions[0]) {
                    return 3;
                }

                break;

            case 3:
                if (!directions[3]) {
                    return 0;
                }

                if (!directions[0]) {
                    return 1;
                }

                if (!directions[1]) {
                    return 2;
                }

                if (!directions[2]) {
                    return 3;
                }

                break;
            case 4:
                return 0;
            default:
                Debug.LogError("What the fuck is going on here?");
                break;
        }

        Debug.LogError("What the fuck is going on here?");
        return -1;
    }

    public static MovementGraphTile MakeUpRight() {
        return new(true, true, false, false);
    }

    public static MovementGraphTile MakeUpLeft() {
        return new(false, true, true, false);
    }

    public static MovementGraphTile MakeLeftRight() {
        return new(true, false, true, false);
    }

    public static MovementGraphTile MakeDownRight() {
        return new(true, false, false, true);
    }

    public static MovementGraphTile MakeDownLeft() {
        return new(false, false, true, true);
    }

    public static MovementGraphTile MakeUpDown() {
        return new(false, true, false, true);
    }

    public static MovementGraphTile MakeUpDownRight() {
        return new(true, true, false, true);
    }
}
}

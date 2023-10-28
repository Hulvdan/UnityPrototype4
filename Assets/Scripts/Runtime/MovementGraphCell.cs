#nullable enable
using UnityEngine;

namespace BFG.Runtime {
public class MovementGraphCell {
    /// <summary>
    ///     Right, Up, Left, Down
    /// </summary>
    public readonly bool[] Directions = new bool[4];

    // ReSharper disable once InconsistentNaming
    public Vector2Int? BFS_Parent;

    // ReSharper disable once InconsistentNaming
    public bool BFS_Visited;

    public MovementGraphCell(bool right, bool up, bool left, bool down) {
        Directions[0] = right;
        Directions[1] = up;
        Directions[2] = left;
        Directions[3] = down;
    }

    public int Count() {
        var i = 0;
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var d in Directions) {
            if (d) {
                i++;
            }
        }

        return i;
    }

    public bool TwoTypeIsVertical() {
        return (Directions[0] && Directions[2]) || (Directions[1] && Directions[3]);
    }

    public int Rotation() {
        switch (Count()) {
            case 0:
                return 0;
            case 1:
                if (Directions[0]) {
                    return 0;
                }

                if (Directions[1]) {
                    return 1;
                }

                if (Directions[2]) {
                    return 2;
                }

                if (Directions[3]) {
                    return 3;
                }

                break;
            case 2:
                if (TwoTypeIsVertical()) {
                    return Directions[0] ? 0 : 1;
                }

                if (Directions[0] && Directions[1]) {
                    return 0;
                }

                if (Directions[1] && Directions[2]) {
                    return 1;
                }

                if (Directions[2] && Directions[3]) {
                    return 2;
                }

                if (Directions[3] && Directions[0]) {
                    return 3;
                }

                break;

            case 3:
                if (!Directions[3]) {
                    return 0;
                }

                if (!Directions[0]) {
                    return 1;
                }

                if (!Directions[1]) {
                    return 2;
                }

                if (!Directions[2]) {
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

    public static MovementGraphCell MakeUpRight() {
        return new(true, true, false, false);
    }

    public static MovementGraphCell MakeUpLeft() {
        return new(false, true, true, false);
    }

    public static MovementGraphCell MakeLeftRight() {
        return new(true, false, true, false);
    }

    public static MovementGraphCell MakeDownRight() {
        return new(true, false, false, true);
    }

    public static MovementGraphCell MakeDownLeft() {
        return new(false, false, true, true);
    }

    public static MovementGraphCell MakeUpDown() {
        return new(false, true, false, true);
    }

    public static MovementGraphCell MakeUpDownRight() {
        return new(true, true, false, true);
    }
}
}

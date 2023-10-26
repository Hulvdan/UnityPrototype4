using UnityEngine;

namespace BFG.Runtime {
public class MovementGraphCell {
    // ReSharper disable once InconsistentNaming
    public Vector2Int? BFS_Parent;

    // ReSharper disable once InconsistentNaming
    public bool BFS_Visited;

    /// <summary>
    ///     Right, Up, Left, Down
    /// </summary>
    public bool[] Directions;

    public MovementGraphCell(bool right, bool up, bool left, bool down) {
        Directions = new bool[4];
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
}
}

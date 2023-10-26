using UnityEngine;

namespace BFG.Runtime {
public class MovementGraphCell {
    // ReSharper disable once InconsistentNaming
    public Vector2Int? BFS_Parent;

    // ReSharper disable once InconsistentNaming
    public bool BFS_Visited;
    public bool Down;
    public bool Left;
    public bool Right;
    public bool Up;

    public MovementGraphCell(bool up, bool right, bool down, bool left) {
        Up = up;
        Right = right;
        Down = down;
        Left = left;
    }

    public int Count() {
        var i = 0;
        if (Up) {
            i++;
        }

        if (Right) {
            i++;
        }

        if (Down) {
            i++;
        }

        if (Left) {
            i++;
        }

        return i;
    }

    public bool TwoTypeIsVertical() {
        return (Up && Down) || (Right && Left);
    }

    public int Rotation() {
        switch (Count()) {
            case 0:
                return 0;
            case 1:
                if (Up) {
                    return 0;
                }

                if (Left) {
                    return 1;
                }

                if (Down) {
                    return 2;
                }

                if (Right) {
                    return 3;
                }

                break;
            case 2:
                if (TwoTypeIsVertical()) {
                    return Up ? 0 : 1;
                }

                if (Up && Right) {
                    return 0;
                }

                if (Left && Up) {
                    return 1;
                }

                if (Down && Left) {
                    return 2;
                }

                if (Right && Down) {
                    return 3;
                }

                break;

            case 3:
                if (!Down) {
                    return 0;
                }

                if (!Right) {
                    return 1;
                }

                if (!Up) {
                    return 2;
                }

                if (!Left) {
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

using System.Runtime.CompilerServices;

namespace BFG.Runtime {
public static class GraphNode {
    public static byte None = 0;
    public static byte Right = 1 << 0;
    public static byte Up = 1 << 1;
    public static byte Left = 1 << 2;
    public static byte Down = 1 << 3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRight(byte node) {
        return (node & (1 << 0)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUp(byte node) {
        return (node & (1 << 1)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeft(byte node) {
        return (node & (1 << 2)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDown(byte node) {
        return (node & (1 << 3)) > 0;
    }

    public static byte SetDirection(byte node, Direction direction, bool value) {
        var dir = (byte)direction;
        if (value) {
            node |= (byte)(1 << dir);
        }
        else {
            node &= (byte)(15 ^ (1 << dir));
        }

        return node;
    }

    public static string ToString(byte node) {
        var str = "[";
        if (IsRight(node)) {
            str += "Right, ";
        }

        if (IsUp(node)) {
            str += "Up, ";
        }

        if (IsLeft(node)) {
            str += "Left, ";
        }

        if (IsDown(node)) {
            str += "Down, ";
        }

        if (node != 0) {
            str = str.Substring(0, str.Length - 2);
        }
        else {
            str += ".";
        }

        return str + "]";
    }

    public static string Repr(byte node) {
        var right = IsRight(node);
        var up = IsUp(node);
        var left = IsLeft(node);
        var down = IsDown(node);

        // 4
        if (right && up && left && down) {
            return "┼";
        }

        // 3
        if (right && up && left) {
            return "┴";
        }

        if (up && left && down) {
            return "┤";
        }

        if (left && down && right) {
            return "┬";
        }

        if (down && right && up) {
            return "├";
        }

        // 2
        if (right && up) {
            return "└";
        }

        if (up && left) {
            return "┘";
        }

        if (left && down) {
            return "┐";
        }

        if (right && down) {
            return "┌";
        }

        // 2 alt
        if (right && left) {
            return "─";
        }

        if (up && down) {
            return "│";
        }

        // 1
        if (right) {
            return "╶";
        }

        if (up) {
            return "╵";
        }

        if (left) {
            return "╴";
        }

        if (down) {
            return "╷";
        }

        return ".";
    }
}
}

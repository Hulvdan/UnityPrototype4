using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BFG.Core;
using UnityEngine.Assertions;

namespace BFG.Graphs {
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class GraphNode {
    public const byte None = 0;
    public const byte Right = 1 << 0;
    public const byte Up = 1 << 1;
    public const byte Left = 1 << 2;
    public const byte Down = 1 << 3;
    public const byte All = Right + Up + Left + Down;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(byte node, int direction) {
        Assert.IsTrue(direction is >= 0 and < 4);

        return (node & (1 << direction)) > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Has(byte node, Direction direction) {
        return Has(node, (int)direction);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRight(byte node) {
        return Has(node, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUp(byte node) {
        return Has(node, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeft(byte node) {
        return Has(node, 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDown(byte node) {
        return Has(node, 3);
    }

    public static byte Mark(byte node, Direction direction, bool value = true) {
        var dir = (byte)direction;
        Assert.IsTrue(dir < 4);

        if (value) {
            node |= (byte)(1 << dir);
        }
        else {
            node &= (byte)(15 ^ (1 << dir));
        }

        Assert.IsTrue(node < 16);
        return node;
    }

    public static string ToString(byte node) {
        Assert.IsTrue(node < 16);

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

#pragma warning disable S3776
    public static string ToDisplayString(byte node) {
#pragma warning restore S3776
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

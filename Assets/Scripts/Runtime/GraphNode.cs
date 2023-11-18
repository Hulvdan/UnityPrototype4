using System;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public struct GraphNode : IEquatable<GraphNode> {
    public byte Directions;

    public GraphNode(bool[] directions) {
        Directions = 0;
        Assert.AreEqual(directions.Length, 4);

        for (byte i = 0; i < 4; i++) {
            if (directions[i]) {
                Directions |= (byte)(1 << i);
            }
        }
    }

    public GraphNode(byte Directions) {
        this.Directions = Directions;
    }

    public bool right {
        get => (Directions & (1 << 0)) > 0;
        set => SetDirection(value, 0);
    }

    public bool up {
        get => (Directions & (1 << 1)) > 0;
        set => SetDirection(value, 1);
    }

    public bool left {
        get => (Directions & (1 << 2)) > 0;
        set => SetDirection(value, 2);
    }

    public bool down {
        get => (Directions & (1 << 3)) > 0;
        set => SetDirection(value, 3);
    }

    public void SetDirection(bool value, byte dir) {
        if (value) {
            Directions |= (byte)(1 << dir);
        }
        else {
            Directions &= (byte)(15 ^ (1 << dir));
        }
    }

    public bool Equals(GraphNode other) {
        return Directions == other.Directions;
    }

    public override bool Equals(object obj) {
        return obj is GraphNode other && Equals(other);
    }

    public override int GetHashCode() {
        return Directions.GetHashCode();
    }

    public override string ToString() {
        var str = "[";
        if (right) {
            str += "Right, ";
        }

        if (up) {
            str += "Up, ";
        }

        if (left) {
            str += "Left, ";
        }

        if (down) {
            str += "Down, ";
        }

        if (Directions != 0) {
            str = str.Substring(0, str.Length - 2);
        }
        else {
            str += ".";
        }

        return str + "]";
    }

    public string Repr() {
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

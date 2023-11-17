using BFG.Runtime;
using UnityEngine;

namespace Tests.EditMode {
internal class MockMapSize : IMapSize {
    public MockMapSize(int x, int y) {
        sizeX = x;
        sizeY = y;
    }

    public int sizeX { get; }
    public int sizeY { get; }

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < sizeX && y >= 0 && y < sizeY;
    }
}
}

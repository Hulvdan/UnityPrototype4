using BFG.Runtime;
using UnityEngine;

namespace Tests.EditMode {
internal class MockMapSize : IMapSize {
    public MockMapSize(int x, int y) {
        width = x;
        height = y;
    }

    public int width { get; }
    public int height { get; }

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
}

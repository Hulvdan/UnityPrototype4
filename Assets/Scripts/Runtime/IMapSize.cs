using UnityEngine;

namespace BFG.Runtime {
public interface IMapSize {
    int sizeY { get; }
    int sizeX { get; }

    bool Contains(Vector2Int pos);
    bool Contains(int x, int y);
}
}

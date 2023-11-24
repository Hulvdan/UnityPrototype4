using UnityEngine;

namespace BFG.Runtime {
public interface IMapSize {
    int height { get; }
    int width { get; }

    bool Contains(Vector2Int pos);
    bool Contains(int x, int y);
}
}

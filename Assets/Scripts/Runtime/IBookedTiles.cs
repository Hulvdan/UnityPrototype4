using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public interface IBookedTiles {
    public bool Contains(Vector2Int tile);
    public void Add(Vector2Int tile);
    public void Remove(Vector2Int tile);
}

public class BookedTilesSparseList : IBookedTiles {
    public bool Contains(Vector2Int tile) {
        foreach (var t in _tiles) {
            if (t == tile) {
                return true;
            }
        }

        return false;
    }

    public void Add(Vector2Int tile) {
        Assert.IsFalse(Contains(tile));

        _tiles.Add(tile);
    }

    public void Remove(Vector2Int tile) {
        Assert.IsTrue(Contains(tile));

        if (_tiles[^1] == tile) {
            _tiles.RemoveAt(_tiles.Count - 1);
            return;
        }

        for (var i = 0; i < _tiles.Count; i++) {
            if (_tiles[i] != tile) {
                continue;
            }

            _tiles[i] = _tiles[^1];
            _tiles.RemoveAt(_tiles.Count - 1);
            break;
        }
    }

    readonly List<Vector2Int> _tiles = new();
}

public class BookedTilesSet : IBookedTiles {
    public bool Contains(Vector2Int tile) {
        return _tiles.Contains(tile);
    }

    public void Add(Vector2Int tile) {
        Assert.IsFalse(Contains(tile));
        _tiles.Add(tile);
    }

    public void Remove(Vector2Int tile) {
        Assert.IsTrue(Contains(tile));
        _tiles.Remove(tile);
    }

    readonly HashSet<Vector2Int> _tiles = new();
}
}

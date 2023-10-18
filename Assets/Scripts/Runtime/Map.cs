using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public struct Tile {
    public string Name;
}

public class Map : MonoBehaviour {
    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Stone, Trees, Ore, Buildings, Rails
    // 3 - Animated (carts, trains, people, horses)
    // 4 - Particles (smoke, footstep dust etc)
    //
    // Stone, Tree, Ore:

    [Header("Dependencies")]
    [SerializeField]
    Grid _grid;

    [SerializeField]
    Tilemap _tilemapTerrain;

    [SerializeField]
    TileBase _tileGrass;

    [Header("Configuration")]
    [SerializeField]
    int _randomSeed;

    [SerializeField]
    [Min(1)]
    int _mapSizeX = 10;

    [SerializeField]
    [Min(1)]
    int _mapSizeY = 10;

    List<List<int>> _tileHeights;
    List<List<Tile>> _tiles;

    void Awake() {
        SetupTiles();
    }

    void OnValidate() {
        SetupTiles();
    }

    void SetupTiles() {
        _tiles = new List<List<Tile>>();
        _tileHeights = new List<List<int>>();

        for (var y = 0; y < _mapSizeY; y++) {
            var tiles = new List<Tile>();
            var tileHeights = new List<int>();

            for (var x = 0; x < _mapSizeX; x++) {
                // tiles.Append();
            }

            _tiles.Add(tiles);
            _tileHeights.Add(tileHeights);
        }

        RegenerateTilemap();
    }

    void RegenerateTilemap() {
        _tilemapTerrain.ClearAllTiles();

        for (var y = 0; y < _mapSizeY; y++) {
            for (var x = 0; x < _mapSizeX; x++) {
                _tilemapTerrain.SetTile(new Vector3Int(x, y, 0), _tileGrass);
            }
        }
    }
}
}

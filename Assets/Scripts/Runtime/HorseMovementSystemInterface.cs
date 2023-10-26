using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public enum CellType {
    None,
    Road,
    Station
}

public class HorseMovementSystemInterface : MonoBehaviour {
    [SerializeField]
    [Required]
    Grid _grid;

    public List<GameObject> Nodes = new();

    [SerializeField]
    [Required]
    Tilemap _tilemap;

    [SerializeField]
    TileBase _arrow0;

    [SerializeField]
    TileBase _arrow1;

    [SerializeField]
    TileBase _arrow21;

    [SerializeField]
    TileBase _arrow22;

    [SerializeField]
    TileBase _arrow3;

    [SerializeField]
    TileBase _arrow4;

    [SerializeField]
    Vector2Int _pointA;

    [SerializeField]
    Vector2Int _pointB;

    [SerializeField]
    [Required]
    Transform _movableObject;

    [SerializeField]
    AnimationCurve _curve;

    // [SerializeField]
    // [TableMatrix(SquareCells = true)]
    // Texture2D[,] _cells = new Texture2D[,] { { null } };

    [SerializeField]
    [Min(0)]
    float _moveDuration = 1f;

    readonly MapCell[,] _cells = {
        {
            MapCell.Road, MapCell.Road, MapCell.Road, MapCell.None, MapCell.Road
        }, {
            MapCell.Road, MapCell.None, MapCell.Road, MapCell.Road, MapCell.Road
        }, {
            MapCell.Road, MapCell.Road, MapCell.Road, MapCell.None, MapCell.Road
        }, {
            MapCell.None, MapCell.None, MapCell.None, MapCell.Road, MapCell.Road
        }, {
            MapCell.Road, MapCell.Road, MapCell.Road, MapCell.Road, MapCell.Road
        }, {
            MapCell.None, MapCell.Road, MapCell.None, MapCell.Road, MapCell.None
        }
    };

    float _cellElapsed;

    int _currentPathOffset;

    MovementGraphCell[,] _movementCells;

    List<Vector2Int> _path = new();

    HorseMovementSystem _system;

    void Awake() {
        GenerateTilemap();

        var system = new HorseMovementSystem();
        var path = system.FindPath(_pointA, _pointB, ref _movementCells);
        if (!path.Success) {
            Debug.LogError("Could not find the path");
            return;
        }

        _path = path.Path;
        PathTween();
    }

    TileBase GetTilebase(MovementGraphCell mapMovementCell) {
        if (mapMovementCell == null) {
            return null;
        }

        var c = mapMovementCell.Count();
        switch (c) {
            case 0:
                return _arrow0;
            case 1:
                return _arrow1;
            case 2:
                return mapMovementCell.TwoTypeIsVertical() ? _arrow21 : _arrow22;
            case 3:
                return _arrow3;
            case 4:
                return _arrow4;
        }

        return null;
    }

    void PathTween() {
        _currentPathOffset += 1;
        if (_currentPathOffset >= _path.Count) {
            return;
        }

        DOTween
            .To(
                () => _movableObject.transform.localPosition,
                val => _movableObject.transform.localPosition = val,
                _path[_currentPathOffset],
                _moveDuration
            )
            .SetEase(_curve)
            .OnComplete(PathTween);
    }

    [Button("Generate tilemap")]
    void GenerateTilemap() {
        _tilemap.ClearAllTiles();

        var sizeY = _cells.GetLength(0);
        var sizeX = _cells.GetLength(1);
        _grid.transform.position = new Vector3(-sizeX / 2f, -sizeY / 2f, 0);

        _movementCells = new MovementGraphCell[sizeY, sizeX];
        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                var cell = _cells[y, x];
                var mCell = new MovementGraphCell(false, false, false, false);
                _movementCells[y, x] = cell.Type == CellType.Road ? mCell : null;

                if (cell.Type == CellType.Road) {
                    mCell.Up = y < sizeY - 1 && _cells[y + 1, x].Type == CellType.Road;
                    mCell.Down = y > 0 && _cells[y - 1, x].Type == CellType.Road;
                    mCell.Left = x > 0 && _cells[y, x - 1].Type == CellType.Road;
                    mCell.Right = x < sizeX - 1 && _cells[y, x + 1].Type == CellType.Road;
                }
            }
        }

        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                var cell = _movementCells[y, x];
                var tb = GetTilebase(cell);
                if (tb == null) {
                    continue;
                }

                var td = new TileChangeData(
                    new Vector3Int(x, y, 0),
                    tb,
                    Color.white,
                    Matrix4x4.TRS(
                        new Vector3(0, 0, 0),
                        Quaternion.Euler(0, 0, 90 * cell.Rotation()),
                        Vector3.one
                    )
                );
                _tilemap.SetTile(td, false);
            }
        }
    }

    // void Update() {
    //     if (_path == null || _path.Count <= 0) {
    //         return;
    //     }
    //
    //     _cellElapsed += Time.deltaTime;
    //     var coef = _cellElapsed / _moveDuration;
    //     while (coef >= 1) {
    //         _cellElapsed -= _moveDuration;
    //     }
    // }
}
}

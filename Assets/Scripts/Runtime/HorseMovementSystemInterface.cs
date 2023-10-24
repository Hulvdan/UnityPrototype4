using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Apple.ReplayKit;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public enum CellType {
    None,
    Road,
    Station
}

public struct Cell {
    public CellType Type;
    public int Rotation;

    public Cell(CellType type, int rotation = 0) {
        if ((type == CellType.Road || type == CellType.None) && rotation != 0) {
            Debug.LogError("WTF IS GOING ON HERE?");
            rotation = 0;
        }

        Type = type;
        Rotation = rotation;
    }

    public static Cell None = new(CellType.None);
    public static Cell Road = new(CellType.Road);
}

public class MCell {
    public bool Up;
    public bool Right;
    public bool Down;
    public bool Left;

    public MCell(bool up, bool right, bool down, bool left) {
        Up = up;
        Right = right;
        Down = down;
        Left = left;
    }

    public int Count() {
        var i = 0;
        if (Up) {
            i++;
        }

        if (Right) {
            i++;
        }

        if (Down) {
            i++;
        }

        if (Left) {
            i++;
        }

        return i;
    }

    public bool TwoTypeIsVertical() {
        return (Up && Down) || (Right && Left);
    }

    public int Rotation() {
        switch (Count()) {
            case 0:
                return 0;
            case 1:
                if (Up) {
                    return 0;
                }

                if (Left) {
                    return 1;
                }

                if (Down) {
                    return 2;
                }

                if (Right) {
                    return 3;
                }

                break;
            case 2:
                if (TwoTypeIsVertical()) {
                    return Up ? 0 : 1;
                }

                if (Up && Right) {
                    return 0;
                }

                if (Left && Up) {
                    return 1;
                }

                if (Down && Left) {
                    return 2;
                }

                if (Right && Down) {
                    return 3;
                }

                break;

            case 3:
                if (!Down) {
                    return 0;
                }

                if (!Right) {
                    return 1;
                }

                if (!Up) {
                    return 2;
                }

                if (!Left) {
                    return 3;
                }

                break;
            case 4:
                return 0;
            default:
                Debug.LogError("What the fuck is going on here?");
                break;
        }

        Debug.LogError("What the fuck is going on here?");
        return -1;
    }
}

public class HorseMovementSystemInterface : MonoBehaviour {
    [SerializeField]
    [Required]
    Grid _grid;

    HorseMovementSystem _system;

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

    TileBase GetTilebase(MCell mCell) {
        if (mCell == null) {
            return null;
        }

        var c = mCell.Count();
        switch (c) {
            case 0:
                return _arrow0;
            case 1:
                return _arrow1;
            case 2:
                return mCell.TwoTypeIsVertical() ? _arrow21 : _arrow22;
            case 3:
                return _arrow3;
            case 4:
                return _arrow4;
        }

        return null;
    }

    // [SerializeField]
    // [TableMatrix(SquareCells = true)]
    // Texture2D[,] _cells = new Texture2D[,] { { null } };

    Cell[,] _cells = {
        {
            Cell.Road, Cell.Road, Cell.Road, Cell.None, Cell.Road
        }, {
            Cell.Road, Cell.None, Cell.Road, Cell.Road, Cell.Road
        }, {
            Cell.Road, Cell.Road, Cell.Road, Cell.None, Cell.Road
        }, {
            Cell.None, Cell.Road, Cell.None, Cell.Road, Cell.Road
        }, {
            Cell.Road, Cell.Road, Cell.Road, Cell.Road, Cell.Road
        }, {
            Cell.None, Cell.Road, Cell.None, Cell.Road, Cell.None
        }
    };

    MCell[,] _movementCells;

    [Button("Generate tilemap")]
    void GenerateTilemap() {
        _tilemap.ClearAllTiles();

        var sizeY = _cells.GetLength(0);
        var sizeX = _cells.GetLength(1);
        _grid.transform.position = new Vector3(-sizeX / 2f, -sizeY / 2f, 0);

        _movementCells = new MCell[sizeY, sizeX];
        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                var cell = _cells[y, x];
                var mCell = new MCell(false, false, false, false);
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
}
}

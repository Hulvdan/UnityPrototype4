using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public enum CellType {
    None,
    Road,
    Station
}

public class HorseMovementSystemInterface : MonoBehaviour {
    [FormerlySerializedAs("_tilemap")]
    [FoldoutGroup("Debug", true)]
    [SerializeField]
    Tilemap _debugTilemap;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow0;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow1;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow21;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow22;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow3;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    TileBase _arrow4;

    [SerializeField]
    Vector2Int _pointA;

    [SerializeField]
    Vector2Int _pointB;

    [SerializeField]
    [Required]
    List<Transform> _movableObjects;

    [SerializeField]
    [Min(0)]
    float _trainSpeed = 1f;

    public MapCell[,] Cells;

    HorseTrain _horse;
    HorseMovementSystem _horseMovement;
    MovementGraphCell[,] _movementCells;
    List<Vector2Int> _path = new();

    public void Init(MapCell[,] cells) {
        Cells = cells;
        GenerateTilemap();

        _horseMovement = new HorseMovementSystem();

        _horse = new HorseTrain(_trainSpeed);
        _horse.AddLocomotive(new TrainNode(1f), 2, 0f);
        _horse.AddNode(new TrainNode(1f));

        _horseMovement.OnReachedTarget += (dir) => {
            (_pointA, _pointB) = (_pointB, _pointA);
            BuildHorsePath(dir);
        };

        BuildHorsePath(Direction.Right);
    }

    void BuildHorsePath(Direction direction) {
        var path = _horseMovement.FindPath(_pointA, _pointB, ref _movementCells, direction);
        if (!path.Success) {
            Debug.LogError("Could not find the path");
            return;
        }

        _path = path.Path;
        foreach (var vertex in _path) {
            _horse.AddSegmentVertex(vertex);
        }
    }

    void Update() {
        UpdateTrain();
    }

    void UpdateTrain() {
        if (_horse == null) {
            return;
        }

        _horseMovement.AdvanceTrain(_horse);
        _horseMovement.RecalculateNodePositions(_horse);

        for (var i = 0; i < _movableObjects.Count; i++) {
            _movableObjects[i].localPosition = _horse.nodes[i].CalculatedPosition;
        }
    }

    TileBase GetTileBase(MovementGraphCell mapMovementCell) {
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

    [Button("Generate tilemap")]
    void GenerateTilemap() {
        if (_debugTilemap != null) {
            _debugTilemap.ClearAllTiles();
        }

        var sizeY = Cells.GetLength(0);
        var sizeX = Cells.GetLength(1);

        _movementCells = new MovementGraphCell[sizeY, sizeX];
        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                var cell = Cells[y, x];
                var mCell = new MovementGraphCell(false, false, false, false);
                _movementCells[y, x] = cell.Type == CellType.None ? null : mCell;

                if (cell.Type == CellType.Road) {
                    mCell.Directions[0] = x < sizeX - 1
                                          && (
                                              Cells[y, x + 1].Type == CellType.Road
                                              || (Cells[y, x + 1].Type == CellType.Station &&
                                                  Cells[y, x + 1].Rotation == 0)
                                          );
                    mCell.Directions[2] = x > 0
                                          && (
                                              Cells[y, x - 1].Type == CellType.Road
                                              || (Cells[y, x - 1].Type == CellType.Station &&
                                                  Cells[y, x - 1].Rotation == 0)
                                          );
                    mCell.Directions[1] = y < sizeY - 1
                                          && (
                                              Cells[y + 1, x].Type == CellType.Road
                                              || (Cells[y + 1, x].Type == CellType.Station &&
                                                  Cells[y + 1, x].Rotation == 1)
                                          );
                    mCell.Directions[3] = y > 0
                                          && (
                                              Cells[y - 1, x].Type == CellType.Road
                                              || (Cells[y - 1, x].Type == CellType.Station &&
                                                  Cells[y - 1, x].Rotation == 1)
                                          );
                }
                else if (cell.Type == CellType.Station) {
                    if (cell.Rotation == 0) {
                        mCell.Directions[0] = x < sizeX - 1
                                              && (
                                                  Cells[y, x + 1].Type == CellType.Road
                                                  || (Cells[y, x + 1].Type == CellType.Station &&
                                                      Cells[y, x + 1].Rotation == 0)
                                              );
                        mCell.Directions[2] = x > 0
                                              && (
                                                  Cells[y, x - 1].Type == CellType.Road
                                                  || (Cells[y, x - 1].Type == CellType.Station &&
                                                      Cells[y, x - 1].Rotation == 0)
                                              );
                    }
                    else if (cell.Rotation == 1) {
                        mCell.Directions[1] = y < sizeY - 1
                                              && (
                                                  Cells[y + 1, x].Type == CellType.Road
                                                  || (Cells[y + 1, x].Type == CellType.Station &&
                                                      Cells[y + 1, x].Rotation == 1)
                                              );
                        mCell.Directions[3] = y > 0
                                              && (
                                                  Cells[y - 1, x].Type == CellType.Road
                                                  || (Cells[y - 1, x].Type == CellType.Station &&
                                                      Cells[y - 1, x].Rotation == 1)
                                              );
                    }
                }
            }
        }

        if (_debugTilemap == null) {
            return;
        }

        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                var cell = _movementCells[y, x];
                var tb = GetTileBase(cell);
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
                _debugTilemap.SetTile(td, false);
            }
        }
    }
}
}

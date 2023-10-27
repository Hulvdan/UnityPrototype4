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

    HorseTrain _horse;
    HorseMovementSystem _horseMovement;

    Map _map;
    MovementGraphCell[,] _movementCells;
    List<Vector2Int> _path = new();

    void Update() {
        UpdateTrain();
    }

    public void Init(Map map) {
        _map = map;
        map.OnCellChanged += OnCellChanged;
        GenerateMovementGraph();

        _horseMovement = new HorseMovementSystem();

        _horse = new HorseTrain(_trainSpeed);
        _horse.AddLocomotive(new TrainNode(1f), 3, 0f);
        _horse.AddNode(new TrainNode(.8f));
        _horse.AddNode(new TrainNode(.8f));
        _horse.AddNode(new TrainNode(.8f));

        _horseMovement.OnReachedTarget += dir => {
            (_pointA, _pointB) = (_pointB, _pointA);
            BuildHorsePath(dir, false);
        };

        BuildHorsePath(Direction.Right, true);
    }

    void OnCellChanged(Vector2Int pos) {
        ref var cells = ref _map.cells;
        UpdateCellAtPos(pos.x, pos.y);
        UpdateDebugTilemapAtPos(pos.x, pos.y);

        if (pos.x > 0) {
            UpdateCellAtPos(pos.x - 1, pos.y);
            UpdateDebugTilemapAtPos(pos.x - 1, pos.y);
        }

        if (pos.x < cells.GetLength(1) - 1) {
            UpdateCellAtPos(pos.x + 1, pos.y);
            UpdateDebugTilemapAtPos(pos.x + 1, pos.y);
        }

        if (pos.y > 0) {
            UpdateCellAtPos(pos.x, pos.y - 1);
            UpdateDebugTilemapAtPos(pos.x, pos.y - 1);
        }

        if (pos.y < cells.GetLength(0) - 1) {
            UpdateCellAtPos(pos.x, pos.y + 1);
            UpdateDebugTilemapAtPos(pos.x, pos.y + 1);
        }
    }

    void BuildHorsePath(Direction direction, bool initial) {
        var path = _horseMovement.FindPath(_pointA, _pointB, ref _movementCells, direction);
        if (!path.Success) {
            Debug.LogError("Could not find the path");
            return;
        }

        _path = path.Path;
        var skipFirst = !initial;
        foreach (var vertex in _path) {
            if (skipFirst) {
                skipFirst = false;
                continue;
            }

            _horse.AddSegmentVertex(vertex);
        }
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

    TileBase GetDebugTileBase(MovementGraphCell mapMovementCell) {
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

    [Button("Generate Movement Graph")]
    void GenerateMovementGraph() {
        if (_debugTilemap != null) {
            _debugTilemap.ClearAllTiles();
        }

        ref var cells = ref _map.cells;
        var sizeY = cells.GetLength(0);
        var sizeX = cells.GetLength(1);

        _movementCells = new MovementGraphCell[sizeY, sizeX];
        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                UpdateCellAtPos(x, y);
            }
        }

        GenerateDebugTilemap();
    }

    void UpdateCellAtPos(int x, int y) {
        ref var cells = ref _map.cells;

        var sizeY = cells.GetLength(0);
        var sizeX = cells.GetLength(1);

        var cell = cells[y, x];

        if (cell.Type == CellType.None) {
            _movementCells[y, x] = null;
            return;
        }

        var mCell = _movementCells[y, x];
        if (mCell == null) {
            mCell = new MovementGraphCell(false, false, false, false);
            _movementCells[y, x] = mCell;
        }

        if (cell.Type == CellType.Road) {
            mCell.Directions[0] = x < sizeX - 1
                                  && (
                                      cells[y, x + 1].Type == CellType.Road
                                      || (cells[y, x + 1].Type == CellType.Station &&
                                          cells[y, x + 1].Rotation == 0)
                                  );
            mCell.Directions[2] = x > 0
                                  && (
                                      cells[y, x - 1].Type == CellType.Road
                                      || (cells[y, x - 1].Type == CellType.Station &&
                                          cells[y, x - 1].Rotation == 0)
                                  );
            mCell.Directions[1] = y < sizeY - 1
                                  && (
                                      cells[y + 1, x].Type == CellType.Road
                                      || (cells[y + 1, x].Type == CellType.Station &&
                                          cells[y + 1, x].Rotation == 1)
                                  );
            mCell.Directions[3] = y > 0
                                  && (
                                      cells[y - 1, x].Type == CellType.Road
                                      || (cells[y - 1, x].Type == CellType.Station &&
                                          cells[y - 1, x].Rotation == 1)
                                  );
        }
        else if (cell.Type == CellType.Station) {
            if (cell.Rotation == 0) {
                mCell.Directions[0] = x < sizeX - 1
                                      && (
                                          cells[y, x + 1].Type == CellType.Road
                                          || (cells[y, x + 1].Type == CellType.Station &&
                                              cells[y, x + 1].Rotation == 0)
                                      );
                mCell.Directions[2] = x > 0
                                      && (
                                          cells[y, x - 1].Type == CellType.Road
                                          || (cells[y, x - 1].Type == CellType.Station &&
                                              cells[y, x - 1].Rotation == 0)
                                      );
            }
            else if (cell.Rotation == 1) {
                mCell.Directions[1] = y < sizeY - 1
                                      && (
                                          cells[y + 1, x].Type == CellType.Road
                                          || (cells[y + 1, x].Type == CellType.Station &&
                                              cells[y + 1, x].Rotation == 1)
                                      );
                mCell.Directions[3] = y > 0
                                      && (
                                          cells[y - 1, x].Type == CellType.Road
                                          || (cells[y - 1, x].Type == CellType.Station &&
                                              cells[y - 1, x].Rotation == 1)
                                      );
            }
        }
    }

    void GenerateDebugTilemap() {
        if (_debugTilemap == null) {
            return;
        }

        ref var cells = ref _map.cells;
        var sizeY = cells.GetLength(0);
        var sizeX = cells.GetLength(1);

        for (var y = 0; y < sizeY; y++) {
            for (var x = 0; x < sizeX; x++) {
                UpdateDebugTilemapAtPos(x, y);
            }
        }
    }

    void UpdateDebugTilemapAtPos(int x, int y) {
        var cell = _movementCells[y, x];
        var tb = GetDebugTileBase(cell);
        if (tb == null) {
            return;
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

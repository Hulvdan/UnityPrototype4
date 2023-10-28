using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public enum ElementTileType {
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

    public readonly Subject<Direction> OnTrainReachedTarget = new();

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
        map.OnElementTileChanged.Subscribe(OnElementTileChanged);
        GenerateMovementGraph();

        _horseMovement = new();

        _horse = new(_trainSpeed);
        _horse.AddLocomotive(new(1f), 3, 0f);
        _horse.AddNode(new(.8f));
        _horse.AddNode(new(.8f));
        _horse.AddNode(new(.8f));

        _horse.AddDestination(new() {
            Type = TrainDestinationType.Load,
            Pos = _pointA
        });
        _horse.AddDestination(new() {
            Type = TrainDestinationType.Unload,
            Pos = _pointB
        });

        // _horseMovement.OnReachedTarget.Subscribe(dir => OnTrainReachedTarget.OnNext(dir));

        BuildHorsePath(Direction.Right, true);
    }

    void OnElementTileChanged(Vector2Int pos) {
        UpdateCellAtPos(pos);
        UpdateDebugTilemapAtPos(pos);

        foreach (var offset in DirectionOffsets.Offsets) {
            var newPos = pos + offset;
            if (_map.Contains(newPos)) {
                UpdateCellAtPos(newPos);
                UpdateDebugTilemapAtPos(newPos);
            }
        }
    }

    public void BuildHorsePath(Direction direction, bool initial) {
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

        _movementCells = new MovementGraphCell[_map.sizeY, _map.sizeX];
        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                UpdateCellAtPos(x, y);
            }
        }

        GenerateDebugTilemap();
    }

    void UpdateCellAtPos(Vector2Int pos) {
        UpdateCellAtPos(pos.x, pos.y);
    }

    void UpdateCellAtPos(int x, int y) {
        var elementTiles = _map.elementTiles;

        var cell = elementTiles[y][x];

        if (cell.Type == ElementTileType.None) {
            _movementCells[y, x] = null;
            return;
        }

        var mCell = _movementCells[y, x];
        if (mCell == null) {
            mCell = new(false, false, false, false);
            _movementCells[y, x] = mCell;
        }

        if (cell.Type == ElementTileType.Road) {
            mCell.Directions[0] = x < _map.sizeX - 1
                                  && (
                                      elementTiles[y][x + 1].Type == ElementTileType.Road
                                      || (elementTiles[y][x + 1].Type == ElementTileType.Station &&
                                          elementTiles[y][x + 1].Rotation == 0)
                                  );
            mCell.Directions[2] = x > 0
                                  && (
                                      elementTiles[y][x - 1].Type == ElementTileType.Road
                                      || (elementTiles[y][x - 1].Type == ElementTileType.Station &&
                                          elementTiles[y][x - 1].Rotation == 0)
                                  );
            mCell.Directions[1] = y < _map.sizeY - 1
                                  && (
                                      elementTiles[y + 1][x].Type == ElementTileType.Road
                                      || (elementTiles[y + 1][x].Type == ElementTileType.Station &&
                                          elementTiles[y + 1][x].Rotation == 1)
                                  );
            mCell.Directions[3] = y > 0
                                  && (
                                      elementTiles[y - 1][x].Type == ElementTileType.Road
                                      || (elementTiles[y - 1][x].Type == ElementTileType.Station &&
                                          elementTiles[y - 1][x].Rotation == 1)
                                  );
        }
        else if (cell.Type == ElementTileType.Station) {
            if (cell.Rotation == 0) {
                mCell.Directions[0] = x < _map.sizeX - 1
                                      && (
                                          elementTiles[y][x + 1].Type == ElementTileType.Road
                                          || (elementTiles[y][x + 1].Type ==
                                              ElementTileType.Station &&
                                              elementTiles[y][x + 1].Rotation == 0)
                                      );
                mCell.Directions[2] = x > 0
                                      && (
                                          elementTiles[y][x - 1].Type == ElementTileType.Road
                                          || (elementTiles[y][x - 1].Type ==
                                              ElementTileType.Station &&
                                              elementTiles[y][x - 1].Rotation == 0)
                                      );
            }
            else if (cell.Rotation == 1) {
                mCell.Directions[1] = y < _map.sizeY - 1
                                      && (
                                          elementTiles[y + 1][x].Type == ElementTileType.Road
                                          || (elementTiles[y + 1][x].Type ==
                                              ElementTileType.Station &&
                                              elementTiles[y + 1][x].Rotation == 1)
                                      );
                mCell.Directions[3] = y > 0
                                      && (
                                          elementTiles[y - 1][x].Type == ElementTileType.Road
                                          || (elementTiles[y - 1][x].Type ==
                                              ElementTileType.Station &&
                                              elementTiles[y - 1][x].Rotation == 1)
                                      );
            }
        }
    }

    void GenerateDebugTilemap() {
        if (_debugTilemap == null) {
            return;
        }

        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                UpdateDebugTilemapAtPos(x, y);
            }
        }
    }

    void UpdateDebugTilemapAtPos(Vector2Int pos) {
        UpdateDebugTilemapAtPos(pos.x, pos.y);
    }

    void UpdateDebugTilemapAtPos(int x, int y) {
        var cell = _movementCells[y, x];
        var tb = GetDebugTileBase(cell);
        if (tb == null) {
            return;
        }

        var td = new TileChangeData(
            new(x, y, 0),
            tb,
            Color.white,
            Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(0, 0, 90 * cell.Rotation()),
                Vector3.one
            )
        );
        _debugTilemap.SetTile(td, false);
    }
}
}

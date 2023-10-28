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
    Station,
}

public class HorseCompoundSystem : MonoBehaviour {
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
    [Min(0)]
    float _trainItemLoadingDuration = 1f;

    [SerializeField]
    [Min(0)]
    float _trainItemUnloadingDuration = 1f;

    [SerializeField]
    [Min(1)]
    int _horsesGatheringAroundStationRadius = 2;

    [SerializeField]
    [Required]
    List<Transform> _movableObjects;

    [FormerlySerializedAs("_trainSpeed")]
    [SerializeField]
    [Min(0)]
    float _horseSpeed = 1f;

    public readonly Subject<Direction> OnHorseReachedTarget = new();

    HorseTrain _horse;

    Map _map;
    List<List<MovementGraphCell>> _movementCells;
    HorseMovementSystem _movementSystem;
    List<Vector2Int> _path = new();

    void Update() {
        if (_horse != null) {
            UpdateHorse(_horse);
        }
    }

    public void Init(Map map) {
        _map = map;
        _map.OnElementTileChanged.Subscribe(OnElementTileChanged);

        GenerateMovementGraph();

        _movementSystem = new();
        _movementSystem.Init(_map, _movementCells);
        _movementSystem.OnReachedDestination.Subscribe(OnHorseReachedDestination);

        _horse = new(_horseSpeed, Direction.Right);
        _horse.AddSegmentVertex(_pointA);
        _horse.AddSegmentVertex(_pointA);
        _horse.AddSegmentVertex(_pointA);
        _horse.AddSegmentVertex(_pointA);
        _horse.AddLocomotive(new(1f), 3, 0f);
        _horse.AddNode(new(.8f));
        _horse.AddNode(new(.8f));
        _horse.AddNode(new(.8f));

        _horse.AddDestination(new() {
            Type = HorseDestinationType.Load,
            Pos = _pointB,
        });
        _horse.AddDestination(new() {
            Type = HorseDestinationType.Unload,
            Pos = _pointA,
        });

        _movementSystem.TrySetNextDestinationAndBuildPath(_horse);

        // _horseMovement.OnReachedTarget.Subscribe(dir => OnTrainReachedTarget.OnNext(dir));
        // BuildHorsePath(Direction.Right, true);
    }

    void OnHorseReachedDestination(OnReachedDestinationData data) {
        if (data.destination.Type == HorseDestinationType.Load) {
            data.train.State = TrainState.Loading;
        }
        else if (data.destination.Type == HorseDestinationType.Load) {
            data.train.State = TrainState.Unloading;
        }
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

    // public void BuildHorsePath(Direction direction, bool initial) {
    //     var path = _movementSystem.FindPath(_pointA, _pointB, ref _movementCells, direction);
    //     if (!path.Success) {
    //         Debug.LogError("Could not find the path");
    //         return;
    //     }
    //
    //     _path = path.Path;
    //     var skipFirst = !initial;
    //     foreach (var vertex in _path) {
    //         if (skipFirst) {
    //             skipFirst = false;
    //             continue;
    //         }
    //
    //         _horse.AddSegmentVertex(vertex);
    //     }
    // }

    void UpdateHorse(HorseTrain horse) {
        switch (horse.State) {
            case TrainState.Idle:
                break;
            case TrainState.Moving:
                _movementSystem.AdvanceHorse(horse);
                _movementSystem.RecalculateNodePositions(horse);
                break;
            case TrainState.Loading:
                horse.TrainLoadingUnloadingElapsed += Time.deltaTime;
                if (horse.TrainLoadingUnloadingElapsed >= _trainItemLoadingDuration) {
                    horse.TrainLoadingUnloadingElapsed -= _trainItemLoadingDuration;

                    if (_map.AreThereAvailableResourcesForTheTrain(horse)) {
                        _map.PickRandomItemForTheTrain(horse);
                    }
                    else {
                        horse.State = TrainState.Moving;
                        _movementSystem.TrySetNextDestinationAndBuildPath(horse);
                    }
                }

                break;
            case TrainState.Unloading:
                horse.TrainLoadingUnloadingElapsed += Time.deltaTime;
                break;
        }

        for (var i = 0; i < _movableObjects.Count; i++) {
            _movableObjects[i].localPosition = horse.nodes[i].CalculatedPosition;
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

        _movementCells = new(_map.sizeY);
        for (var y = 0; y < _map.sizeY; y++) {
            var row = new List<MovementGraphCell>(_map.sizeX);
            for (var x = 0; x < _map.sizeX; x++) {
                row.Add(null);
            }

            _movementCells.Add(row);
        }

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
            _movementCells[y][x] = null;
            return;
        }

        var mCell = _movementCells[y][x];
        if (mCell == null) {
            mCell = new(false, false, false, false);
            _movementCells[y][x] = mCell;
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
        var cell = _movementCells[y][x];
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

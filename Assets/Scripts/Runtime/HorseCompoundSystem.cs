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
    [FoldoutGroup("Debug", true)]
    [SerializeField]
    bool _debugMode;

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
    [Required]
    List<Transform> _movableObjects;

    [FormerlySerializedAs("_trainSpeed")]
    [SerializeField]
    [Min(0)]
    float _horseSpeed = 1f;

    public readonly Subject<Direction> OnHorseReachedTarget = new();

    HorseTrain _horse;

    IMap _map;
    IMapSize _mapSize;

    HorseMovementSystem _movementSystem;
    List<List<MovementGraphTile>> _movementTiles;
    List<Vector2Int> _path = new();

    void Update() {
        if (_horse != null) {
            UpdateHorse(_horse);
        }
    }

    public void Init(IMap map, IMapSize mapSize) {
        _mapSize = mapSize;
        _map = map;
        _map.OnElementTileChanged.Subscribe(OnElementTileChanged);

        GenerateMovementGraph();

        _movementSystem = new() { debugMode = _debugMode };
        _movementSystem.Init(_mapSize, _movementTiles);
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
        _horse.State = TrainState.Moving;
    }

    void OnHorseReachedDestination(OnReachedDestinationData data) {
        switch (data.destination.Type) {
            case HorseDestinationType.Load:
                data.train.State = TrainState.Loading;
                break;
            case HorseDestinationType.Unload:
                data.train.State = TrainState.Unloading;
                break;
        }
    }

    void OnElementTileChanged(Vector2Int pos) {
        UpdateTileAtPos(pos);
        UpdateDebugTilemapAtPos(pos);

        foreach (var offset in DirectionOffsets.Offsets) {
            var newPos = pos + offset;
            if (_mapSize.Contains(newPos)) {
                UpdateTileAtPos(newPos);
                UpdateDebugTilemapAtPos(newPos);
            }
        }
    }

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

                    if (
                        horse.IsThereANodeThatCanStoreItem()
                        && _map.AreThereAvailableResourcesForTheTrain(horse)
                    ) {
                        if (_debugMode) {
                            Debug.Log("There is an empty slot in the train and " +
                                      "found a new item that can be added to the train");
                        }

                        _map.PickRandomItemForTheTrain(horse);
                    }
                    else {
                        if (_debugMode) {
                            Debug.Log("Switching to the next destination");
                        }

                        horse.State = TrainState.Moving;
                        _movementSystem.TrySetNextDestinationAndBuildPath(horse);
                    }
                }

                break;
            case TrainState.Unloading:
                horse.TrainLoadingUnloadingElapsed += Time.deltaTime;
                if (horse.TrainLoadingUnloadingElapsed >= _trainItemUnloadingDuration) {
                    horse.TrainLoadingUnloadingElapsed -= _trainItemUnloadingDuration;

                    if (_map.AreThereAvailableSlotsTheTrainCanPassResourcesTo(horse)) {
                        _map.PickRandomSlotForTheTrainToPassItemTo(horse);
                    }
                    else {
                        horse.State = TrainState.Moving;
                        _movementSystem.TrySetNextDestinationAndBuildPath(horse);
                    }
                }

                break;
        }

        for (var i = 0; i < _movableObjects.Count; i++) {
            _movableObjects[i].localPosition = horse.nodes[i].CalculatedPosition;
        }
    }

    TileBase GetDebugTileBase(MovementGraphTile mapMovementTile) {
        if (mapMovementTile == null) {
            return null;
        }

        var c = mapMovementTile.Count();
        switch (c) {
            case 0:
                return _arrow0;
            case 1:
                return _arrow1;
            case 2:
                return mapMovementTile.TwoTypeIsVertical() ? _arrow21 : _arrow22;
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

        _movementTiles = new(_mapSize.sizeY);
        for (var y = 0; y < _mapSize.sizeY; y++) {
            var row = new List<MovementGraphTile>(_mapSize.sizeX);
            for (var x = 0; x < _mapSize.sizeX; x++) {
                row.Add(null);
            }

            _movementTiles.Add(row);
        }

        for (var y = 0; y < _mapSize.sizeY; y++) {
            for (var x = 0; x < _mapSize.sizeX; x++) {
                UpdateTileAtPos(x, y);
            }
        }

        GenerateDebugTilemap();
    }

    void UpdateTileAtPos(Vector2Int pos) {
        UpdateTileAtPos(pos.x, pos.y);
    }

    void UpdateTileAtPos(int x, int y) {
        var elementTiles = _map.elementTiles;

        var tile = elementTiles[y][x];

        if (tile.Type == ElementTileType.None) {
            _movementTiles[y][x] = null;
            return;
        }

        var mTile = _movementTiles[y][x];
        if (mTile == null) {
            mTile = new(false, false, false, false);
            _movementTiles[y][x] = mTile;
        }

        if (tile.Type == ElementTileType.Road) {
            UpdateRoadTile(x, y, tile, mTile, elementTiles);
        }
        else if (tile.Type == ElementTileType.Station) {
            UpdateStationTile(x, y, tile, mTile, elementTiles);
        }
    }

    void UpdateRoadTile(
        int x,
        int y,
        ElementTile tile,
        MovementGraphTile mTile,
        List<List<ElementTile>> tiles
    ) {
        mTile.Directions[0] = x < _mapSize.sizeX - 1
                              && (
                                  tiles[y][x + 1].Type == ElementTileType.Road
                                  || (
                                      tiles[y][x + 1].Type == ElementTileType.Station
                                      && tiles[y][x + 1].Rotation == 0
                                  )
                              );
        mTile.Directions[2] = x > 0
                              && (
                                  tiles[y][x - 1].Type == ElementTileType.Road
                                  || (
                                      tiles[y][x - 1].Type == ElementTileType.Station
                                      && tiles[y][x - 1].Rotation == 0
                                  )
                              );
        mTile.Directions[1] = y < _mapSize.sizeY - 1
                              && (
                                  tiles[y + 1][x].Type == ElementTileType.Road
                                  || (
                                      tiles[y + 1][x].Type == ElementTileType.Station
                                      && tiles[y + 1][x].Rotation == 1
                                  )
                              );
        mTile.Directions[3] = y > 0
                              && (
                                  tiles[y - 1][x].Type == ElementTileType.Road
                                  || (
                                      tiles[y - 1][x].Type == ElementTileType.Station
                                      && tiles[y - 1][x].Rotation == 1
                                  )
                              );
    }

    void UpdateStationTile(
        int x,
        int y,
        ElementTile tile,
        MovementGraphTile mTile,
        List<List<ElementTile>> tiles
    ) {
        if (tile.Rotation == 0) {
            mTile.Directions[0] = x < _mapSize.sizeX - 1
                                  && (
                                      tiles[y][x + 1].Type == ElementTileType.Road
                                      || (
                                          tiles[y][x + 1].Type == ElementTileType.Station
                                          && tiles[y][x + 1].Rotation == 0
                                      )
                                  );
            mTile.Directions[2] = x > 0
                                  && (
                                      tiles[y][x - 1].Type == ElementTileType.Road
                                      || (
                                          tiles[y][x - 1].Type == ElementTileType.Station
                                          && tiles[y][x - 1].Rotation == 0
                                      )
                                  );
        }
        else if (tile.Rotation == 1) {
            mTile.Directions[1] = y < _mapSize.sizeY - 1
                                  && (
                                      tiles[y + 1][x].Type == ElementTileType.Road
                                      || (
                                          tiles[y + 1][x].Type == ElementTileType.Station
                                          && tiles[y + 1][x].Rotation == 1
                                      )
                                  );
            mTile.Directions[3] = y > 0
                                  && (
                                      tiles[y - 1][x].Type == ElementTileType.Road
                                      || (
                                          tiles[y - 1][x].Type == ElementTileType.Station
                                          && tiles[y - 1][x].Rotation == 1
                                      )
                                  );
        }
    }

    void GenerateDebugTilemap() {
        if (_debugTilemap == null) {
            return;
        }

        for (var y = 0; y < _mapSize.sizeY; y++) {
            for (var x = 0; x < _mapSize.sizeX; x++) {
                UpdateDebugTilemapAtPos(x, y);
            }
        }
    }

    void UpdateDebugTilemapAtPos(Vector2Int pos) {
        UpdateDebugTilemapAtPos(pos.x, pos.y);
    }

    void UpdateDebugTilemapAtPos(int x, int y) {
        var tile = _movementTiles[y][x];
        var tb = GetDebugTileBase(tile);
        if (tb == null) {
            return;
        }

        var td = new TileChangeData(
            new(x, y, 0),
            tb,
            Color.white,
            Matrix4x4.TRS(
                Vector3.zero,
                Quaternion.Euler(0, 0, 90 * tile.Rotation()),
                Vector3.one
            )
        );
        _debugTilemap.SetTile(td, false);
    }
}
}

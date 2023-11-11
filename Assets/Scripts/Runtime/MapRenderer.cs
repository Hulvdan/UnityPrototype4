using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public enum PickupableItemHoveringState {
    StartedHovering,
    FinishedHovering,
}

public class MapRenderer : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    const string BuildingsTilemapNameTemplate = "Gen-Buildings";
    const string ResourcesTilemapNameTemplate = "Gen-Resources";

    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _movementSystemTilemap;

    [SerializeField]
    [Required]
    Sprite WagonSprite_Right;

    [SerializeField]
    [Required]
    Sprite WagonSprite_Up;

    [SerializeField]
    [Required]
    Tilemap _previewTilemap;

    [SerializeField]
    [Required]
    Grid _grid;

    [SerializeField]
    [Required]
    TileBase _tileGrass;

    [SerializeField]
    TileBase _tileRoad;

    [SerializeField]
    [Required]
    TileBase _tileStationHorizontal;

    [SerializeField]
    [Required]
    TileBase _tileStationVertical;

    [SerializeField]
    [Required]
    TileBase _tileForest;

    [SerializeField]
    [Required]
    TileBase _tileForestTop;

    [SerializeField]
    [Required]
    GameObject _tilemapPrefab;

    [SerializeField]
    [Required]
    GameObject _tilemapBuildingsPrefab;

    [SerializeField]
    [Required]
    GameObject _humanPrefab;

    [SerializeField]
    [Required]
    ScriptableResource _logResource;

    [SerializeField]
    [Required]
    Transform _itemsLayer;

    [SerializeField]
    [Required]
    GameObject _itemPrefab;

    [Header("Setup")]
    [SerializeField]
    [Required]
    [Min(.1f)]
    float _itemPlacingDuration = 1f;

    [SerializeField]
    [Required]
    AnimationCurve _itemPlacingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    [Min(0.1f)]
    float _buildingMovingItemToTheWarehouseDuration = 1f;

    [SerializeField]
    [Min(0.1f)]
    AnimationCurve _buildingMovingItemToTheWarehouseDurationCurve =
        AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    [Required]
    GameObject _locomotivePrefab;

    [SerializeField]
    [Required]
    GameObject _wagonPrefab;

    [SerializeField]
    [Min(.1f)]
    float _sinCosScale;

    [SerializeField]
    [Min(0)]
    float _buildingScaleAmplitude = .2f;

    [Header("Inputs")]
    [SerializeField]
    [Required]
    InputActionAsset _inputActionAsset;

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    [FormerlySerializedAs("_debugTileUnwalkable")]
    [SerializeField]
    [Required]
    TileBase _debugTileUnbuildable;

    readonly List<IDisposable> _dependencyHooks = new();

    readonly Dictionary<
        Guid,
        Tuple<HorseTrain, List<Tuple<TrainNode, TrainNodeGO>>>
    > _horses = new();

    readonly Dictionary<Guid, Tuple<Human, HumanGO>> _humans = new();
    readonly Dictionary<Guid, ItemGO> _storedItems = new();

    readonly Dictionary<Guid, Tuple<TrainNode, TrainNodeGO>> _trainNodes = new();

    float _buildingScaleTimeline;
    Tilemap _buildingsTilemap;

    Camera _camera;

    GameManager _gameManager;
    InputActionMap _gameplayInputMap;

    bool _isHoveringOverItems;
    IMap _map;
    IMapSize _mapSize;
    InputAction _mouseBuildAction;
    InputAction _mouseMoveAction;
    Matrix4x4 _previewMatrix;

    Tilemap _resourceTilemap;

    public Subject<PickupableItemHoveringState> OnPickupableItemHoveringChanged { get; } = new();

    void Awake() {
        _camera = Camera.main;

        _gameplayInputMap = _inputActionAsset.FindActionMap("Gameplay");
        _mouseMoveAction = _gameplayInputMap.FindAction("PreviewMouseMove");
        _mouseBuildAction = _gameplayInputMap.FindAction("Build");
    }

    void Update() {
        UpdateHumans();
        UpdateTrains(_gameManager.currentGameSpeed);
        UpdateBuildings();

        var hoveredTile = GetHoveredTile();
        UpdateHoveringOverItems(hoveredTile);

        DisplayPreviewTile();

        // TODO: Move inputs to GameManager
        if (_mouseBuildAction.WasPressedThisFrame()) {
            if (_mapSize.Contains(hoveredTile)) {
                if (_isHoveringOverItems) {
                    _map.CollectItems(hoveredTile);
                }
                else {
                    _map.TryBuild(hoveredTile, _gameManager.selectedItem);
                }
            }
        }
        // else if (_mouseBuildAction.WasReleasedThisFrame()) {
        // }
    }

    void OnEnable() {
        _gameplayInputMap.Enable();
    }

    void OnDisable() {
        _gameplayInputMap.Disable();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;

        foreach (var building in _map.buildings) {
            if (building.scriptableBuilding == null
                || building.scriptableBuilding.tilesRadius == 0) {
                continue;
            }

            var r = building.scriptableBuilding.tilesRadius + .55f;
            var gridOffset = _grid.transform.localPosition + transform.localPosition;
            var points = new Vector3[] {
                new(r, r, 0),
                new(r, -r, 0),
                new(r, -r, 0),
                new(-r, -r, 0),
                new(-r, -r, 0),
                new(-r, r, 0),
                new(-r, r, 0),
                new(r, r, 0),
            };
            for (var i = 0; i < points.Length; i++) {
                var point = points[i];
                point.x += building.posX + gridOffset.x + .5f;
                point.y += building.posY + gridOffset.y + .5f;
                points[i] = point;
            }

            Gizmos.DrawLineList(points);
        }
    }

    void UpdateBuildings() {
        _buildingScaleTimeline += Time.deltaTime * _gameManager.currentGameSpeed;
        if (_buildingScaleTimeline >= 2 * Mathf.PI * _sinCosScale) {
            _buildingScaleTimeline -= 2 * Mathf.PI * _sinCosScale;
        }

        foreach (var building in _map.buildings) {
            if (building.scriptableBuilding.type != BuildingType.Produce) {
                continue;
            }

            var scale = GetBuildingScale(
                building.IsProcessing,
                building.ProcessingElapsed,
                building.scriptableBuilding.ItemProcessingDuration
            );
            SetBuilding(building, scale.x, scale.y);
        }
    }

    Vector2 GetBuildingScale(
        bool isProcessing,
        float buildingProcessingElapsed,
        float scriptableBuildingItemProcessingDuration
    ) {
        if (!isProcessing || buildingProcessingElapsed == 0) {
            return new(1, 1);
        }

        return new(
            1 + _buildingScaleAmplitude * Mathf.Sin(_buildingScaleTimeline * _sinCosScale),
            1 + _buildingScaleAmplitude * Mathf.Cos(_buildingScaleTimeline * _sinCosScale)
        );
    }

    void UpdateHoveringOverItems(Vector2Int hoveredTile) {
        var shouldStopHovering = false;
        if (_mapSize.Contains(hoveredTile)) {
            if (_map.CellContainsPickupableItems(hoveredTile)) {
                if (!_isHoveringOverItems) {
                    OnPickupableItemHoveringChanged.OnNext(
                        PickupableItemHoveringState.StartedHovering
                    );
                    _isHoveringOverItems = true;
                }
            }
            else if (_isHoveringOverItems) {
                shouldStopHovering = true;
            }
        }
        else if (_isHoveringOverItems) {
            shouldStopHovering = true;
        }

        if (shouldStopHovering && _isHoveringOverItems) {
            OnPickupableItemHoveringChanged.OnNext(
                PickupableItemHoveringState.FinishedHovering
            );
            _isHoveringOverItems = false;
        }
    }

    public void InitDependencies(GameManager gameManager, IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
        _gameManager = gameManager;

        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();
        _dependencyHooks.Add(_gameManager.OnSelectedItemChanged.Subscribe(OnSelectedItemChanged));
        _dependencyHooks.Add(_map.onElementTileChanged.Subscribe(OnElementTileChanged));

        _dependencyHooks.Add(_map.onHumanCreated.Subscribe(OnHumanCreated));
        _dependencyHooks.Add(_map.onHumanPickedUpResource.Subscribe(OnHumanPickedUpResource));
        _dependencyHooks.Add(_map.onHumanPlacedResource.Subscribe(OnHumanPlacedResource));

        _dependencyHooks.Add(_map.onTrainCreated.Subscribe(OnTrainCreated));
        _dependencyHooks.Add(_map.onTrainNodeCreated.Subscribe(OnTrainNodeCreated));
        _dependencyHooks.Add(_map.onTrainPickedUpResource.Subscribe(OnTrainPickedUpResource));
        _dependencyHooks.Add(_map.onTrainPushedResource.Subscribe(OnTrainPushedResource));

        _dependencyHooks.Add(
            _map.onBuildingStartedProcessing.Subscribe(OnBuildingStartedProcessing)
        );
        _dependencyHooks.Add(_map.onBuildingProducedItem.Subscribe(OnBuildingProducedItem));
        _dependencyHooks.Add(
            _map.onProducedResourcesPickedUp.Subscribe(OnProducedResourcesPickedUp)
        );
    }

    void OnProducedResourcesPickedUp(E_ProducedResourcesPickedUp data) {
        foreach (var res in data.Resources) {
            Destroy(_storedItems[res.id].gameObject);
            _storedItems.Remove(res.id);
        }
    }

    void OnBuildingStartedProcessing(E_BuildingStartedProcessing data) {
        Destroy(_storedItems[data.Resource.id].gameObject);
        _storedItems.Remove(data.Resource.id);
    }

    void OnBuildingProducedItem(E_BuildingProducedItem data) {
        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.Building;
        var scriptable = building.scriptableBuilding;

        var i = (building.producedResources.Count - 1) % scriptable.producedItemsPositions.Count;
        var itemOffset = scriptable.producedItemsPositions[i];
        item.transform.localPosition = (Vector2)building.position;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.script);

        DOTween.To(
            () => item.transform.localPosition,
            val => item.transform.localPosition = val,
            (Vector3)(building.position + itemOffset + Vector2.right / 2),
            _buildingMovingItemToTheWarehouseDuration / _gameManager.currentGameSpeed
        ).SetEase(_buildingMovingItemToTheWarehouseDurationCurve);

        _storedItems.Add(data.Resource.id, itemGo);
    }

    void OnSelectedItemChanged(SelectedItem item) {
    }

    void OnElementTileChanged(Vector2Int pos) {
        var elementTile = _map.elementTiles[pos.y][pos.x];

        TileBase tile;
        switch (elementTile.Type) {
            case ElementTileType.Road:
                tile = _tileRoad;
                break;
            case ElementTileType.Station:
                tile = elementTile.Rotation == 0 ? _tileStationHorizontal : _tileStationVertical;
                break;
            case ElementTileType.None:
                return;
            default:
                return;
        }

        _movementSystemTilemap.SetTile(new(pos.x, pos.y), tile);

        var debugTile = _map.IsBuildable(pos.x, pos.y) ? null : _debugTileUnbuildable;
        _debugTilemap.SetTile(new(pos.x, pos.y, 0), debugTile);
        foreach (var offset in DirectionOffsets.Offsets) {
            var newPos = pos + offset;
            if (!_mapSize.Contains(newPos)) {
                continue;
            }

            debugTile = _map.IsBuildable(newPos.x, newPos.y) ? null : _debugTileUnbuildable;
            _debugTilemap.SetTile(new(newPos.x, newPos.y, 0), debugTile);
        }
    }

    void UpdateTileBasedOnRemainingResourcePercent(
        Vector2Int pos,
        float dataRemainingAmountPercent
    ) {
        if (dataRemainingAmountPercent > 0) {
            return;
        }

        _resourceTilemap.SetTile(new(pos.x, pos.y, 0), null);
        _resourceTilemap.SetTile(new(pos.x, pos.y, -1), null);
    }

    public void ResetRenderer() {
        DeleteOldTilemaps();
        RegenerateTilemapGameObject();
        RegenerateDebugTilemapGameObject();
        UpdateGridPosition();
    }

    void DeleteOldTilemaps() {
        foreach (Transform child in _grid.transform) {
            if (child.gameObject.name.StartsWith(TerrainTilemapNameTemplate)
                || child.gameObject.name.StartsWith(BuildingsTilemapNameTemplate)
                || child.gameObject.name.StartsWith(ResourcesTilemapNameTemplate)) {
                child.gameObject.SetActive(false);
            }
        }
    }

    void RegenerateTilemapGameObject() {
        var maxHeight = 0;
        for (var y = 0; y < _mapSize.sizeY; y++) {
            for (var x = 0; x < _mapSize.sizeX; x++) {
                maxHeight = Math.Max(maxHeight, _map.terrainTiles[y][x].Height);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain = GenerateTilemap(i, i, TerrainTilemapNameTemplate, _tilemapPrefab);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _mapSize.sizeY; y++) {
                if (h > 0 && y == _mapSize.sizeY) {
                    continue;
                }

                for (var x = 0; x < _mapSize.sizeX; x++) {
                    if (
                        h == 0
                        || _map.terrainTiles[y][x].Height >= h
                        || (y > 0 && _map.terrainTiles[y - 1][x].Height == h)
                    ) {
                        terrainMaps[h].SetTile(new(x, y, 0), _tileGrass);
                    }
                }
            }
        }

        _resourceTilemap = GenerateTilemap(
            0, maxHeight + 1, ResourcesTilemapNameTemplate, _tilemapPrefab
        ).GetComponent<Tilemap>();
        for (var y = 0; y < _mapSize.sizeY; y++) {
            for (var x = 0; x < _mapSize.sizeX; x++) {
                if (_map.terrainTiles[y][x].Resource != null
                    && _map.terrainTiles[y][x].Resource.name == _logResource.name) {
                    _resourceTilemap.SetTile(new(x, y, 0), _tileForest);
                    _resourceTilemap.SetTile(new(x, y, -1), _tileForestTop);
                }
            }
        }

        _buildingsTilemap = GenerateTilemap(
            0, maxHeight + 2, BuildingsTilemapNameTemplate, _tilemapBuildingsPrefab
        ).GetComponent<Tilemap>();
        foreach (var building in _map.buildings) {
            SetBuilding(building, 1, 1);
        }
    }

    void SetBuilding(Building building, float scaleX, float scaleY) {
        var widthOffset = (building.scriptableBuilding.size.x - 1) / 2f;
        var heightOffset = (building.scriptableBuilding.size.y - 1) / 2f;

        _buildingsTilemap.SetTile(
            new(
                new(building.posX, building.posY, 0),
                building.scriptableBuilding.tile,
                Color.white,
                Matrix4x4.TRS(
                    new(widthOffset, heightOffset),
                    Quaternion.identity,
                    new(scaleX, scaleY, 1)
                )
            ),
            false
        );
    }

    void RegenerateDebugTilemapGameObject() {
        _debugTilemap.ClearAllTiles();

        for (var y = 0; y < _mapSize.sizeY; y++) {
            for (var x = 0; x < _mapSize.sizeX; x++) {
                if (!_map.IsBuildable(x, y)) {
                    _debugTilemap.SetTile(new(x, y, 0), _debugTileUnbuildable);
                }
            }
        }
    }

    GameObject GenerateTilemap(int i, float order, string nameTemplate, GameObject prefabTemplate) {
        var terrainTilemap = Instantiate(prefabTemplate, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.localPosition = new(0, -order / 100000f, 0);
        return terrainTilemap;
    }

    void UpdateGridPosition() {
        _grid.transform.localPosition = new(-_mapSize.sizeX / 2f, -_mapSize.sizeY / 2f);
    }

    void DisplayPreviewTile() {
        _previewTilemap.ClearAllTiles();

        var tile = GetHoveredTile();
        if (!_mapSize.Contains(tile)) {
            return;
        }

        TileBase tilemapTile;
        if (_gameManager.selectedItem == SelectedItem.Road) {
            tilemapTile = _tileRoad;
        }
        else if (_gameManager.selectedItem == SelectedItem.Station) {
            tilemapTile = _gameManager.selectedItemRotation % 2 == 0
                ? _tileStationVertical
                : _tileStationHorizontal;
        }
        else {
            return;
        }

        _previewTilemap.SetTile(
            new(new(tile.x, tile.y, 0), tilemapTile, Color.white, _previewMatrix), false
        );
    }

    Vector2Int GetHoveredTile() {
        var mousePos = _mouseMoveAction.ReadValue<Vector2>();
        var wPos = _camera.ScreenToWorldPoint(mousePos);
        return (Vector2Int)_previewTilemap.WorldToCell(wPos);
    }

    #region HumanSystem

    void OnHumanCreated(E_HumanCreated data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        _humans.Add(data.Human.ID, Tuple.Create(data.Human, go.GetComponent<HumanGO>()));
    }

    void OnHumanPickedUpResource(E_HumanPickedUpResource data) {
        UpdateTileBasedOnRemainingResourcePercent(
            data.ResourceTilePosition,
            data.RemainingAmountPercent
        );

        _humans[data.Human.ID].Item2.OnPickedUpResource(
            data.Resource.script, _gameManager.currentGameSpeed
        );
    }

    void OnHumanPlacedResource(E_HumanPlacedResource data) {
        _humans[data.Human.ID].Item2.OnPlacedResource(_gameManager.currentGameSpeed);

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.StoreBuilding;
        var scriptable = building.scriptableBuilding;

        var i = building.storedResources.Count - 1;
        if (i >= scriptable.storedItemPositions.Count) {
            Debug.LogError("WTF i >= scriptable.storedItemPositions.Count");
            i = scriptable.storedItemPositions.Count - 1;
        }

        var itemOffset = scriptable.storedItemPositions[i];
        item.transform.localPosition = building.position + itemOffset;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.script);

        _storedItems.Add(data.Resource.id, itemGo);
    }

    Vector3 GameLogicToRenderPos(Vector2 pos) {
        return _grid.LocalToWorld(pos) + new Vector3(.5f, .5f, 0);
    }

    void UpdateHumans() {
        foreach (var (human, go) in _humans.Values) {
            go.transform.position = GameLogicToRenderPos(human.position);
        }
    }

    #endregion

    #region TrainSystem

    void OnTrainCreated(E_TrainCreated data) {
        _horses.Add(data.Horse.ID, new(data.Horse, new()));
    }

    void OnTrainNodeCreated(E_TrainNodeCreated data) {
        var go = Instantiate(data.IsLocomotive ? _locomotivePrefab : _wagonPrefab, _grid.transform);
        var trainNodeGo = go.GetComponent<TrainNodeGO>();
        _horses[data.Horse.ID].Item2.Add(new(data.Node, trainNodeGo));
        _trainNodes.Add(data.Node.ID, new(data.Node, trainNodeGo));
    }

    void OnTrainPickedUpResource(E_TrainPickedUpResource data) {
        var resID = data.Resource.id;
        Destroy(_storedItems[resID].gameObject);
        _storedItems.Remove(resID);

        _trainNodes[data.TrainNode.ID].Item2.OnPickedUpResource(
            data.Resource.script, data.ResourceSlotPosition, _gameManager.currentGameSpeed
        );
    }

    void OnTrainPushedResource(E_TrainPushedResource data) {
        _trainNodes[data.TrainNode.ID].Item2.OnPushedResource();

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.Building;
        var scriptable = building.scriptableBuilding;

        var i = (building.storedResources.Count - 1) % scriptable.storedItemPositions.Count;
        var itemOffset = scriptable.storedItemPositions[i];

        item.transform.localPosition = data.TrainNode.Position + Vector2.one / 2f;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.script);

        var resId = data.Resource.id;
        _storedItems.Add(resId, itemGo);

        DOTween
            .To(
                () => item.transform.localPosition,
                val => item.transform.localPosition = val,
                (Vector3)(building.position + itemOffset + Vector2.right / 2),
                _itemPlacingDuration / _gameManager.currentGameSpeed
            )
            .SetEase(_itemPlacingCurve)
            .OnComplete(() => {
                if (data.StoreResourceResult == StoreResourceResult.AddedToProcessingImmediately) {
                    Destroy(_storedItems[resId].gameObject);
                    _storedItems.Remove(resId);
                }
            });
    }

    void UpdateTrains(float gameSpeed) {
        foreach (var horse in _horses.Values) {
            foreach (var (trainNode, go) in horse.Item2) {
                go.transform.position = GameLogicToRenderPos(trainNode.Position);

                if (trainNode.isLocomotive) {
                    var animator = go.LocomotiveAnimator;
                    animator.SetFloat("speedX", Mathf.Cos(Mathf.Deg2Rad * trainNode.Rotation));
                    animator.SetFloat("speedY", Mathf.Sin(Mathf.Deg2Rad * trainNode.Rotation));
                    animator.SetFloat("speed", horse.Item1.NormalisedSpeed * gameSpeed);
                }
                else {
                    if (trainNode.Rotation == 0 || Math.Abs(trainNode.Rotation - 180) < 0.001f) {
                        go.MainSpriteRenderer.sprite = WagonSprite_Right;
                    }
                    else {
                        go.MainSpriteRenderer.sprite = WagonSprite_Up;
                    }
                }

                go.MainSpriteRenderer.flipX = Math.Abs(trainNode.Rotation - 180) < 0.001f;
            }
        }
    }

    #endregion
}
}

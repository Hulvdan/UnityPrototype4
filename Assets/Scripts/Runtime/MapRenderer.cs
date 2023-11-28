using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Extensions;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
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
    Tilemap _flagsTilemap;

    [FormerlySerializedAs("WagonSprite_Right")]
    [SerializeField]
    [Required]
    Sprite _wagonSprite_Right;

    [FormerlySerializedAs("WagonSprite_Up")]
    [SerializeField]
    [Required]
    Sprite _wagonSprite_Up;

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
    TileBase _tileUnfinishedBuilding;

    [SerializeField]
    [Required]
    TileBase _tileFlag;

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
    ScriptableResource _planksResource;

    [SerializeField]
    [Required]
    Transform _itemsLayer;

    [SerializeField]
    [Required]
    GameObject _itemPrefab;

    [SerializeField]
    [Required]
    GameObject _locomotivePrefab;

    [SerializeField]
    [Required]
    GameObject _wagonPrefab;

    [SerializeField]
    [Required]
    Transform _buildingModalsContainer;

    [SerializeField]
    [Required]
    [AssetsOnly]
    Stable_Panel _stablesModalPrefab;

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
    [Min(.1f)]
    float _sinCosScale;

    [SerializeField]
    [Min(0)]
    float _buildingScaleAmplitude = .2f;

    [SerializeField]
    Color _unbuildableTileColor = Color.red;

    [SerializeField]
    [Required]
    MovementPattern _movementPattern;

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

    readonly Dictionary<Guid, (Human, HumanGO)> _humans = new();
    readonly Dictionary<Guid, (HumanTransporter, HumanGO, HumanBinding)> _humanTransporters = new();

    readonly Dictionary<Guid, GameObject> _modals = new();
    readonly Dictionary<Guid, ItemGO> _storedItems = new();

    readonly Dictionary<Guid, (TrainNode, TrainNodeGO)> _trainNodes = new();

    float _buildingScaleTimeline;
    Tilemap _buildingsTilemap;

    Camera _camera;

    GameManager _gameManager;
    InputActionMap _gameplayInputMap;
    Building _hoveredBuilding;

    bool _isHoveringOverItems;
    IMap _map;
    IMapSize _mapSize;
    InputAction _mouseBuildAction;
    InputAction _mouseMoveAction;
    Matrix4x4 _previewMatrix;

    Tilemap _resourceTilemap;

    public Subject<PickupableItemHoveringState> onPickupableItemHoveringChanged { get; } = new();

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

        var hoveredTile = GetHoveredTilePos();
        UpdateHoveringState(hoveredTile);

        DisplayPreviewTile();

        // TODO: Move inputs to GameManager
        if (_mouseBuildAction.WasPressedThisFrame() && _mapSize.Contains(hoveredTile)) {
            if (_isHoveringOverItems) {
                _map.CollectItems(hoveredTile);
            }
            else if (_gameManager.SelectedItem == null && _hoveredBuilding != null) {
                if (_hoveredBuilding.scriptable.type == BuildingType.SpecialStable) {
                    ToggleStablesPanel(_hoveredBuilding);
                }
            }
            else if (
                _gameManager.SelectedItem != null
                && _map.CanBePlaced(hoveredTile, _gameManager.SelectedItem.Type)
            ) {
                _map.TryBuild(hoveredTile, _gameManager.SelectedItem);
            }
        }
    }

    void OnEnable() {
        _gameplayInputMap.Enable();
    }

    void OnDisable() {
        _gameplayInputMap.Disable();
    }

    void OnDrawGizmos() {
        if (_map == null) {
            return;
        }

        Gizmos.color = Color.cyan;

        foreach (var building in _map.buildings) {
            if (
                building.scriptable == null
                || building.scriptable.type == BuildingType.Harvest
                || building.scriptable.tilesRadius == 0
            ) {
                continue;
            }

            var r = building.scriptable.tilesRadius + .45f;
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
                point.x += building.posX + .5f;
                point.y += building.posY + .5f;
                points[i] = _grid.transform.TransformPoint(point);
            }

            Gizmos.DrawLineList(points);
        }

        foreach (var (human, _, _) in _humanTransporters.Values) {
            Gizmos.color = Color.cyan;
            var offset = Vector2.one / 2;
            Gizmos.DrawSphere(_grid.transform.TransformPoint(human.pos + offset), .2f);

            if (human.movingTo != null) {
                Gizmos.color = Color.red;
                var humanMovingFrom = human.movingFrom + offset;
                var humanMovingTo = human.movingTo.Value + offset;
                Gizmos.DrawLine(
                    _grid.transform.TransformPoint(humanMovingFrom),
                    _grid.transform.TransformPoint(humanMovingTo)
                );
                Gizmos.DrawSphere(
                    _grid.transform.TransformPoint(
                        Vector2.Lerp(
                            humanMovingFrom,
                            humanMovingTo,
                            human.movingNormalized
                        )
                    ),
                    .2f
                );
            }
        }

        var clri = 0;
        var colors = new[] {
            Color.white, Color.red, Color.cyan, Color.green, Color.magenta, Color.yellow,
        };
        foreach (var segment in _map.segments) {
            Gizmos.color = colors[clri];

            for (var y = 0; y < segment.Graph.height; y++) {
                for (var x = 0; x < segment.Graph.width; x++) {
                    var node = segment.Graph.Nodes[y][x];
                    if (node == 0) {
                        continue;
                    }

                    var pos = new Vector3(x, y) + Vector3.one / 2 +
                              new Vector3(segment.Graph.Offset.x, segment.Graph.Offset.y);
                    if (GraphNode.IsRight(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.right / 2)
                        );
                    }

                    if (GraphNode.IsUp(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.up / 2)
                        );
                    }

                    if (GraphNode.IsLeft(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.left / 2)
                        );
                    }

                    if (GraphNode.IsDown(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.down / 2)
                        );
                    }
                }
            }

            clri++;
            if (clri >= colors.Length) {
                clri = 0;
            }
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
        InitializeDependencyHooks();
    }

    void InitializeDependencyHooks() {
        var hooks = _dependencyHooks;

        hooks.Add(_gameManager.OnSelectedItemChanged.Subscribe(
            OnSelectedItemChanged));
        hooks.Add(_map.onElementTileChanged.Subscribe(
            OnElementTileChanged));

        hooks.Add(_map.onHumanCreated.Subscribe(
            OnHumanCreated));
        hooks.Add(_map.onHumanTransporterCreated.Subscribe(
            OnHumanTransporterCreated));
        hooks.Add(_map.onHumanPickedUpResource.Subscribe(
            OnHumanPickedUpResource));
        hooks.Add(_map.onHumanPlacedResource.Subscribe(
            OnHumanPlacedResource));
        hooks.Add(_map.onHumanReachedCityHall.Subscribe(
            OnHumanReachedCityHall));

        hooks.Add(_map.onHumanTransporterMovedToTheNextTile.Subscribe(
            OnHumanTransporterMovedToTheNextTile));
        hooks.Add(_map.onHumanTransporterStartedPickingUpResource.Subscribe(
            OnHumanTransporterStartedPickingUpResource));
        hooks.Add(_map.onHumanTransporterPickedUpResource.Subscribe(
            OnHumanTransporterPickedUpResource));
        hooks.Add(_map.onHumanTransporterStartedPlacingResource.Subscribe(
            OnHumanTransporterStartedPlacingResource));
        hooks.Add(_map.onHumanTransporterPlacedResource.Subscribe(
            OnHumanTransporterPlacedResource));

        hooks.Add(_map.onTrainCreated.Subscribe(
            OnTrainCreated));
        hooks.Add(_map.onTrainNodeCreated.Subscribe(
            OnTrainNodeCreated));
        hooks.Add(_map.onTrainPickedUpResource.Subscribe(
            OnTrainPickedUpResource));
        hooks.Add(_map.onTrainPushedResource.Subscribe(
            OnTrainPushedResource));

        hooks.Add(_map.onBuildingPlaced.Subscribe(
            OnBuildingPlaced));

        hooks.Add(_map.onBuildingStartedProcessing.Subscribe(
            OnBuildingStartedProcessing));
        hooks.Add(_map.onBuildingProducedItem.Subscribe(
            OnBuildingProducedItem));
        hooks.Add(_map.onProducedResourcesPickedUp.Subscribe(
            OnProducedResourcesPickedUp));
    }

    void OnHumanTransporterMovedToTheNextTile(E_HumanTransporterMovedToTheNextTile data) {
        var (human, go, binding) = _humanTransporters[data.Human.ID];
        go.transform.localPosition = new Vector2(human.pos.x, human.pos.y) + Vector2.one / 2;

        binding.CurvePerFeedback.Clear();
        foreach (var feedback in _movementPattern.Feedbacks) {
            binding.CurvePerFeedback.Add(feedback.GetRandomCurve());
        }
    }

    void OnHumanTransporterPlacedResource(E_HumanTransporterPlacedResource data) {
        var (human, go, _) = _humanTransporters[data.Human.ID];
        go.OnStoppedPlacingResource(data.Resource.Scriptable);

        var item = Instantiate(_itemPrefab, _itemsLayer);
        item.transform.localPosition = new Vector2(human.pos.x, human.pos.y) + Vector2.one / 2;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.Scriptable);

        _storedItems.Add(data.Resource.ID, itemGo);
    }

    void OnHumanTransporterPickedUpResource(E_HumanTransporterPickedUpResource data) {
        var (human, go, _) = _humanTransporters[data.Human.ID];
        go.OnStoppedPickingUpResource(data.Resource.Scriptable);

        if (_storedItems.TryGetValue(data.Resource.ID, out var itemGo)) {
            Destroy(itemGo.gameObject);
            _storedItems.Remove(data.Resource.ID);
        }
    }

    void OnHumanTransporterStartedPlacingResource(
        E_HumanTransporterStartedPlacingResource data
    ) {
        var (human, go, t) = _humanTransporters[data.Human.ID];
        go.OnStartedPlacingResource(data.Resource.Scriptable);
    }

    void OnHumanTransporterStartedPickingUpResource(
        E_HumanTransportedStartedPickingUpResource data
    ) {
        var (human, go, _) = _humanTransporters[data.Human.ID];
        go.OnStartedPickingUpResource(data.Resource.Scriptable);

        if (_storedItems.TryGetValue(data.Resource.ID, out var res)) {
            Destroy(res.gameObject);
            _storedItems.Remove(data.Resource.ID);
        }
    }

    void OnBuildingPlaced(E_BuildingPlaced data) {
        SetBuilding(data.Building, 1, 1);
    }

    void ToggleStablesPanel(Building building) {
        foreach (var modal in _modals.Values) {
            if (modal.GetComponent<Stable_Panel>().building == building) {
                modal.GetComponent<Stable_Panel>().Close();
                return;
            }
        }

        var createdModal = Instantiate(_stablesModalPrefab, _buildingModalsContainer);
        var panel = createdModal.GetComponent<Stable_Panel>();
        panel.Init(Guid.NewGuid(), _map, _gameManager, building, new() { new(1, _planksResource) });
        panel.OnCreateHorse += _map.OnCreateHorse;
        panel.OnClose += OnModalClose;
        _modals.Add(panel.id, createdModal.gameObject);
    }

    void OnModalClose(Stable_Panel panel) {
        _modals.Remove(panel.id);
        Destroy(panel.gameObject);
    }

    void UpdateBuildings() {
        _buildingScaleTimeline += Time.deltaTime * _gameManager.currentGameSpeed;
        if (_buildingScaleTimeline >= 2 * Mathf.PI * _sinCosScale) {
            _buildingScaleTimeline -= 2 * Mathf.PI * _sinCosScale;
        }

        foreach (var building in _map.buildings) {
            if (building.scriptable.type != BuildingType.Produce) {
                continue;
            }

            var scale = GetBuildingScale(
                building.IsProducing,
                building.ProducingElapsed,
                building.scriptable.ItemProcessingDuration
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

    void UpdateHoveringState(Vector2Int hoveredTile) {
        _hoveredBuilding = null;
        foreach (var building in _map.buildings) {
            if (building.Contains(hoveredTile)) {
                _hoveredBuilding = building;
                break;
            }
        }

        var shouldStopHovering = false;
        if (_mapSize.Contains(hoveredTile)) {
            if (_map.CellContainsPickupableItems(hoveredTile)) {
                if (!_isHoveringOverItems) {
                    onPickupableItemHoveringChanged.OnNext(
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
            onPickupableItemHoveringChanged.OnNext(
                PickupableItemHoveringState.FinishedHovering
            );
            _isHoveringOverItems = false;
        }
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
        var scriptable = building.scriptable;

        var i = (building.producedResources.Count - 1) % scriptable.producedItemsPositions.Count;
        var itemOffset = scriptable.producedItemsPositions[i];
        item.transform.localPosition = (Vector2)building.pos;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.script);

        DOTween
            .To(
                () => item.transform.localPosition,
                val => item.transform.localPosition = val,
                (Vector3)(building.pos + itemOffset + Vector2.right / 2),
                _buildingMovingItemToTheWarehouseDuration / _gameManager.currentGameSpeed
            )
            .SetLink(item)
            .SetEase(_buildingMovingItemToTheWarehouseDurationCurve);

        _storedItems.Add(data.Resource.id, itemGo);
    }

    void OnSelectedItemChanged(SelectedItemType itemType) {
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
            case ElementTileType.Flag:
                _flagsTilemap.SetTile(new(pos.x, pos.y), _tileFlag);
                return;
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
        RegenerateTilemapGameObjects();
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

    void RegenerateTilemapGameObjects() {
        var maxHeight = 0;
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                maxHeight = Math.Max(maxHeight, _map.terrainTiles[y][x].Height);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain = GenerateTerrainTilemap(i, i, TerrainTilemapNameTemplate, _tilemapPrefab);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _mapSize.height; y++) {
                if (h > 0 && y == _mapSize.height) {
                    continue;
                }

                for (var x = 0; x < _mapSize.width; x++) {
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

        _resourceTilemap = GenerateTerrainTilemap(
                0, maxHeight + 1, ResourcesTilemapNameTemplate, _tilemapPrefab
            )
            .GetComponent<Tilemap>();
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                if (_map.terrainTiles[y][x].Resource != null
                    && _map.terrainTiles[y][x].Resource.name == _logResource.name) {
                    _resourceTilemap.SetTile(new(x, y, 0), _tileForest);
                    _resourceTilemap.SetTile(new(x, y, -1), _tileForestTop);
                }
            }
        }

        _buildingsTilemap = GenerateTerrainTilemap(
                0, maxHeight + 2, BuildingsTilemapNameTemplate, _tilemapBuildingsPrefab
            )
            .GetComponent<Tilemap>();
        foreach (var building in _map.buildings) {
            SetBuilding(building, 1, 1);
        }
    }

    void SetBuilding(Building building, float scaleX, float scaleY) {
        var widthOffset = (building.scriptable.size.x - 1) / 2f;
        var heightOffset = (building.scriptable.size.y - 1) / 2f;

        var tile = building.scriptable.tile;
        if (building.BuildingProgress < 1) {
            tile = _tileUnfinishedBuilding;
        }

        _buildingsTilemap.SetTile(
            new(
                new(building.posX, building.posY, 0),
                tile,
                Color.white,
                Matrix4x4.TRS(
                    new(widthOffset, heightOffset),
                    Quaternion.identity,
                    new(scaleX, scaleY, 1)
                )
            ),
            false
        );

        _movementSystemTilemap.SetTile(new(building.posX, building.posY, 0), _tileRoad);
    }

    void RegenerateDebugTilemapGameObject() {
        _debugTilemap.ClearAllTiles();

        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                if (!_map.IsBuildable(x, y)) {
                    _debugTilemap.SetTile(new(x, y, 0), _debugTileUnbuildable);
                }
            }
        }
    }

    GameObject GenerateTerrainTilemap(
        int i,
        float order,
        string nameTemplate,
        GameObject prefabTemplate
    ) {
        var terrainTilemap = Instantiate(prefabTemplate, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.localPosition = new(0, -order / 100000f, 0);
        return terrainTilemap;
    }

    void UpdateGridPosition() {
        _grid.transform.localPosition = new(-_mapSize.width / 2f, -_mapSize.height / 2f);
    }

    void DisplayPreviewTile() {
        _previewTilemap.ClearAllTiles();

        var pos = GetHoveredTilePos();
        if (!_mapSize.Contains(pos)) {
            return;
        }

        TileBase tilemapTile;
        var item = _gameManager.SelectedItem;
        if (item == null) {
            return;
        }

        switch (item.Type) {
            case SelectedItemType.Road:
                tilemapTile = _tileRoad;
                break;
            case SelectedItemType.Flag:
                tilemapTile = _tileFlag;
                break;
            case SelectedItemType.Station:
                tilemapTile = _gameManager.selectedItemRotation % 2 == 0
                    ? _tileStationVertical
                    : _tileStationHorizontal;
                break;
            case SelectedItemType.Building:
                Assert.IsNotNull(item.Building);
                tilemapTile = item.Building.tile;
                break;
            default:
                return;
        }

        var buildable = _map.CanBePlaced(pos, item.Type);
        if (!buildable) {
            return;
        }

        var matrix = Matrix4x4.identity;
        if (item.Type == SelectedItemType.Building) {
            matrix = Matrix4x4.TRS(new(0, -0.5f, 0), Quaternion.identity, Vector3.one);
        }

        _previewTilemap.SetTile(
            new(
                new(pos.x, pos.y, 0),
                tilemapTile,
                buildable ? Color.white : _unbuildableTileColor,
                matrix
            ),
            false
        );
    }

    Vector2Int GetHoveredTilePos() {
        var mousePos = _mouseMoveAction.ReadValue<Vector2>();
        var wPos = _camera.ScreenToWorldPoint(mousePos);
        return (Vector2Int)_previewTilemap.WorldToCell(wPos);
    }

    #region HumanSystem

    void OnHumanCreated(E_HumanCreated data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        _humans.Add(data.Human.ID, new(data.Human, go.GetComponent<HumanGO>()));
    }

    void OnHumanTransporterCreated(E_HumanTransporterCreated data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        var humanGo = go.GetComponent<HumanGO>();

        var movementBinding = new HumanBinding {
            CurvePerFeedback = new() { Capacity = _movementPattern.Feedbacks.Count },
        };
        foreach (var feedback in _movementPattern.Feedbacks) {
            movementBinding.CurvePerFeedback.Add(feedback.GetRandomCurve());
        }

        _humanTransporters.Add(data.Human.ID, new(data.Human, humanGo, movementBinding));
        UpdateHumanTransporter(data.Human, humanGo, movementBinding);
    }

    void OnHumanPickedUpResource(E_HumanPickedUpResource data) {
        UpdateTileBasedOnRemainingResourcePercent(
            data.ResourceTilePosition,
            data.RemainingAmountPercent
        );

        _humans[data.Human.ID]
            .Item2.OnPickedUpResource(
                data.Resource.script, _gameManager.currentGameSpeed
            );
    }

    void OnHumanPlacedResource(E_HumanPlacedResource data) {
        _humans[data.Human.ID].Item2.OnPlacedResource(_gameManager.currentGameSpeed);

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.StoreBuilding;
        var scriptable = building.scriptable;

        var i = building.storedResources.Count - 1;
        if (i >= scriptable.storedItemPositions.Count) {
            Debug.LogError("WTF i >= scriptable.storedItemPositions.Count");
            i = scriptable.storedItemPositions.Count - 1;
        }

        var itemOffset = scriptable.storedItemPositions[i];
        item.transform.localPosition = building.pos + itemOffset;
        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.script);

        _storedItems.Add(data.Resource.id, itemGo);
    }

    void OnHumanReachedCityHall(E_HumanReachedCityHall data) {
        var human = _humanTransporters[data.Human.ID];
        Destroy(human.Item2.gameObject);
        _humanTransporters.Remove(data.Human.ID);
    }

    void UpdateHumans() {
        foreach (var (human, go) in _humans.Values) {
            go.transform.localPosition = human.position + Vector2.one / 2;
        }

        foreach (var (human, go, binding) in _humanTransporters.Values) {
            UpdateHumanTransporter(human, go, binding);
        }
    }

    void UpdateHumanTransporter(HumanTransporter human, HumanGO go, HumanBinding binding) {
        if (human.movingTo == null) {
            go.transform.localPosition = human.movingFrom;
        }
        else {
            for (var i = 0; i < _movementPattern.Feedbacks.Count; i++) {
                var feedback = _movementPattern.Feedbacks[i];
                var curve = binding.CurvePerFeedback[i];
                var coef = curve.Evaluate(human.movingNormalized);

                feedback.UpdateData(
                    Time.deltaTime,
                    human.movingNormalized,
                    coef,
                    human.movingFrom,
                    human.movingTo.Value,
                    go.gameObject
                );
            }
        }

        go.transform.localPosition += Vector3.one.With(z: 0) / 2;
        if (human.stateMovingResource
            == HumanTransporter_MovingResource_Controller.State.PickingUpResource) {
            go.SetPickingUpResourceCoef(human.stateMovingResource_pickingUpResourceNormalized);
        }

        if (human.stateMovingResource
            == HumanTransporter_MovingResource_Controller.State.PlacingResource) {
            go.SetPlacingResourceCoef(human.stateMovingResource_placingResourceNormalized);
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

        _trainNodes[data.TrainNode.ID]
            .Item2.OnPickedUpResource(
                data.Resource.script, data.ResourceSlotPosition, _gameManager.currentGameSpeed
            );
    }

    void OnTrainPushedResource(E_TrainPushedResource data) {
        _trainNodes[data.TrainNode.ID].Item2.OnPushedResource();

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.Building;
        var scriptable = building.scriptable;

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
                (Vector3)(building.pos + itemOffset + Vector2.right / 2),
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
                go.transform.localPosition = trainNode.Position + Vector2.one / 2;

                if (trainNode.isLocomotive) {
                    var animator = go.LocomotiveAnimator;
                    animator.SetFloat("speedX", Mathf.Cos(Mathf.Deg2Rad * trainNode.Rotation));
                    animator.SetFloat("speedY", Mathf.Sin(Mathf.Deg2Rad * trainNode.Rotation));
                    animator.SetFloat("speed", horse.Item1.NormalisedSpeed * gameSpeed);
                }
                else {
                    if (trainNode.Rotation == 0 || Math.Abs(trainNode.Rotation - 180) < 0.001f) {
                        go.MainSpriteRenderer.sprite = _wagonSprite_Right;
                    }
                    else {
                        go.MainSpriteRenderer.sprite = _wagonSprite_Up;
                    }
                }

                go.MainSpriteRenderer.flipX = Math.Abs(trainNode.Rotation - 180) < 0.001f;
            }
        }
    }

    #endregion
}
}

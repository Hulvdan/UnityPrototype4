using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public class RaycastBridge : EventTrigger {
    [SerializeField]
    [Required]
    InputActionAsset _inputs;

    [SerializeField]
    [Required]
    Tilemap _previewTilemap;

    Camera _camera;

    InputActionMap _map;
    InputAction _mouseMoveAction;

    public void InitDependencies(MapRenderer mapRenderer) {
        _mapRenderer = mapRenderer;
    }

    public void Awake() {
        _camera = Camera.main;

        _map = _inputs.FindActionMap("Gameplay");
        _mouseMoveAction = _map.FindAction("PreviewMouseMove");
    }

    public override void OnPointerUp(PointerEventData eventData) {
        _mapRenderer.mouseBuildActionWasPressed = true;
    }

    public override void OnPointerExit(PointerEventData eventData) {
        _pointerIsInside = false;
        _mapRenderer.hoveredTile = null;
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        _pointerIsInside = true;

        var mousePos = _mouseMoveAction.ReadValue<Vector2>();
        var wPos = _camera.ScreenToWorldPoint(mousePos);
        _mapRenderer.hoveredTile = (Vector2Int)_previewTilemap.WorldToCell(wPos);
    }

    public void Update() {
        if (_pointerIsInside) {
            var mousePos = _mouseMoveAction.ReadValue<Vector2>();
            var wPos = _camera.ScreenToWorldPoint(mousePos);
            _mapRenderer.hoveredTile = (Vector2Int)_previewTilemap.WorldToCell(wPos);
        }
    }

    public void LateUpdate() {
        _mapRenderer.mouseBuildActionWasPressed = false;
    }

    void OnEnable() {
        _map.Enable();
    }

    void OnDisable() {
        _map.Disable();
    }

    MapRenderer _mapRenderer;
    bool _pointerIsInside;
}
}

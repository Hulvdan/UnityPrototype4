using System.Collections.Generic;
using System.Reactive.Subjects;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BFG.Runtime {
public class GameManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

    [SerializeField]
    [Required]
    MapRenderer _mapRenderer;

    [SerializeField]
    [Required]
    BuildablesPanel _buildablesPanel;

    [SerializeField]
    [Required]
    UIManager _uiManager;

    [SerializeField]
    [Required]
    CursorController _cursorController;

    [SerializeField]
    [Required]
    InputActionAsset _inputActionAsset;

    [SerializeField]
    float _mapMovementScale = 32f;

    readonly List<float> _zooms = new() { .25f, 0.5f, 1f, 2f, 4f };
    readonly List<float> _gameSpeeds = new() { .1f, .25f, .5f, 1f, 2f, 4f };

    public readonly Subject<SelectedItem> OnSelectedItemChanged = new();
    public readonly Subject<int> OnSelectedItemRotationChanged = new();

    InputAction _actionDecreaseGameSpeed;
    InputAction _actionIncreaseGameSpeed;
    InputAction _actionMoveMap;
    InputAction _actionRotate;
    InputAction _actionStartMapMovement;
    InputAction _actionZoom;
    int _currentZoomIndex;

    InputActionMap _inputActionMap;

    bool _movingMap;

    SelectedItem _selectedItem = SelectedItem.None;

    int _selectedItemRotation;
    int _currentGameSpeedIndex;

    public float currentGameSpeed => _gameSpeeds[_currentGameSpeedIndex];

    public int selectedItemRotation {
        get => _selectedItemRotation;
        private set {
            _selectedItemRotation = value;
            OnSelectedItemRotationChanged.OnNext(value);
        }
    }

    public float dt => Time.deltaTime * currentGameSpeed;

    public SelectedItem selectedItem {
        get => _selectedItem;
        set {
            _selectedItem = value;
            OnSelectedItemChanged.OnNext(value);

            selectedItemRotation = 0;
        }
    }

    float currentZoom => _zooms[_currentZoomIndex];

    void Awake() {
        _inputActionMap = _inputActionAsset.FindActionMap("Gameplay");
        _actionRotate = _inputActionMap.FindAction("Rotate");
        _actionIncreaseGameSpeed = _inputActionMap.FindAction("IncreaseGameSpeed");
        _actionDecreaseGameSpeed = _inputActionMap.FindAction("DecreaseGameSpeed");
        _actionStartMapMovement = _inputActionMap.FindAction("StartMapMovement");
        _actionMoveMap = _inputActionMap.FindAction("MoveMap");
        _actionZoom = _inputActionMap.FindAction("ZoomMap");
    }

    void Start() {
        _map.InitDependencies(this);
        _mapRenderer.InitDependencies(this, _map, _map);
        _buildablesPanel.InitDependencies(this);
        _uiManager.InitDependencies(_map);
        _cursorController.InitDependencies();

        _map.Init();
        _buildablesPanel.Init();

        _currentGameSpeedIndex =
            PlayerPrefs.GetInt("GameManager_CurrentGameSpeedIndex", 3) % _gameSpeeds.Count;
        _currentZoomIndex = PlayerPrefs.GetInt("GameManager_CurrentZoomIndex", 2) % _zooms.Count;
        _map.transform.localScale = new(currentZoom, currentZoom, 1);
    }

    void Update() {
        if (_actionRotate.WasPressedThisFrame()) {
            selectedItemRotation += Mathf.RoundToInt(_actionRotate.ReadValue<float>());
        }

        if (_actionIncreaseGameSpeed.WasPressedThisFrame()) {
            NextGameSpeed();
        }

        if (_actionDecreaseGameSpeed.WasPressedThisFrame()) {
            PreviousGameSpeed();
        }

        if (_actionStartMapMovement.WasPressedThisFrame()) {
            _movingMap = true;
        }
        else if (_actionStartMapMovement.WasReleasedThisFrame()) {
            _movingMap = false;
        }

        if (_movingMap) {
            _map.transform.localPosition +=
                (Vector3)_actionMoveMap.ReadValue<Vector2>() / _mapMovementScale;
        }

        var zoomValue = _actionZoom.ReadValue<float>();
        if (zoomValue > 0) {
            NextZoomLevel();
        }
        else if (zoomValue < 0) {
            PreviousZoomLevel();
        }

        if (zoomValue != 0) {
            _map.transform.localScale = new(currentZoom, currentZoom, 1);
        }
    }

    void PreviousZoomLevel() {
        _currentZoomIndex -= 1;
        if (_currentZoomIndex < 0) {
            _currentZoomIndex = 0;
        }

        PlayerPrefs.SetInt("GameManager_CurrentZoomIndex", _currentGameSpeedIndex);
        PlayerPrefs.Save();
    }

    void NextZoomLevel() {
        _currentZoomIndex += 1;
        if (_currentZoomIndex >= _zooms.Count - 1) {
            _currentZoomIndex = _zooms.Count - 1;
        }

        PlayerPrefs.SetInt("GameManager_CurrentZoomIndex", _currentGameSpeedIndex);
        PlayerPrefs.Save();
    }

    void OnEnable() {
        _inputActionMap.Enable();
    }

    void OnDisable() {
        _inputActionMap.Disable();
    }

    void OnValidate() {
        _mapRenderer.InitDependencies(this, _map, _map);
    }

    void NextGameSpeed() {
        _currentGameSpeedIndex += 1;
        if (_currentGameSpeedIndex >= _gameSpeeds.Count) {
            _currentGameSpeedIndex = _gameSpeeds.Count - 1;
        }

        PlayerPrefs.SetInt("GameManager_CurrentGameSpeedIndex", _currentGameSpeedIndex);
        PlayerPrefs.Save();
    }

    void PreviousGameSpeed() {
        _currentGameSpeedIndex -= 1;
        if (_currentGameSpeedIndex < 0) {
            _currentGameSpeedIndex = 0;
        }

        PlayerPrefs.SetInt("GameManager_CurrentGameSpeedIndex", _currentGameSpeedIndex);
        PlayerPrefs.Save();
    }
}
}

using System.Collections.Generic;
using System.Reactive.Subjects;
using BFG.Runtime.Entities;
using BFG.Runtime.Localization;
using BFG.Runtime.Rendering;
using BFG.Runtime.Rendering.UI;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BFG.Runtime {
[ExecuteAlways]
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
    AudioManager _audioManager;

    [SerializeField]
    [Required]
    CursorController _cursorController;

    [SerializeField]
    [Required]
    InputActionAsset _inputActionAsset;

    [SerializeField]
    [Required]
    MapRendererRaycaster _raycastBridge;

    [SerializeField]
    float _mapMovementScale = 32f;

    readonly List<float> _gameSpeeds = new() { .1f, .25f, .5f, 1f, 2f, 4f };

    readonly List<float> _zooms = new() { .25f, 0.5f, 1f, 2f, 4f };

    public readonly Subject<ItemToBuildType> OnSelectedItemChanged = new();
    public readonly Subject<int> OnSelectedItemRotationChanged = new();
    InputAction _actionChangeLanguage;

    InputAction _actionDecreaseGameSpeed;
    InputAction _actionIncreaseGameSpeed;
    InputAction _actionMoveMap;
    InputAction _actionRotate;
    InputAction _actionStartMapMovement;
    InputAction _actionZoom;
    int _currentGameSpeedIndex;
    int _currentZoomIndex = 2;

    InputActionMap _inputActionMap;

    bool _movingMap;

    int _selectedItemRotation;

    bool _editMode_ReinitDependencies;

    [CanBeNull]
    public ItemToBuild ItemToBuild;

    public float currentGameSpeed => _gameSpeeds[_currentGameSpeedIndex];
    public float currentZoom => _zooms[_currentZoomIndex];

    public int selectedItemRotation {
        get => _selectedItemRotation;
        private set {
            _selectedItemRotation = value;
            OnSelectedItemRotationChanged.OnNext(value);
        }
    }

    public float dt => Time.deltaTime * currentGameSpeed;

    void Awake() {
        InitDependencies();
    }

    void Start() {
        Init();

        _currentGameSpeedIndex =
            PlayerPrefs.GetInt("GameManager_CurrentGameSpeedIndex", 3) % _gameSpeeds.Count;
#if UNITY_EDITOR
        _currentZoomIndex = 2;
#else
        _currentZoomIndex = PlayerPrefs.GetInt("GameManager_CurrentZoomIndex", 2) % _zooms.Count;
#endif
        _map.transform.localScale = new(currentZoom, currentZoom, 1);
    }

    [Button("Manually Init Dependencies")]
    void EditMode_InitDependencies() {
        InitDependencies();
        Init();
    }

    void InitDependencies() {
        _inputActionMap = _inputActionAsset.FindActionMap("Gameplay");
        _actionRotate = _inputActionMap.FindAction("Rotate");
        _actionIncreaseGameSpeed = _inputActionMap.FindAction("IncreaseGameSpeed");
        _actionDecreaseGameSpeed = _inputActionMap.FindAction("DecreaseGameSpeed");
        _actionStartMapMovement = _inputActionMap.FindAction("StartMapMovement");
        _actionMoveMap = _inputActionMap.FindAction("MoveMap");
        _actionZoom = _inputActionMap.FindAction("ZoomMap");
        _actionChangeLanguage = _inputActionMap.FindAction("ChangeLanguage");
    }

    void Init() {
        _audioManager.Init();

        _map.InitDependencies(this);
        _mapRenderer.InitDependencies(this, _map, _map);
        _buildablesPanel.InitDependencies(this);
        _uiManager.InitDependencies(_map);
        _cursorController.InitDependencies();
        _raycastBridge.InitDependencies(_mapRenderer);

        _map.Init();
        _mapRenderer.Init();
        _buildablesPanel.Init();
    }

    void Update() {
        if (Application.isPlaying) {
            UpdateWhenPlaying();
        }
        else if (_editMode_ReinitDependencies) {
            _editMode_ReinitDependencies = false;
            EditMode_InitDependencies();
        }
    }

    void UpdateWhenPlaying() {
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

        if (_actionChangeLanguage.WasReleasedThisFrame()) {
            var newLanguage = (int)LocalizationDatabase.instance.currentLanguage + 1;
            newLanguage %= (int)(Language.Ru + 1);
            LocalizationDatabase.instance.ChangeLanguage((Language)newLanguage);
        }
    }

    void OnEnable() {
        if (Application.isPlaying) {
            _inputActionMap.Enable();
        }
    }

    void OnDisable() {
        if (Application.isPlaying) {
            _inputActionMap.Disable();
        }
    }

    void OnValidate() {
        _editMode_ReinitDependencies = true;
#if UNITY_EDITOR
        EditorApplication.QueuePlayerLoopUpdate();
#endif
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

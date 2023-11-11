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

    readonly List<float> GameSpeeds = new() { .1f, .25f, .5f, 1f, 2f, 4f };

    public readonly Subject<SelectedItem> OnSelectedItemChanged = new();
    public readonly Subject<int> OnSelectedItemRotationChanged = new();
    InputAction _actionDecreaseGameSpeed;
    InputAction _actionIncreaseGameSpeed;
    InputAction _actionRotate;

    InputActionMap _inputActionMap;

    SelectedItem _selectedItem = SelectedItem.None;

    int _selectedItemRotation;
    int CurrentGameSpeedIndex;

    public float CurrentGameSpeed => GameSpeeds[CurrentGameSpeedIndex];

    public int selectedItemRotation {
        get => _selectedItemRotation;
        private set {
            _selectedItemRotation = value;
            OnSelectedItemRotationChanged.OnNext(value);
        }
    }

    public float dt => Time.deltaTime * CurrentGameSpeed;

    public SelectedItem selectedItem {
        get => _selectedItem;
        set {
            _selectedItem = value;
            OnSelectedItemChanged.OnNext(value);

            selectedItemRotation = 0;
        }
    }

    void Awake() {
        _inputActionMap = _inputActionAsset.FindActionMap("Gameplay");
        _actionRotate = _inputActionMap.FindAction("Rotate");
        _actionIncreaseGameSpeed = _inputActionMap.FindAction("IncreaseGameSpeed");
        _actionDecreaseGameSpeed = _inputActionMap.FindAction("DecreaseGameSpeed");
    }

    void Start() {
        _map.InitDependencies(this);
        _mapRenderer.InitDependencies(this, _map, _map);
        _buildablesPanel.InitDependencies(this);
        _uiManager.InitDependencies(_map);
        _cursorController.InitDependencies();

        _map.Init();
        _buildablesPanel.Init();

        CurrentGameSpeedIndex =
            PlayerPrefs.GetInt("GameManager_CurrentGameSpeedIndex", 3) % GameSpeeds.Count;
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
        CurrentGameSpeedIndex += 1;
        if (CurrentGameSpeedIndex >= GameSpeeds.Count) {
            CurrentGameSpeedIndex = GameSpeeds.Count - 1;
        }

        PlayerPrefs.SetInt("GameManager_CurrentGameSpeedIndex", CurrentGameSpeedIndex);
    }

    void PreviousGameSpeed() {
        CurrentGameSpeedIndex -= 1;
        if (CurrentGameSpeedIndex < 0) {
            CurrentGameSpeedIndex = 0;
        }

        PlayerPrefs.SetInt("GameManager_CurrentGameSpeedIndex", CurrentGameSpeedIndex);
    }

    public void RotateSelectedItemCW() {
    }

    public void RotateSelectedItemCCW() {
    }
}
}

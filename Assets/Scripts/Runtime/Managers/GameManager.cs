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
    InputActionAsset _inputActionAsset;

    public readonly Subject<SelectedItem> OnSelectedItemChanged = new();
    public readonly Subject<int> OnSelectedItemRotationChanged = new();
    InputAction _actionRotate;

    InputActionMap _inputActionMap;

    SelectedItem _selectedItem = SelectedItem.None;

    int _selectedItemRotation;

    public int selectedItemRotation {
        get => _selectedItemRotation;
        private set {
            _selectedItemRotation = value;
            OnSelectedItemRotationChanged.OnNext(value);
        }
    }

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
    }

    void Start() {
        _map.InitDependencies(this);
        _mapRenderer.InitDependencies(this, _map, _map);
        _buildablesPanel.InitDependencies(this);
        _uiManager.InitDependencies(_map);

        _map.Init();
        _buildablesPanel.Init();
    }

    void Update() {
        if (_actionRotate.WasPressedThisFrame()) {
            selectedItemRotation += Mathf.RoundToInt(_actionRotate.ReadValue<float>());
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

    public void RotateSelectedItemCW() {
    }

    public void RotateSelectedItemCCW() {
    }
}
}

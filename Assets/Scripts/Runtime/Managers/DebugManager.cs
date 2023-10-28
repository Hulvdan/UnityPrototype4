using UnityEngine;
using UnityEngine.InputSystem;

namespace BFG.Runtime {
public class DebugState {
    public const string Key_IsActive_Debug = "Debug_IsActive_Debug";
    public const string Key_IsActive_MovementSystemPaths = "Debug_IsActive_MovementSystemPaths";
    public const string Key_IsActive_UnwalkableCells = "Debug_IsActive_UnwalkableCells";

    public bool IsActive_Debug;
    public bool IsActive_MovementSystemPaths;
    public bool IsActive_UnwalkableCells;
}

public class DebugManager : MonoBehaviour {
    [SerializeField]
    InputActionAsset _inputActionAsset;

    [SerializeField]
    GameObject _movementSystemPaths;

    [SerializeField]
    GameObject _unwalkableCells;

    InputActionMap _map;

    DebugState _prefs;
    InputAction _toggleBuildableCellsAction;
    InputAction _toggleDebugAction;
    InputAction _toggleMovementSystemPathsAction;

    void Awake() {
        _prefs = LoadPrefs();

        _map = _inputActionAsset.FindActionMap("Debug");
        _toggleDebugAction = _map.FindAction("Toggle");
        _toggleMovementSystemPathsAction = _map.FindAction("ToggleMovementSystem");
        _toggleBuildableCellsAction = _map.FindAction("ToggleBuildableCells");
    }

    void Start() {
        UpdateDebugView();
    }

    void Update() {
        if (_toggleDebugAction.WasReleasedThisFrame()) {
            _prefs.IsActive_Debug = !_prefs.IsActive_Debug;
            DumpPrefs(_prefs);
            UpdateDebugView();
        }

        if (_toggleMovementSystemPathsAction.WasReleasedThisFrame()) {
            _prefs.IsActive_MovementSystemPaths = !_prefs.IsActive_MovementSystemPaths;
            DumpPrefs(_prefs);
            UpdateDebugView();
        }

        if (_toggleBuildableCellsAction.WasReleasedThisFrame()) {
            _prefs.IsActive_UnwalkableCells = !_prefs.IsActive_UnwalkableCells;
            DumpPrefs(_prefs);
            UpdateDebugView();
        }
    }

    void OnEnable() {
        _map.Enable();
    }

    void OnDisable() {
        _map.Disable();
    }

    void UpdateDebugView() {
        if (!_prefs.IsActive_Debug) {
            _movementSystemPaths.SetActive(false);
            _unwalkableCells.SetActive(false);
            return;
        }

        _movementSystemPaths.SetActive(_prefs.IsActive_MovementSystemPaths);
        _unwalkableCells.SetActive(_prefs.IsActive_UnwalkableCells);
    }

    void DumpPrefs(DebugState prefs) {
        PlayerPrefs.SetInt(DebugState.Key_IsActive_Debug, prefs.IsActive_Debug ? 1 : 0);
        PlayerPrefs.SetInt(DebugState.Key_IsActive_MovementSystemPaths,
            prefs.IsActive_MovementSystemPaths ? 1 : 0);
        PlayerPrefs.SetInt(DebugState.Key_IsActive_UnwalkableCells,
            prefs.IsActive_UnwalkableCells ? 1 : 0);
        PlayerPrefs.Save();
    }

    DebugState LoadPrefs() {
        return new() {
            IsActive_Debug = PlayerPrefs.GetInt(DebugState.Key_IsActive_Debug, 0) > 0,
            IsActive_MovementSystemPaths =
                PlayerPrefs.GetInt(DebugState.Key_IsActive_MovementSystemPaths, 0) > 0,
            IsActive_UnwalkableCells =
                PlayerPrefs.GetInt(DebugState.Key_IsActive_UnwalkableCells, 0) > 0
        };
    }
}
}

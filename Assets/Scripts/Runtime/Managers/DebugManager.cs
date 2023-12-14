using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace BFG.Runtime {
public class DebugState {
    public const string KEY_IS_ACTIVE_DEBUG = "Debug_IsActive_Debug";
    public const string KEY_IS_ACTIVE_MOVEMENT_SYSTEM_PATHS = "Debug_IsActive_MovementSystemPaths";
    public const string KEY_IS_ACTIVE_UNWALKABLE_TILES = "Debug_IsActive_UnwalkableTiles";

    public bool isActive_debug;
    public bool isActive_movementSystemPaths;
    public bool isActive_unwalkableTiles;
}

public class DebugManager : MonoBehaviour {
    [SerializeField]
    InputActionAsset _inputActionAsset;

    [SerializeField]
    GameObject _movementSystemPaths;

    [SerializeField]
    GameObject _unwalkableTiles;

    InputActionMap _map;

    DebugState _preferences;
    InputAction _toggleBuildableTilesAction;
    InputAction _toggleDebugAction;
    InputAction _toggleMovementSystemPathsAction;

    void Awake() {
        _preferences = LoadPreferences();

        _map = _inputActionAsset.FindActionMap("Debug");
        _toggleDebugAction = _map.FindAction("Toggle");
        _toggleMovementSystemPathsAction = _map.FindAction("ToggleMovementSystem");
        _toggleBuildableTilesAction = _map.FindAction("ToggleBuildableCells");
    }

    void OnApplicationQuit() {
        Tracing.DisposeWriter();
    }

    void Start() {
        UpdateDebugView();
    }

    void Update() {
        if (_toggleDebugAction.WasReleasedThisFrame()) {
            _preferences.isActive_debug = !_preferences.isActive_debug;
            DumpPreferences(_preferences);
            UpdateDebugView();
        }

        if (_toggleMovementSystemPathsAction.WasReleasedThisFrame()) {
            _preferences.isActive_movementSystemPaths = !_preferences.isActive_movementSystemPaths;
            DumpPreferences(_preferences);
            UpdateDebugView();
        }

        if (_toggleBuildableTilesAction.WasReleasedThisFrame()) {
            _preferences.isActive_unwalkableTiles = !_preferences.isActive_unwalkableTiles;
            DumpPreferences(_preferences);
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
        if (!_preferences.isActive_debug) {
            _movementSystemPaths.SetActive(false);
            _unwalkableTiles.SetActive(false);
            return;
        }

        _movementSystemPaths.SetActive(_preferences.isActive_movementSystemPaths);
        _unwalkableTiles.SetActive(_preferences.isActive_unwalkableTiles);
    }

    void DumpPreferences(DebugState preferences) {
        PlayerPrefs.SetInt(DebugState.KEY_IS_ACTIVE_DEBUG, preferences.isActive_debug ? 1 : 0);
        PlayerPrefs.SetInt(DebugState.KEY_IS_ACTIVE_MOVEMENT_SYSTEM_PATHS,
            preferences.isActive_movementSystemPaths ? 1 : 0);
        PlayerPrefs.SetInt(DebugState.KEY_IS_ACTIVE_UNWALKABLE_TILES,
            preferences.isActive_unwalkableTiles ? 1 : 0);
        PlayerPrefs.Save();
    }

    DebugState LoadPreferences() {
        return new() {
            isActive_debug = PlayerPrefs.GetInt(DebugState.KEY_IS_ACTIVE_DEBUG, 0) > 0,
            isActive_movementSystemPaths =
                PlayerPrefs.GetInt(DebugState.KEY_IS_ACTIVE_MOVEMENT_SYSTEM_PATHS, 0) > 0,
            isActive_unwalkableTiles =
                PlayerPrefs.GetInt(DebugState.KEY_IS_ACTIVE_UNWALKABLE_TILES, 0) > 0,
        };
    }
}
}

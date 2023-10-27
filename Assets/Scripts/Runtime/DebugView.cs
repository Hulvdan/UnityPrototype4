using UnityEngine;
using UnityEngine.InputSystem;

namespace BFG.Runtime {
public class DebugState {
    public const string KeyDebugActive = "Debug_DebugActive";
    public const string KeyMovementSystemPathsActive = "Debug_MovementSystemPathsActive";

    public bool DebugActive;
    public bool MovementSystemPathsActive;
}

public class DebugView : MonoBehaviour {
    [SerializeField]
    InputActionAsset _inputActionAsset;

    [SerializeField]
    GameObject _movementSystemPaths;

    InputActionMap _map;

    DebugState _prefs;
    InputAction _toggleDebugAction;
    InputAction _toggleMovementSystemPaths;

    void Awake() {
        _prefs = LoadPrefs();

        _map = _inputActionAsset.FindActionMap("Debug");
        _toggleDebugAction = _map.FindAction("Toggle");
        _toggleMovementSystemPaths = _map.FindAction("ToggleMovementSystem");
    }

    void Start() {
        UpdateDebugView();
    }

    void Update() {
        if (_toggleDebugAction.WasReleasedThisFrame()) {
            _prefs.DebugActive = !_prefs.DebugActive;
            DumpPrefs(_prefs);
            UpdateDebugView();
        }

        if (_toggleMovementSystemPaths.WasReleasedThisFrame()) {
            _prefs.MovementSystemPathsActive = !_prefs.MovementSystemPathsActive;
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
        if (!_prefs.DebugActive) {
            _movementSystemPaths.SetActive(false);
            return;
        }

        _movementSystemPaths.SetActive(_prefs.MovementSystemPathsActive);
    }

    void DumpPrefs(DebugState prefs) {
        PlayerPrefs.SetInt(DebugState.KeyDebugActive, prefs.DebugActive ? 1 : 0);
        PlayerPrefs.SetInt(DebugState.KeyMovementSystemPathsActive,
            prefs.MovementSystemPathsActive ? 1 : 0);
        PlayerPrefs.Save();
    }

    DebugState LoadPrefs() {
        return new DebugState {
            DebugActive = PlayerPrefs.GetInt(DebugState.KeyDebugActive, 0) > 0,
            MovementSystemPathsActive =
                PlayerPrefs.GetInt(DebugState.KeyMovementSystemPathsActive, 0) > 0
        };
    }
}
}

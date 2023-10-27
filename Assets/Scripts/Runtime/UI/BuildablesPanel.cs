using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class BuildablesPanel : MonoBehaviour {
    [SerializeField]
    List<BuildableButton> _buttons;

    void Start() {
        foreach (var button in _buttons) {
            button.Init(this);
        }
    }

    public void OnButtonSelected(int buttonInstanceID) {
        foreach (var button in _buttons) {
            if (button.GetInstanceID() != buttonInstanceID) {
                button.SetSelected(false);
            }
        }
    }
}
}

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class BuildablesPanel : MonoBehaviour {
    [SerializeField]
    List<BuildableButton> _buttons;

    [SerializeField]
    [Required]
    Map _map;

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
            else {
                _map.SetSelectedItem(button.item);
            }
        }
    }
}
}

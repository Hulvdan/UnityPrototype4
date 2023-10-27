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

    bool _ignoreButtonStateChanges;

    void Start() {
        foreach (var button in _buttons) {
            button.Init(this);
        }
    }

    public void OnButtonChangedSelectedState(int? selectedButtonInstanceID) {
        if (_ignoreButtonStateChanges) {
            return;
        }

        _ignoreButtonStateChanges = true;
        foreach (var button in _buttons) {
            if (button.GetInstanceID() == selectedButtonInstanceID) {
                _map.SetSelectedItem(button.item);
                continue;
            }

            button.SetSelected(false);
        }

        _ignoreButtonStateChanges = false;

        if (selectedButtonInstanceID == null) {
            _map.SetSelectedItem(SelectedItem.None);
        }
    }
}
}

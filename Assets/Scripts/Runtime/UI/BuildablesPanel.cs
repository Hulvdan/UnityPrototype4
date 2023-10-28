using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class BuildablesPanel : MonoBehaviour {
    [SerializeField]
    List<BuildableButton> _buttons;

    GameManager _gameManager;
    bool _ignoreButtonStateChanges;

    public void InitDependencies(GameManager gameManager) {
        _gameManager = gameManager;
    }

    public void Init() {
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
                _gameManager.SetSelectedItem(button.item);
                continue;
            }

            button.SetSelected(false);
        }

        _ignoreButtonStateChanges = false;

        if (selectedButtonInstanceID == null) {
            _gameManager.SetSelectedItem(SelectedItem.None);
        }
    }
}
}

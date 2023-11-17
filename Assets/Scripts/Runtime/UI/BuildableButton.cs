using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BFG.Runtime {
[Serializable]
public struct ColorBlock {
    [SerializeField]
    Color _normalColor;

    [SerializeField]
    Color _highlightedColor;

    [SerializeField]
    Color _selectedColor;

    [SerializeField]
    Color _disabledColor;

    public Color normalColor {
        get => _normalColor;
        set => _normalColor = value;
    }

    public Color highlightedColor {
        get => _highlightedColor;
        set => _highlightedColor = value;
    }

    public Color selectedColor {
        get => _selectedColor;
        set => _selectedColor = value;
    }

    public Color disabledColor {
        get => _disabledColor;
        set => _disabledColor = value;
    }
}

public enum ButtonEnum {
    Disabled,
    Selected,
    Hovered,
    Normal,
}

public class BuildableButton : MonoBehaviour {
    [FormerlySerializedAs("_item")]
    [SerializeField]
    [Required]
    public SelectedItemType ItemType;

    [SerializeField]
    [ShowIf("ItemType", SelectedItemType.Building)]
    [Required]
    [CanBeNull]
    ScriptableBuilding _building;

    [Space]
    [SerializeField]
    List<Image> _images;

    [SerializeField]
    [Min(0)]
    float _fadeDuration;

    [SerializeField]
    ColorBlock _colorBlock;

    float _fadeElapsed;

    bool _fading;
    Color _fromColor;

    BuildablesPanel _panel;

    ButtonEnum _state = ButtonEnum.Normal;
    bool _stateHovered;
    bool _stateInteractable = true;
    bool _stateSelected;

    Color _targetColor;

    public SelectedItem selectedItem {
        get {
            var item = new SelectedItem { Type = ItemType };

            if (ItemType == SelectedItemType.Building) {
                Assert.IsNotNull(_building);
                item.Building = _building;
            }

            return item;
        }
    }

    void Update() {
        if (!_fading) {
            return;
        }

        _fadeElapsed += Time.deltaTime;

        var t = _fadeElapsed / _fadeDuration;
        if (t >= 1) {
            _fading = false;
            t = 1;
        }

        foreach (var image in _images) {
            image.color = Color.Lerp(_fromColor, _targetColor, t);
        }
    }

    public void Init(BuildablesPanel panel) {
        _panel = panel;
    }

    void StartFading(Color target, bool instant = false) {
        if (instant) {
            foreach (var image in _images) {
                image.color = target;
            }

            return;
        }

        _fading = true;
        _fadeElapsed = 0;
        _targetColor = target;
        _fromColor = _images[0].color;
    }

    void RecalculateState(ButtonEnum state) {
        if (_state == state) {
            return;
        }

        int? selectedButtonInstanceID = null;
        if (state == ButtonEnum.Selected) {
            selectedButtonInstanceID = GetInstanceID();
        }

        if (_state == ButtonEnum.Selected || state == ButtonEnum.Selected) {
            _panel.OnButtonChangedSelectedState(selectedButtonInstanceID);
        }

        _state = state;
        StartFading(GetColor(state));
    }

    Color GetColor(ButtonEnum state) {
        return state switch {
            ButtonEnum.Disabled => _colorBlock.disabledColor,
            ButtonEnum.Selected => _colorBlock.selectedColor,
            ButtonEnum.Hovered => _colorBlock.highlightedColor,
            ButtonEnum.Normal => _colorBlock.normalColor,
            _ => _colorBlock.normalColor,
        };
    }

    public void PointerClick() {
        if (!_stateInteractable) {
            return;
        }

        _stateSelected = !_stateSelected;
        UpdateState();
    }

    public void PointerEnter() {
        _stateHovered = true;
        UpdateState();
    }

    public void PointerExit() {
        _stateHovered = false;
        UpdateState();
    }

    void UpdateState() {
        if (!_stateInteractable) {
            RecalculateState(ButtonEnum.Disabled);
            return;
        }

        if (_stateSelected) {
            RecalculateState(ButtonEnum.Selected);
            return;
        }

        if (_stateHovered) {
            RecalculateState(ButtonEnum.Hovered);
            return;
        }

        RecalculateState(ButtonEnum.Normal);
    }

    public void SetSelected(bool selected) {
        if (!_stateInteractable) {
            return;
        }

        _stateSelected = selected;
        UpdateState();
    }

    public void SetInteractable(bool interactable) {
        _stateInteractable = interactable;
        UpdateState();
    }
}
}

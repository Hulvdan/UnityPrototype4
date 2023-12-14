using System;
using System.Collections.Generic;
using BFG.Runtime.Entities;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BFG.Runtime.Rendering.UI {
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

public enum ButtonState {
    Disabled,
    Selected,
    Hovered,
    Normal,
}

public class BuildableButton : MonoBehaviour {
    [FormerlySerializedAs("ItemType")]
    [FormerlySerializedAs("_item")]
    [SerializeField]
    [Required]
    ItemToBuildType _itemType;

    [SerializeField]
    [ShowIf("ItemType", ItemToBuildType.Building)]
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

    ButtonState _state = ButtonState.Normal;
    bool _stateHovered;
    bool _stateInteractable = true;
    bool _stateSelected;

    Color _targetColor;

    public ItemToBuild itemToBuild {
        get {
            var item = new ItemToBuild { type = _itemType };

            if (_itemType == ItemToBuildType.Building) {
                Assert.IsNotNull(_building);
                item.building = _building;
            }

            return item;
        }
    }

    public void Init(BuildablesPanel panel) {
        _panel = panel;
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

    void UpdateState() {
        if (!_stateInteractable) {
            RecalculateState(ButtonState.Disabled);
            return;
        }

        if (_stateSelected) {
            RecalculateState(ButtonState.Selected);
            return;
        }

        if (_stateHovered) {
            RecalculateState(ButtonState.Hovered);
            return;
        }

        RecalculateState(ButtonState.Normal);
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

    void RecalculateState(ButtonState state) {
        if (_state == state) {
            return;
        }

        int? selectedButtonInstanceID = null;
        if (state == ButtonState.Selected) {
            selectedButtonInstanceID = GetInstanceID();
        }

        if (_state == ButtonState.Selected || state == ButtonState.Selected) {
            _panel.OnButtonChangedSelectedState(selectedButtonInstanceID);
        }

        _state = state;
        StartFading(GetColor(state));
    }

    Color GetColor(ButtonState state) {
        return state switch {
            ButtonState.Disabled => _colorBlock.disabledColor,
            ButtonState.Selected => _colorBlock.selectedColor,
            ButtonState.Hovered => _colorBlock.highlightedColor,
            ButtonState.Normal => _colorBlock.normalColor,
            _ => _colorBlock.normalColor,
        };
    }
}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace BFG.Runtime {
[Serializable]
public struct ColorBlock : IEquatable<ColorBlock> {
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

    public static ColorBlock defaultColorBlock;

    static ColorBlock() {
        defaultColorBlock = new ColorBlock {
            _normalColor = new Color32(255, 255, 255, 255),
            _highlightedColor = new Color32(245, 245, 245, 255),
            _selectedColor = new Color32(245, 245, 245, 255),
            _disabledColor = new Color32(200, 200, 200, 128)
        };
    }

    public override bool Equals(object obj) {
        if (!(obj is ColorBlock)) {
            return false;
        }

        return Equals((ColorBlock)obj);
    }

    public bool Equals(ColorBlock other) {
        return normalColor == other.normalColor &&
               highlightedColor == other.highlightedColor &&
               selectedColor == other.selectedColor &&
               selectedColor == other.selectedColor &&
               disabledColor == other.disabledColor;
    }

    public static bool operator ==(ColorBlock point1, ColorBlock point2) {
        return point1.Equals(point2);
    }

    public static bool operator !=(ColorBlock point1, ColorBlock point2) {
        return !point1.Equals(point2);
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }
}

public enum ButtonEnum {
    Disabled,
    Selected,
    Hovered,
    Normal
}

public class BuildableButton : MonoBehaviour {
    [SerializeField]
    List<Image> _images;

    [SerializeField]
    [Min(0)]
    float _fadeDuration;

    [SerializeField]
    ColorBlock _colorBlock;

    Color _targetColor;
    Color _fromColor;

    bool _selected;
    bool _interactable = true;

    ButtonEnum _state = ButtonEnum.Normal;

    bool _fading;

    float _fadeElapsed;
    bool _hovered;
    BuildablesPanel _panel;

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

    void RecalculateState(ButtonEnum state) {
        if (_state != state) {
            _state = state;

            if (_state == ButtonEnum.Selected) {
                _panel.OnButtonSelected(GetInstanceID());
            }

            StartFading(GetColor(state));
        }
    }

    Color GetColor(ButtonEnum state) {
        return state switch {
            ButtonEnum.Disabled => _colorBlock.disabledColor,
            ButtonEnum.Selected => _colorBlock.selectedColor,
            ButtonEnum.Hovered => _colorBlock.highlightedColor,
            ButtonEnum.Normal => _colorBlock.normalColor
        };
    }

    public void PointerClick() {
        if (!_interactable) {
            return;
        }

        _selected = true;
        UpdateState();
    }

    public void PointerEnter() {
        _hovered = true;
        UpdateState();
    }

    public void PointerExit() {
        _hovered = false;
        UpdateState();
    }

    void UpdateState() {
        if (!_interactable) {
            RecalculateState(ButtonEnum.Disabled);
            return;
        }

        if (_selected) {
            RecalculateState(ButtonEnum.Selected);
            return;
        }

        if (_hovered) {
            RecalculateState(ButtonEnum.Hovered);
            return;
        }

        RecalculateState(ButtonEnum.Normal);
    }

    public void SetSelected(bool selected) {
        if (!_interactable) {
            return;
        }

        _selected = selected;
        UpdateState();
    }

    public void SetInteractable(bool interactable) {
        _interactable = interactable;
        UpdateState();
    }
}
}

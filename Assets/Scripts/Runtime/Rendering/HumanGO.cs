using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
// ReSharper disable once InconsistentNaming
public class HumanGO : MonoBehaviour {
    [SerializeField]
    SpriteRenderer _resourceSpriteRenderer;

    [SerializeField]
    Vector2 _verticalOffset;

    [FormerlySerializedAs("_droppingVerticalOffset")]
    [SerializeField]
    Vector2 _placingVerticalOffset;

    [SerializeField]
    [Min(0)]
    float _itemPickupDuration = 1;

    [FormerlySerializedAs("_itemDropDuration")]
    [SerializeField]
    [Min(0)]
    float _itemPlacingDuration = 1;

    [SerializeField]
    AnimationCurve _itemPickupCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [FormerlySerializedAs("_itemDropCurve")]
    [SerializeField]
    AnimationCurve _itemPlacingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public void OnPickedUpResource(ScriptableResource resource, float gameSpeed) {
        _resourceSpriteRenderer.sprite = resource.smallerSprite;
        _resourceSpriteRenderer.transform.localPosition = _placingVerticalOffset;
        DOTween.To(
            () => _resourceSpriteRenderer.transform.localPosition,
            (Vector2 value) => _resourceSpriteRenderer.transform.localPosition = value,
            _verticalOffset,
            _itemPickupDuration / gameSpeed
        ).SetEase(_itemPickupCurve);
    }

    public void OnStartedPickingUpResource(ScriptableResource resource) {
        _resourceSpriteRenderer.sprite = resource.smallerSprite;
    }

    public void SetPickingUpResourceCoef(float normalized) {
        var coef = _itemPickupCurve.Evaluate(normalized);

        var a = transform.TransformPoint(Vector3.zero);
        var b = transform.TransformPoint(_verticalOffset);

        _resourceSpriteRenderer.transform.position = Vector2.Lerp(a, b, coef);
    }

    public void OnStoppedPickingUpResource(ScriptableResource resource) {
        var b = transform.TransformPoint(_verticalOffset);
        _resourceSpriteRenderer.transform.position = b;
    }

    public void OnPlacedResource(float gameSpeed) {
        _resourceSpriteRenderer.transform.localPosition = _verticalOffset;
        DOTween.To(
            () => _resourceSpriteRenderer.transform.localPosition,
            (Vector2 value) => _resourceSpriteRenderer.transform.localPosition = value,
            _placingVerticalOffset,
            _itemPlacingDuration / gameSpeed
        ).SetEase(_itemPlacingCurve).OnComplete(
            () => _resourceSpriteRenderer.sprite = null
        );
    }

    public void OnStartedPlacingResource(ScriptableResource resource) {
    }

    public void SetPlacingResourceCoef(float normalized) {
        var coef = _itemPlacingCurve.Evaluate(normalized);

        var a = transform.TransformPoint(_verticalOffset);
        var b = transform.TransformPoint(Vector3.zero);

        _resourceSpriteRenderer.transform.position = Vector2.Lerp(a, b, coef);
    }

    public void OnStoppedPlacingResource(ScriptableResource resource) {
        _resourceSpriteRenderer.sprite = null;
    }
}
}

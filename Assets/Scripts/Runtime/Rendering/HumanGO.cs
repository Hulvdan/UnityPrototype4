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
}
}

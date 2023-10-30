using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class TrainNodeGO : MonoBehaviour {
    [SerializeField]
    [Required]
    SpriteRenderer _resourceSpriteRenderer;

    [SerializeField]
    [Required]
    Transform _itemOffset;

    [SerializeField]
    [Required]
    [Min(0)]
    float _itemPickupDuration = 1f;

    [SerializeField]
    [Required]
    AnimationCurve _itemPickupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void OnPickedUpResource(ScriptableResource resource, Vector2 resourceMapPosition) {
        _resourceSpriteRenderer.sprite = resource.smallerSprite;
        _itemOffset.transform.localPosition = resourceMapPosition;
        _itemOffset.transform.localPosition -= transform.localPosition;
        DOTween.To(
            () => _itemOffset.transform.localPosition,
            (Vector2 value) => _itemOffset.transform.localPosition = value,
            Vector2.zero,
            _itemPickupDuration
        ).SetEase(_itemPickupCurve);
    }

    public void OnPushedResource() {
        _resourceSpriteRenderer.sprite = null;
    }
}
}

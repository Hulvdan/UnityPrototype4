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
    SpriteRenderer _mainSpriteRenderer;

    [SerializeField]
    [Required]
    Transform _itemOffset;

    [SerializeField]
    Animator _locomotiveAnimator;

    [SerializeField]
    [Required]
    [Min(0)]
    float _itemPickupDuration = 1f;

    [SerializeField]
    [Required]
    AnimationCurve _itemPickupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public Animator LocomotiveAnimator => _locomotiveAnimator;
    public SpriteRenderer MainSpriteRenderer => _mainSpriteRenderer;

    public void OnPickedUpResource(
        ScriptableResource resource,
        Vector2 resourceMapPosition,
        float gameSpeed
    ) {
        _resourceSpriteRenderer.sprite = resource.smallerSprite;
        _itemOffset.localPosition = resourceMapPosition;
        _itemOffset.localPosition -= transform.localPosition;
        DOTween.To(
                () => _itemOffset.localPosition,
                (Vector2 value) => _itemOffset.localPosition = value,
                Vector2.zero,
                _itemPickupDuration / gameSpeed
            )
            .SetEase(_itemPickupCurve);
    }

    public void OnPushedResource() {
        _resourceSpriteRenderer.sprite = null;
    }
}
}

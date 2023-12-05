using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering {
// ReSharper disable once InconsistentNaming
public class HumanGO : MonoBehaviour {
    [SerializeField]
    SpriteRenderer _resourceSpriteRenderer;

    [SerializeField]
    Vector2 _verticalOffset;

    [SerializeField]
    AnimationCurve _itemPickupCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [FormerlySerializedAs("_itemDropCurve")]
    [SerializeField]
    AnimationCurve _itemPlacingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public void OnStartedPickingUpResource(ScriptableResource resource) {
        _resourceSpriteRenderer.sprite = resource.smallerSprite;
    }

    public void SetPickingUpResourceProgress(float progress) {
        var a = transform.TransformPoint(Vector3.zero);
        var b = transform.TransformPoint(_verticalOffset);
        var t = _itemPickupCurve.Evaluate(progress);

        _resourceSpriteRenderer.transform.position = Vector2.Lerp(a, b, t);
    }

    public void OnStoppedPickingUpResource() {
        var b = transform.TransformPoint(_verticalOffset);
        _resourceSpriteRenderer.transform.position = b;
    }

    public void OnStartedPlacingResource(ScriptableResource resource) {
        // Hulvdan: Intentionally left blank
    }

    public void SetPlacingResourceProgress(float progress) {
        var a = transform.TransformPoint(_verticalOffset);
        var b = transform.TransformPoint(Vector3.zero);
        var t = _itemPlacingCurve.Evaluate(progress);

        _resourceSpriteRenderer.transform.position = Vector2.Lerp(a, b, t);
    }

    public void OnStoppedPlacingResource() {
        _resourceSpriteRenderer.sprite = null;
    }
}
}

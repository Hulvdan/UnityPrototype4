using UnityEngine;

namespace BFG.Prototyping {
public sealed class ScaleMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeX = new(0.9f, 1.1f);

    [SerializeField]
    Vector2 _amplitudeY = new(0.9f, 1.1f);

    public override void UpdateData(float dt, float normalized, float coef, HumanBinding binding) {
        var scaleX = Mathf.Lerp(_amplitudeX.x, _amplitudeX.y, coef);
        var scaleY = Mathf.Lerp(_amplitudeY.x, _amplitudeY.y, coef);

        var tr = binding.Human.transform;
        var trScale = tr.localScale;
        trScale.x = scaleX;
        trScale.y = scaleY;
        tr.localScale = trScale;
    }
}
}

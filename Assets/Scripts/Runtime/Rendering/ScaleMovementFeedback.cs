using UnityEngine;

namespace BFG.Runtime.Rendering {
public sealed class ScaleMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeX = new(0.9f, 1.1f);

    [SerializeField]
    Vector2 _amplitudeY = new(0.9f, 1.1f);

    public override void UpdateData(
        float dt,
        float progress,
        float t,
        Vector2 from,
        Vector2Int to,
        GameObject human
    ) {
        var scaleX = Mathf.Lerp(_amplitudeX.x, _amplitudeX.y, t);
        var scaleY = Mathf.Lerp(_amplitudeY.x, _amplitudeY.y, t);

        var tr = human.transform;
        var trScale = tr.localScale;
        trScale.x = scaleX;
        trScale.y = scaleY;
        tr.localScale = trScale;
    }
}
}

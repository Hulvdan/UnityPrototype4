using UnityEngine;

namespace BFG.Runtime {
public sealed class PositionOffsetMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeX = new(-.1f, .1f);

    [SerializeField]
    Vector2 _amplitudeY = new(-.1f, .1f);

    public override void UpdateData(
        float dt,
        float normalized,
        float coef,
        Vector2 from,
        Vector2Int to,
        GameObject human
    ) {
        human.transform.localPosition += new Vector3(
            Mathf.Lerp(_amplitudeX.x, _amplitudeX.y, coef),
            Mathf.Lerp(_amplitudeY.x, _amplitudeY.y, coef),
            0
        );
    }
}
}

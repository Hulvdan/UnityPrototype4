using UnityEngine;

namespace BFG.Runtime {
public sealed class PositionMovementFeedback : MovementFeedback {
    public override void UpdateData(
        float dt,
        float progress,
        float t,
        Vector2 from,
        Vector2Int to,
        GameObject human
    ) {
        human.transform.localPosition = Vector2.Lerp(from, to, t);
    }
}
}

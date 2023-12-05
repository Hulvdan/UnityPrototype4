using UnityEngine;

namespace BFG.Runtime {
public sealed class RotationMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeZ = new Vector2(-1, 1) * 20;

    public override void UpdateData(
        float dt,
        float progress,
        float curveEvaluatedProgress,
        Vector2 from,
        Vector2Int to,
        GameObject human
    ) {
        var rot = Mathf.Lerp(_amplitudeZ.x, _amplitudeZ.y, curveEvaluatedProgress);

        var tr = human.transform;
        var trRotation = tr.localRotation;
        var euler = trRotation.eulerAngles;
        euler.z = rot;
        trRotation.eulerAngles = euler;
        tr.localRotation = trRotation;
    }
}
}

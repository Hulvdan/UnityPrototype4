using UnityEngine;

namespace BFG.Runtime.Rendering {
public sealed class RotationMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeZ = new Vector2(-1, 1) * 20;

    public override void UpdateData(
        float dt,
        float progress,
        float t,
        Vector2 from,
        Vector2Int to,
        GameObject human
    ) {
        var rot = Mathf.Lerp(_amplitudeZ.x, _amplitudeZ.y, t);

        var tr = human.transform;
        var trRotation = tr.localRotation;
        var euler = trRotation.eulerAngles;
        euler.z = rot;
        trRotation.eulerAngles = euler;
        tr.localRotation = trRotation;
    }
}
}

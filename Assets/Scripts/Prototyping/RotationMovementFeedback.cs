using UnityEngine;

namespace BFG.Prototyping {
public sealed class RotationMovementFeedback : MovementFeedback {
    [SerializeField]
    Vector2 _amplitudeZ = new Vector2(-1, 1) * 20;

    public override void UpdateData(float dt, float normalized, float coef, HumanBinding binding) {
        var rot = Mathf.Lerp(_amplitudeZ.x, _amplitudeZ.y, coef);

        var tr = binding.Human.transform;
        var trRotation = tr.localRotation;
        var euler = trRotation.eulerAngles;
        euler.z = rot;
        trRotation.eulerAngles = euler;
        tr.localRotation = trRotation;
    }
}
}

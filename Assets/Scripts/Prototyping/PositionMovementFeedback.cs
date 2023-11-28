using UnityEngine;

namespace BFG.Prototyping {
public sealed class PositionMovementFeedback : MovementFeedback {
    public override void UpdateData(float dt, float normalized, float coef, HumanBinding binding) {
        var from = binding.GetMovingFrom();
        var to = binding.GetMovingTo();

        binding.Human.transform.position =
            Vector2.Lerp(from, to, coef)
            + Vector2.one / 2;
    }
}
}

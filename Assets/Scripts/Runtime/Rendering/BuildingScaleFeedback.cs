using UnityEngine;

namespace BFG.Runtime {
internal class BuildingScaleFeedback : BuildingFeedback {
    [SerializeField]
    Vector2 _scaleAmplitude = Vector2.one;

    [SerializeField]
    AnimationCurve _curve;

    [SerializeField]
    [Min(.01f)]
    float _duration = 1f;

    public override void UpdateData(Building building, ref BuildingData data) {
        float elapsed;
        if (building.scriptable.type == BuildingType.SpecialCityHall) {
            elapsed = building.timeSinceHumanWasCreated;
        }
        else {
            elapsed = building.timeSinceItemWasPlaced;
        }

        var coef = 1f;
        if (elapsed < _duration) {
            coef = elapsed / _duration;
        }

        var evaluated = _curve.Evaluate(coef);
        var scale = Vector2.one + Vector2.Lerp(-_scaleAmplitude, _scaleAmplitude, evaluated);
        data.Scale = scale;
    }
}
}

using BFG.Runtime.Entities;
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

        var progress = 1f;
        if (elapsed < _duration) {
            progress = elapsed / _duration;
        }

        var t = _curve.Evaluate(progress);
        var scale = Vector2.one + Vector2.Lerp(-_scaleAmplitude, _scaleAmplitude, t);
        data.Scale = scale;
    }
}
}

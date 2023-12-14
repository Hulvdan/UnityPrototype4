using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BFG.Runtime.Rendering {
public abstract class MovementFeedback : MonoBehaviour {
    [SerializeField]
    List<AnimationCurve> _randomCurves = new();

    public AnimationCurve GetRandomCurve() {
        if (_randomCurves.Count == 0) {
            return _LINEAR_CURVE;
        }

        var index = Math.Min((int)(Random.value * _randomCurves.Count), _randomCurves.Count - 1);
        return _randomCurves[index];
    }

    public abstract void UpdateData(
        float dt,
        float progress,
        float t,
        Vector2 from,
        Vector2Int to,
        GameObject human
    );

    static readonly AnimationCurve _LINEAR_CURVE = AnimationCurve.Linear(0, 0, 1, 1);
}
}

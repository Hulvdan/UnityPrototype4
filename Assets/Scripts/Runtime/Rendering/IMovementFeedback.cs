using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BFG.Runtime.Rendering {
public abstract class MovementFeedback : MonoBehaviour {
    [FormerlySerializedAs("RandomCurves")]
    [SerializeField]
    List<AnimationCurve> _randomCurves = new();

    public AnimationCurve GetRandomCurve() {
        if (_randomCurves.Count == 0) {
            return LinearCurve;
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

    static readonly AnimationCurve LinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
}
}

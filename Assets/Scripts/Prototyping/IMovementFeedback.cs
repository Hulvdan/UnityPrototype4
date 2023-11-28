using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BFG.Prototyping {
public abstract class MovementFeedback : MonoBehaviour {
    static readonly AnimationCurve LinearCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public List<AnimationCurve> RandomCurves = new();

    public AnimationCurve GetRandomCurve() {
        if (RandomCurves.Count == 0) {
            return LinearCurve;
        }

        var index = Math.Min((int)(Random.value * RandomCurves.Count), RandomCurves.Count - 1);
        return RandomCurves[index];
    }

    public abstract void UpdateData(float dt, float normalized, float coef, HumanBinding binding);
}
}

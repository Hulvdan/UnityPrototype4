using System;
using System.Collections.Generic;
using BFG.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Prototyping {
[Serializable]
public class MovementPattern {
    public List<MovementFeedback> Feedbacks = new();
}

[Serializable]
public class HumanBinding {
    public GameObject Human;
    public MovementPattern Pattern;

    public List<Vector2Int> Path = new();

    [NonSerialized]
    public List<AnimationCurve> CurvePerFeedback = new();

    [NonSerialized]
    public List<Vector2Int> MovementPathSplitIntoCells = new();

    [NonSerialized]
    public int CurrentIndex;

    public void Init() {
        foreach (var feedback in Pattern.Feedbacks) {
            CurvePerFeedback.Add(feedback.GetRandomCurve());
        }

        var path = Path;
        for (var i = 0; i < path.Count - 1; i++) {
            var a = path[i];
            var b = path[i + 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                MovementPathSplitIntoCells.Add(tile);
            }
        }

        for (var i = path.Count - 1; i > 0; i--) {
            var a = path[i];
            var b = path[i - 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                MovementPathSplitIntoCells.Add(tile);
            }
        }
    }

    public Vector2Int UpdateNextTileInPath() {
        CurrentIndex++;
        while (CurrentIndex >= MovementPathSplitIntoCells.Count) {
            CurrentIndex -= MovementPathSplitIntoCells.Count;
        }

        return Vector2Int.zero;
    }

    public Vector2Int GetMovingFrom() {
        return MovementPathSplitIntoCells[CurrentIndex];
    }

    public Vector2Int GetMovingTo() {
        return MovementPathSplitIntoCells[(CurrentIndex + 1) % MovementPathSplitIntoCells.Count];
    }

    List<Vector2Int> GetFromToTiles(Vector2Int from, Vector2Int to) {
        Assert.IsTrue(from.x == to.x || from.y == to.y);
        Assert.IsFalse(from.x == to.x && from.y == to.y);

        var offset = Utils.Direction(from, to).AsOffset();
        var length = Math.Max(
            Math.Abs(from.x - to.x),
            Math.Abs(from.y - to.y)
        );

        var current = from;
        var res = new List<Vector2Int> { current };

        for (var i = 0; i < length - 1; i++) {
            current += offset;
            res.Add(current);
        }

        return res;
    }
}

public class HumansMovement : MonoBehaviour {
    public float OneTileMovementDuration = 1f;

    public List<HumanBinding> Bindings = new();

    [NonSerialized]
    float _movementElapsed;

    public void Start() {
        foreach (var binding in Bindings) {
            binding.Init();
        }
    }

    public void Update() {
        _movementElapsed += Time.deltaTime;
        while (_movementElapsed > OneTileMovementDuration) {
            _movementElapsed -= OneTileMovementDuration;

            foreach (var binding in Bindings) {
                binding.UpdateNextTileInPath();

                for (var i = 0; i < binding.CurvePerFeedback.Count; i++) {
                    binding.CurvePerFeedback[i] = binding.Pattern.Feedbacks[i].GetRandomCurve();
                }
            }
        }

        var normalized = _movementElapsed / OneTileMovementDuration;
        Assert.IsTrue(normalized <= 1);
        Assert.IsTrue(_movementElapsed <= OneTileMovementDuration);

        foreach (var binding in Bindings) {
            for (var i = 0; i < binding.Pattern.Feedbacks.Count; i++) {
                var curve = binding.CurvePerFeedback[i];
                var coef = curve.Evaluate(normalized);
                var feedback = binding.Pattern.Feedbacks[i];
                feedback.UpdateData(Time.deltaTime, normalized, coef, binding);
            }
        }
    }
}
}

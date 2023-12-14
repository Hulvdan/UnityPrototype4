using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Runtime.Rendering;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace BFG.Prototyping {
public class HumansMovement : MonoBehaviour {
    public float oneTileMovementDuration = 1f;

    public List<HumanBinding> bindings = new();

    public int rows = 9;

    public int columns = 4;

    public int columnsGap = 1;

    public int rowsGap = 1;

    public int startingRow;

    public int startingColumn;

    public int travelDistanceX = 2;

    public int travelDistanceY;

    public Transform humansContainer;
    public GameObject humanPrefab;
    public List<MovementFeedback> defaultMovementFeedbacks;

    [NonSerialized]
    float _movementElapsed;

    public void Start() {
        foreach (var binding in bindings) {
            binding.Init();
        }
    }

    public void Update() {
        _movementElapsed += Time.deltaTime;
        while (_movementElapsed > oneTileMovementDuration) {
            _movementElapsed -= oneTileMovementDuration;

            foreach (var binding in bindings) {
                binding.UpdateNextTileInPath();

                for (var i = 0; i < binding.curvePerFeedback.Count; i++) {
                    binding.curvePerFeedback[i] = binding.pattern.feedbacks[i].GetRandomCurve();
                }
            }
        }

        var progress = _movementElapsed / oneTileMovementDuration;
        Assert.IsTrue(progress <= 1);
        Assert.IsTrue(_movementElapsed <= oneTileMovementDuration);

        foreach (var binding in bindings) {
            for (var i = 0; i < binding.pattern.feedbacks.Count; i++) {
                var feedback = binding.pattern.feedbacks[i];
                var curve = binding.curvePerFeedback[i];
                var t = curve.Evaluate(progress);

                feedback.UpdateData(
                    Time.deltaTime,
                    progress,
                    t,
                    binding.GetMovingFrom(),
                    binding.GetMovingTo(),
                    binding.human
                );
            }
        }
    }

    [Button]
    public void RegenerateHumans() {
        foreach (Transform human in humansContainer) {
            human.gameObject.SetActive(false);
        }

        bindings.Clear();

        var index = 0;
        var x = startingColumn;
        for (var column = 0; column < columns; column++) {
            var y = startingRow;
            for (var row = 0; row < rows; row++) {
                var human = Instantiate(humanPrefab, humansContainer);
                human.name = $"Human.{index++}";

                human.transform.localPosition = new Vector3(x, y, 0) + new Vector3(1, 1, 0) / 2;

                var path = new List<Vector2Int> {
                    new(x, y),
                    new Vector2Int(x, y) + new Vector2Int(travelDistanceX, travelDistanceY),
                };

                bindings.Add(new() {
                    human = human,
                    path = path,
                    pattern = new() {
                        feedbacks = defaultMovementFeedbacks.Select(i => i).ToList(),
                    },
                });

                y += rowsGap + 1;
            }

            x += columnsGap + 1;
        }
    }
}
}

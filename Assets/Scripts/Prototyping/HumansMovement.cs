using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Prototyping {
public class HumansMovement : MonoBehaviour {
    public float OneTileMovementDuration = 1f;

    public List<HumanBinding> Bindings = new();

    public int Rows = 9;
    public int Columns = 4;
    public int ColumnsGap = 1;
    public int RowsGap = 1;
    public int StartingRow;
    public int StartingColumn;
    public int TravelDistanceX = 2;
    public int TravelDistanceY;

    public Transform humansContainer;
    public GameObject humanPrefab;
    public List<MovementFeedback> defaultMovementFeedbacks;

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

        var progress = _movementElapsed / OneTileMovementDuration;
        Assert.IsTrue(progress <= 1);
        Assert.IsTrue(_movementElapsed <= OneTileMovementDuration);

        foreach (var binding in Bindings) {
            for (var i = 0; i < binding.Pattern.Feedbacks.Count; i++) {
                var feedback = binding.Pattern.Feedbacks[i];
                var curve = binding.CurvePerFeedback[i];
                var t = curve.Evaluate(progress);

                feedback.UpdateData(
                    Time.deltaTime,
                    progress,
                    t,
                    binding.GetMovingFrom(),
                    binding.GetMovingTo(),
                    binding.Human
                );
            }
        }
    }

    [Button]
    public void RegenerateHumans() {
        foreach (Transform human in humansContainer) {
            human.gameObject.SetActive(false);
        }

        Bindings.Clear();

        var index = 0;
        var x = StartingColumn;
        for (var column = 0; column < Columns; column++) {
            var y = StartingRow;
            for (var row = 0; row < Rows; row++) {
                var human = Instantiate(humanPrefab, humansContainer);
                human.name = $"Human.{index++}";

                human.transform.localPosition = new Vector3(x, y, 0) + new Vector3(1, 1, 0) / 2;

                var path = new List<Vector2Int> {
                    new(x, y),
                    new Vector2Int(x, y) + new Vector2Int(TravelDistanceX, TravelDistanceY),
                };

                Bindings.Add(new() {
                    Human = human,
                    Path = path,
                    Pattern = new() {
                        Feedbacks = defaultMovementFeedbacks.Select(i => i).ToList(),
                    },
                });

                y += RowsGap + 1;
            }

            x += ColumnsGap + 1;
        }
    }
}
}

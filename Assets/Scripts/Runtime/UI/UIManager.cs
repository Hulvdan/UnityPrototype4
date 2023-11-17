using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Console = BeastConsole.Console;

namespace BFG.Runtime {
public class UIManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    List<ResourceTMPTextMapping> _resourceTextsMapping;

    [SerializeField]
    [Required]
    Grid _mapGrid;

    [SerializeField]
    [Required]
    GameObject _itemPickupFeedbacks;

    [SerializeField]
    [Required]
    GameObject _itemPickupFeedbackPrefab;

    [FormerlySerializedAs("feedbackVerticalOffset")]
    [SerializeField]
    float _feedbackVerticalOffset;

    [FormerlySerializedAs("feedbackDuration")]
    [SerializeField]
    [Min(0)]
    float _feedbackDuration = 1f;

    [FormerlySerializedAs("feedbackCurve")]
    [SerializeField]
    AnimationCurve _feedbackCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [FormerlySerializedAs("feedbackCurve")]
    [SerializeField]
    AnimationCurve _feedbackOpacityCurve = AnimationCurve.Linear(1, 1, 0, 0);

    [Header("Debug")]
    [SerializeField]
    [Required]
    ScriptableResource _testResource;

    readonly List<IDisposable> _dependencyHooks = new();

    IMap _map;

    public void InitDependencies(IMap map) {
        _map = map;
        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();
        _dependencyHooks.Add(_map.onResourceChanged.Subscribe(OnResourceChanged));
        _dependencyHooks.Add(
            _map.onProducedResourcesPickedUp.Subscribe(OnProducedResourcesPickedUp)
        );
    }

    void Start() {
        Console.AddCommand(
            "pickup", "", this, _ => Debug_OnProducedResourcesPickedUp()
        );
    }

    void Debug_OnProducedResourcesPickedUp() {
        OnProducedResourcesPickedUp(new() {
            Position = new(1, 1),
            Resources = new() { new(Guid.NewGuid(), _testResource) },
        });
    }

    void OnResourceChanged(E_TopBarResourceChanged data) {
        foreach (var mapping in _resourceTextsMapping) {
            if (mapping.Resource != data.Resource) {
                continue;
            }

            mapping.Text.text = data.NewAmount.ToString();
            break;
        }
    }

    void OnHumanHarvestedResource(E_HumanPickedUpResource data) {
    }

    void OnProducedResourcesPickedUp(E_ProducedResourcesPickedUp data) {
        foreach (var res in data.Resources) {
            CreateFeedback(res, data.Position);
        }
    }

    void CreateFeedback(ResourceObj res, Vector2Int pos) {
        var feedback = Instantiate(_itemPickupFeedbackPrefab, _itemPickupFeedbacks.transform);
        var uiPos =
            Camera.main.WorldToScreenPoint(_mapGrid.LocalToWorld(new(pos.x + .5f, pos.y + .5f, 0)))
            - new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0);
        feedback.transform.localPosition = uiPos;

        var f = feedback.GetComponent<ItemPickupFeedback>();
        f.Init(res.script);

        DOTween
            .To(() => f.Group.alpha, val => f.Group.alpha = val, 0, _feedbackDuration)
            .SetEase(_feedbackOpacityCurve);

        DOTween
            .To(
                () => feedback.transform.localPosition,
                (Vector2 val) => feedback.transform.localPosition = val,
                uiPos + new Vector3(0, _feedbackVerticalOffset, 0),
                _feedbackDuration
            )
            .SetEase(_feedbackCurve)
            .OnComplete(() => Destroy(feedback));
    }
}
}

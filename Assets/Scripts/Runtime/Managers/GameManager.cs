using System.Reactive.Subjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class GameManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

    [SerializeField]
    [Required]
    MapRenderer _mapRenderer;

    [SerializeField]
    [Required]
    BuildablesPanel _buildablesPanel;

    public readonly Subject<SelectedItem> OnSelectedItemChanged = new();

    public SelectedItem selectedItem { get; private set; } = SelectedItem.None;

    void Start() {
        _map.InitDependencies();
        _mapRenderer.InitDependencies(this, _map);
        _buildablesPanel.InitDependencies(this);

        _map.Init();
        _buildablesPanel.Init();
    }

    void OnValidate() {
        _mapRenderer.InitDependencies(this, _map);
    }

    public void SetSelectedItem(SelectedItem item) {
        selectedItem = item;
        OnSelectedItemChanged.OnNext(item);
    }
}
}

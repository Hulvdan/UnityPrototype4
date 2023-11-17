using JetBrains.Annotations;

namespace BFG.Runtime {
public class SelectedItem {
    public SelectedItemType Type;

    [CanBeNull]
    public ScriptableBuilding Building;
}
}

using JetBrains.Annotations;

namespace BFG.Runtime.Entities {
public class ItemToBuild {
    public ItemToBuildType Type;

    [CanBeNull]
    public ScriptableBuilding Building;
}
}

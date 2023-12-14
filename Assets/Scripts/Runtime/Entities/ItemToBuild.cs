using JetBrains.Annotations;

namespace BFG.Runtime.Entities {
public class ItemToBuild {
    public ItemToBuildType type;

    [CanBeNull]
    public ScriptableBuilding building;
}
}

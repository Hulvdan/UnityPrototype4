using JetBrains.Annotations;

namespace BFG.Runtime {
public class Tile {
    public bool isBooked;
    public string Name;
    public int ResourceAmount;

    [CanBeNull]
    public ScriptableResource resource;

    void TakeResource(int amount) {
        ResourceAmount -= amount;
        if (ResourceAmount <= 0) {
            resource = null;
        }
    }
}
}

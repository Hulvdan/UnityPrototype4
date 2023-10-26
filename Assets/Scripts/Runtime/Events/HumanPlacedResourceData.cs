namespace BFG.Runtime {
public class HumanPlacedResourceData {
    public int Amount;
    public Human Human;
    public ScriptableResource Resource;

    public Building StoreBuilding;

    public HumanPlacedResourceData(int amount, Human human, Building storeBuilding,
        ScriptableResource resource) {
        Amount = amount;
        Human = human;
        StoreBuilding = storeBuilding;
        Resource = resource;
    }
}
}

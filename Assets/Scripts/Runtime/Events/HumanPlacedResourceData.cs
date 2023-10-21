using UnityEngine;

namespace BFG.Runtime {
public class HumanPlacedResourceData {
    public int Amount;
    public Human Human;

    public Building StoreBuilding;
    public ScriptableResource Resource;

    public HumanPlacedResourceData(int amount, Human human, Building storeBuilding,
        ScriptableResource resource) {
        Amount = amount;
        Human = human;
        StoreBuilding = storeBuilding;
        Resource = resource;
    }
}
}

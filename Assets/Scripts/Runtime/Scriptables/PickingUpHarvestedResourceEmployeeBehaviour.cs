namespace BFG.Runtime.Entities {
public sealed class PickingUpHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        // TODO: Event on started picking up the resource
    }

    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        // TODO: Event on finished picking up the resource
    }
}
}

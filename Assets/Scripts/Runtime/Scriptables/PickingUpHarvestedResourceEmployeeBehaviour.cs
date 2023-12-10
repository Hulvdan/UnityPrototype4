namespace BFG.Runtime.Entities {
public sealed class PickingUpHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override void OnEnter(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        // TODO: Event on started picking up the resource
    }

    public override void OnExit(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        // TODO: Event on finished picking up the resource
    }
}
}

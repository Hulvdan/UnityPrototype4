using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class BuildingController {
    public BuildingController(MainController controller) {
        _controller = controller;
    }

    public void OnEnter(
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.segment, null, "human.segment == null");
        Assert.AreEqual(human.moving.to, null, "human.movingTo == null");
        Assert.AreEqual(human.moving.path.Count, 0, "human.movingPath.Count == 0");

        Assert.AreEqual(human.stateBuilding_elapsed, 0);
        Assert.AreEqual(human.stateBuilding_progress, 0);
    }

    public void OnExit(
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();
    }

    public void Update(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        float dt
    ) {
        human.stateBuilding_elapsed += dt;
        // TODO!
        // human.stateBuilding_progress = human.
    }

    readonly MainController _controller;
}
}

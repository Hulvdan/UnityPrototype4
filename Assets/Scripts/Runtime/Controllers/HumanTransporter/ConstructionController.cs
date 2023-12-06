using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.Human {
public class ConstructionController {
    public ConstructionController(MainController controller) {
        _controller = controller;
    }

    public void OnEnter(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.type, Entities.Human.Type.Constructor);

        Assert.AreNotEqual(human.building, null, "human.building != null");
        Assert.AreEqual(human.segment, null, "human.segment == null");

        Assert.AreEqual(human.moving.to, null, "human.movingTo == null");
        Assert.AreEqual(human.moving.path.Count, 0, "human.movingPath.Count == 0");
        Assert.AreEqual(human.building.pos, human.moving.pos);

        Assert.IsFalse(human.building.isConstructed);
    }

    public void OnExit(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.building, null);
    }

    public void Update(
        Entities.Human human,
        HumanData data,
        float dt
    ) {
        var building = human.building;
        Assert.AreNotEqual(building, null);

        building!.constructionElapsed += dt;
        if (building.constructionElapsed > building.scriptable.ConstructionDuration) {
            building.constructionElapsed = building.scriptable.ConstructionDuration;
        }

        if (building.constructionProgress >= 1) {
            human.building = null;

            data.map.OnBuildingConstructed(building, human);

            _controller.SetState(human, MainState.MovingInTheWorld);
        }
    }

    readonly MainController _controller;
}
}

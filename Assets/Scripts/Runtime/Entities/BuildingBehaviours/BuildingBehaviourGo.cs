using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public enum BuildingBehaviourGoType {
    OutsourceHuman = 0,
    TakingResource = 1,
    ProcessingResource = 2,
}

[Serializable]
public class BuildingBehaviourGo {
    public BuildingBehaviourGoType Type;

    [CanBeNull]
    public EmployeeBehaviourSetGo EmployeeBehaviourSet;

    public BuildingBehaviour ToBuildingBehaviour() {
        switch (Type) {
            case BuildingBehaviourGoType.OutsourceHuman:
                Assert.AreNotEqual(EmployeeBehaviourSet, null);
                return new OutsourceHumanBuildingBehaviour(
                    EmployeeBehaviourSet.ToEmployeeBehaviourSet()
                );

            case BuildingBehaviourGoType.TakingResource:
                throw new NotImplementedException();

            case BuildingBehaviourGoType.ProcessingResource:
                throw new NotImplementedException();

            default:
                throw new NotSupportedException();
        }
    }
}
}

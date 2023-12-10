using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
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

    [ShowIf("Type", BuildingBehaviourGoType.OutsourceHuman)]
    public List<EmployeeBehaviourGo> EmployeeBehaviours;

    public BuildingBehaviour ToBuildingBehaviour() {
        switch (Type) {
            case BuildingBehaviourGoType.OutsourceHuman:
                Assert.AreNotEqual(EmployeeBehaviours.Count, 0);
                var behaviours = EmployeeBehaviours.Select(i => i.ToEmployeeBehaviour()).ToList();
                return new OutsourceHumanBuildingBehaviour(new(behaviours));

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

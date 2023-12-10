using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace BFG.Runtime.Entities {
public enum BuildingBehaviourGoType {
    OutsourceHuman = 0,
    TakingResource = 1,
    ProcessingResource = 2,
}

[Serializable]
public class BuildingBehaviourGo {
    [FormerlySerializedAs("Type")]
    [SerializeField]
    BuildingBehaviourGoType _type;

    [FormerlySerializedAs("EmployeeBehaviours")]
    [SerializeField]
    [ShowIf("Type", BuildingBehaviourGoType.OutsourceHuman)]
    List<EmployeeBehaviourGo> _employeeBehaviours;

    public BuildingBehaviour ToBuildingBehaviour() {
        switch (_type) {
            case BuildingBehaviourGoType.OutsourceHuman:
                Assert.AreNotEqual(_employeeBehaviours.Count, 0);
                var behaviours = new List<EmployeeBehaviour> {
                    Capacity = _employeeBehaviours.Count,
                };
                foreach (var beh in _employeeBehaviours) {
                    behaviours.Add(beh.ToEmployeeBehaviour());
                }

                return new OutsourceHumanBuildingBehaviour(new(behaviours));

            case BuildingBehaviourGoType.TakingResource:
                throw new NotImplementedException();

            case BuildingBehaviourGoType.ProcessingResource:
                throw new NotImplementedException();

            default:
                throw new NotImplementedException();
        }
    }
}
}

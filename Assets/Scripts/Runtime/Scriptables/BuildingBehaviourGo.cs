using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace BFG.Runtime.Entities {
public enum BuildingBehaviourGoType {
    OutsourceEmployee = 0,
    TakingResource = 1,
    ProcessingResource = 2,
}

[Serializable]
public class BuildingBehaviourGo {
    [SerializeField]
    BuildingBehaviourGoType _type;

    [SerializeField]
    [ShowIf("_type", BuildingBehaviourGoType.OutsourceEmployee)]
    List<EmployeeBehaviourGo> _employeeBehaviours;

    public BuildingBehaviour ToBuildingBehaviour() {
        switch (_type) {
            case BuildingBehaviourGoType.OutsourceEmployee:
                Assert.AreNotEqual(_employeeBehaviours.Count, 0);
                var behaviours = new List<EmployeeBehaviour> {
                    Capacity = _employeeBehaviours.Count,
                };

                foreach (var beh in _employeeBehaviours) {
                    behaviours.Add(beh.ToEmployeeBehaviour());
                }

                return new OutsourceEmployeeBuildingBehaviour(new(behaviours));

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

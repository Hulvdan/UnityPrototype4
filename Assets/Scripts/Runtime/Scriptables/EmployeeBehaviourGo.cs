#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime.Entities {
public enum EmployeeBehaviourGoType {
    GoToDestination = 1,
    Processing = 2,
    PickingUpHarvestedResource = 3,
    PlacingHarvestedResource = 4,
}

[Serializable]
public sealed class EmployeeBehaviourGo {
    [SerializeField]
    EmployeeBehaviourGoType _type;

    [SerializeField]
    [ShowIf("_type", EmployeeBehaviourGoType.GoToDestination)]
    HumanDestinationType _destinationType;

    public EmployeeBehaviour ToEmployeeBehaviour() {
        switch (_type) {
            case EmployeeBehaviourGoType.GoToDestination:
                return new GoToDestinationEmployeeBehaviour(_destinationType);
            case EmployeeBehaviourGoType.Processing:
                return new ProcessingEmployeeBehaviour();
            case EmployeeBehaviourGoType.PickingUpHarvestedResource:
                return new PickingUpHarvestedResourceEmployeeBehaviour();
            case EmployeeBehaviourGoType.PlacingHarvestedResource:
                return new PlacingHarvestedResourceEmployeeBehaviour();
            default:
                throw new NotImplementedException();
        }
    }
}
}

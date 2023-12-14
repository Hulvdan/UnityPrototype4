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

    [SerializeField]
    [ShowIf("_type", EmployeeBehaviourGoType.GoToDestination)]
    [Min(-1)]
    int _bookingTileBehaviourId = -1;

    [SerializeField]
    [ShowIf("_type", EmployeeBehaviourGoType.Processing)]
    [Min(-1)]
    int _unbookingTileBehaviourId = -1;

    public EmployeeBehaviour ToEmployeeBehaviour() {
        switch (_type) {
            case EmployeeBehaviourGoType.GoToDestination:
                return new GoToDestinationEmployeeBehaviour(
                    _destinationType,
                    _bookingTileBehaviourId == -1 ? null : _bookingTileBehaviourId
                );
            case EmployeeBehaviourGoType.Processing:
                int? unbookingId = null;
                if (_unbookingTileBehaviourId == -1) {
                    unbookingId = _unbookingTileBehaviourId;
                }

                return new ProcessingEmployeeBehaviour(unbookingId);
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

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime.Entities {
public enum EmployeeBehaviourGoType {
    ChooseDestination = 0,
    GoToDestination = 1,
    Processing = 2,
    PickingUpHarvestedResource = 3,
    PlacingHarvestedResource = 4,
}

[Serializable]
public sealed class EmployeeBehaviourGo {
    public EmployeeBehaviourGoType Type;

    public EmployeeBehaviour ToEmployeeBehaviour() {
        switch (Type) {
            case EmployeeBehaviourGoType.ChooseDestination:
            case EmployeeBehaviourGoType.GoToDestination:
            case EmployeeBehaviourGoType.Processing:
            case EmployeeBehaviourGoType.PickingUpHarvestedResource:
            case EmployeeBehaviourGoType.PlacingHarvestedResource:
                throw new NotImplementedException();
            default:
                throw new NotSupportedException();
        }
    }
}

[Serializable]
public sealed class EmployeeBehaviourSetGo {
    public List<EmployeeBehaviourGo> Behaviours = new();

    public EmployeeBehaviourSet ToEmployeeBehaviourSet() {
        var list = Behaviours.Select(i => i.ToEmployeeBehaviour()).ToList();
        return new(list);
    }
}
}

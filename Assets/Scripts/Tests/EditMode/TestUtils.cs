using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Tests.EditMode {
public static class TestUtils {
    public static void AssertSetEquals<T>(IEnumerable<T> a, IEnumerable<T> b) {
        Assert.IsTrue(SetEquals(a, b));
    }

    static bool SetEquals<T>(IEnumerable<T> a, IEnumerable<T> b) {
        var setA = new HashSet<T>();
        var setB = new HashSet<T>();

        foreach (var item in a) {
            Assert.IsFalse(setA.Contains(item));
            setA.Add(item);
        }

        foreach (var item in b) {
            Assert.IsFalse(setB.Contains(item));
            setB.Add(item);
        }

        return setA.SetEquals(setB);
    }
}
}

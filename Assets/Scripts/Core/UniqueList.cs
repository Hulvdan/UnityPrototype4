using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BFG.Core {
public class UniqueList<T> : IList<T> {
    public IEnumerator<T> GetEnumerator() {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(T item) {
        foreach (var item1 in _list) {
            Assert.IsFalse(ReferenceEquals(item, item1));
        }

        _list.Add(item);
    }

    public void Clear() {
        _list.Clear();
    }

    public bool Contains(T item) {
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        return _list.Remove(item);
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) {
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item) {
        foreach (var item1 in _list) {
            Assert.IsFalse(ReferenceEquals(item, item1));
        }

        _list.Insert(index, item);
    }

    public void RemoveAt(int index) {
        _list.RemoveAt(index);
    }

    public T this[int index] {
        get => _list[index];
        set {
            foreach (var item1 in _list) {
                Assert.IsFalse(ReferenceEquals(value, item1));
            }

            _list[index] = value;
        }
    }

    readonly List<T> _list = new();
}
}

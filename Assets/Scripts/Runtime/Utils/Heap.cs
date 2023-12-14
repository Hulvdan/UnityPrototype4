using System;
using System.Collections.Generic;

namespace BFG.Runtime {
public class Heap<T> {
    readonly List<T> _list = new();
    readonly Comparer<T> _comparer;

    public Heap(Comparer<T> comparer) {
        _comparer = comparer;
    }

    public Heap(Comparison<T> comparison) {
        _comparer = Comparer<T>.Create(comparison);
    }

    public Heap() {
        _comparer = Comparer<T>.Default;
    }

    public void Add(T value) {
        _list.Add(value);
        HeapifyUp(_list.Count - 1);
    }

    public T PeekTop() {
        return _list[0];
    }

    public T PopTop() {
        var result = PeekTop();
        Swap(0, _list.Count - 1);
        _list.RemoveAt(_list.Count - 1);
        HeapifyDown(0);
        return result;
    }

    public void Remove(T value) {
        if (_list[_list.Count - 1].Equals(value)) {
            _list.RemoveAt(_list.Count - 1);
            return;
        }

        var index = _list.IndexOf(value);
        Swap(index, _list.Count - 1);
        _list.RemoveAt(_list.Count - 1);

        var parentIndex = (index - 1) / 2;
        if (_comparer.Compare(_list[index], _list[parentIndex]) > 0) {
            HeapifyUp(index);
        }
        else {
            HeapifyDown(index);
        }
    }

    public int count => _list.Count;

    void HeapifyUp(int index) {
        while (index > 0) {
            var parentIndex = (index - 1) / 2;
            if (_comparer.Compare(_list[index], _list[parentIndex]) > 0) {
                Swap(index, parentIndex);
                index = parentIndex;
            }
            else {
                break;
            }
        }
    }

    void HeapifyDown(int index) {
        while (true) {
            var leftChildIndex = index * 2 + 1;
            if (leftChildIndex >= _list.Count) {
                return;
            }

            var bestChildIndex = leftChildIndex;

            var rightChildIndex = index * 2 + 2;
            if (rightChildIndex < _list.Count) {
                if (_comparer.Compare(_list[rightChildIndex], _list[leftChildIndex]) > 0) {
                    bestChildIndex = rightChildIndex;
                }
            }

            if (_comparer.Compare(_list[bestChildIndex], _list[index]) > 0) {
                Swap(bestChildIndex, index);
                index = bestChildIndex;
            }
            else {
                break;
            }
        }
    }

    void Swap(int i, int j) {
        (_list[i], _list[j]) = (_list[j], _list[i]);
    }
}

public static class MaxHeap {
    public static Heap<T> Create<T>() {
        return new();
    }

    public static Heap<T> Create<T>(Func<T, int> keySelector) {
        Comparison<T> comparison = (x, y) => keySelector(x) - keySelector(y);
        return new(comparison);
    }

    public static Heap<T> Create<T, Key>(Func<T, Key> keySelector, Comparer<Key> keyComparer) {
        Comparison<T> comparison = (x, y) => keyComparer.Compare(keySelector(x), keySelector(y));
        return new(comparison);
    }

    public static Heap<T> Create<T, Key>(Func<T, Key> keySelector, Comparison<Key> keyComparer) {
        Comparison<T> comparison = (x, y) => keyComparer(keySelector(x), keySelector(y));
        return new(comparison);
    }

    public static Heap<T> Create<T, Key>(Func<T, Key> keySelector) {
        Comparison<T> comparison = (x, y) =>
            Comparer<Key>.Default.Compare(keySelector(x), keySelector(y));
        return new(comparison);
    }

    public static Heap<T> Create<T>(Comparer<T> comparer) {
        return new(comparer);
    }

    public static Heap<T> Create<T>(Comparison<T> comparison) {
        return new(comparison);
    }
}

public static class MinHeap {
    public static Heap<T> Create<T>() {
        return new((x, y) => Comparer<T>.Default.Compare(y, x));
    }

    public static Heap<T> Create<T>(Func<T, float> keySelector) {
        return new(Comparison);

        int Comparison(T x, T y) {
            var cmp = keySelector(y) - keySelector(x);
            return cmp == 0 ? 0 : Math.Sign(cmp);
        }
    }

    public static Heap<T> Create<T>(Comparison<T> comparison) {
        return new((x, y) => comparison(y, x));
    }
}
}

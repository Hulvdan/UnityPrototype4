#nullable enable
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public static class Tracing {
    static readonly bool TracingEnabled = Debug.isDebugBuild;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Scope() {
        if (!TracingEnabled) {
            return Disposable.Empty;
        }

        var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
        return Scope(method.DeclaringType.Name, method.Name);
    }

    public static void LogError(string text) {
        Log(2, text);
    }

    public static void LogWarning(string text) {
        Log(1, text);
    }

    public static void Log(string text) {
        Log(0, text);
    }

    static IDisposable Scope(params string[] name) {
        var scope = GetNew();
        if (name.Length > 0) {
            var joinedName = string.Join('.', name);
            scope.Traces.Add(new(0, ""));
            scope.Traces.Add(new(0, $"[{joinedName}]"));
        }

        return Disposable.Create(() => {
            var current = Current()!;
            var parent = Parent();
            Assert.IsTrue(current != null);

            if (parent == null) {
                if (current!.Traces.Count > 2) {
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, current.Format());
                }

                InvalidateTop();
            }
            else {
                if (current!.Traces.Count > 2) {
                    current.FormatForParent(parent);
                }

                InvalidateTop();
            }
        });
    }

    static void Log(int level, string text) {
        if (!TracingEnabled) {
            return;
        }

        var scope = Current();
        if (scope == null) {
            switch (level) {
                case 0:
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, text);
                    break;
                case 1:
                    Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, text);
                    break;
                case 2:
                    Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, text);
                    break;
            }

            return;
        }

        scope.Traces.Add(new(level, text));
    }

    static readonly List<TracingScope> Pool = new();
    static int _current = -1;

    static TracingScope GetNew() {
        if (_current < Pool.Count - 1) {
            _current++;
            return Pool[_current];
        }

        var scope = new TracingScope();
        Pool.Add(scope);
        _current = Pool.Count - 1;
        return scope;
    }

    static TracingScope? Current() {
        if (_current >= 0) {
            return Pool[_current];
        }

        return null;
    }

    static TracingScope? Parent() {
        if (_current - 1 >= 0) {
            return Pool[_current - 1];
        }

        return null;
    }

    static void InvalidateTop() {
        Assert.AreNotEqual(Pool.Count, 0);
        Pool[_current].Traces.Clear();
        _current--;
    }
}

internal class TracingScope {
    public readonly List<(int, string)> Traces = new();

    public string Format() {
        var res = new List<string>();
        foreach (var (p, t) in Traces) {
            switch (p) {
                case 0:
                    res.Add("" + t);
                    break;
                case 1:
                    res.Add("[WARN] " + t);
                    break;
                case 2:
                    res.Add("[ERROR] " + t);
                    break;
            }
        }

        return string.Join('\n', res);
    }

    public void FormatForParent(TracingScope parent) {
        foreach (var (p, t) in Traces) {
            switch (p) {
                case 0:
                    parent.Traces.Add(new(0, "\t" + t));
                    break;
                case 1:
                    parent.Traces.Add(new(0, "\t[WARN] " + t));
                    break;
                case 2:
                    parent.Traces.Add(new(0, "\t[ERROR] " + t));
                    break;
            }
        }
    }
}
}

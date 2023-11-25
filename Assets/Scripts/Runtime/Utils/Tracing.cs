#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public static class Tracing {
    static readonly bool TracingEnabled = Debug.isDebugBuild;
    static StreamWriter? _writer;
    static bool _closing;
    static int _collapseNumber;
    static string _previousString = null!;

    static StreamWriter? writer {
        get {
            if (_closing) {
                return null;
            }

            if (_writer == null) {
                _writer = new(
                    Application.dataPath
                    + Path.DirectorySeparatorChar
                    + ".."
                    + Path.DirectorySeparatorChar
                    + "Logs"
                    + Path.DirectorySeparatorChar
                    + "Tracing.log",
                    true,
                    Encoding.UTF8
                );
            }

            Application.logMessageReceived += (condition, trace, type) => {
                switch (type) {
                    case LogType.Error:
                    case LogType.Assert:
                    case LogType.Warning:
                    case LogType.Exception:
                        Log(type, condition + "\n" + trace);
                        break;
                    case LogType.Log:
                    default:
                        break;
                }
            };
            return _writer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Scope() {
        if (!TracingEnabled) {
            return Disposable.Empty;
        }

        var method = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
        return Scope(method.DeclaringType!.Name, method.Name);
    }

    public static void LogError(string text) {
        Log(LogType.Error, text);
    }

    public static void LogAssert(string text) {
        Log(LogType.Assert, text);
    }

    public static void LogWarning(string text) {
        Log(LogType.Warning, text);
    }

    public static void Log(string text) {
        Log(LogType.Log, text);
    }

    public static void LogException(string text) {
        Log(LogType.Exception, text);
    }

    public static void DisposeWriter() {
        _closing = true;
        if (_writer == null) {
            return;
        }

        _writer.Close();
        _writer = null;
    }

    static IDisposable Scope(params string[] name) {
        _ = GetNew();

        string joinedName;
        if (name.Length > 0) {
            joinedName = string.Join('.', name);
            // scope.Traces.Add(new(0, ""));
        }
        else {
            joinedName = "-anonymous-scope-";
        }

        var item2 = $"[{joinedName}]";
        // scope.Traces.Add(new(0, item2));

        Log(LogType.Log, item2);
        // writer?.WriteLine(
        //     new string('\t', Math.Max(0, _current)) + Prepend(LogType.Log, item2)
        // );
        return Disposable.Create(() => { _current--; });
        // return Disposable.Create(() => {
        //     var current = Current()!;
        //     var parent = Parent();
        //     Assert.IsTrue(current != null);
        //
        //     if (parent == null) {
        //         if (current!.Traces.Count > 2) {
        //             _writingToConsole = true;
        //             Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, current.Format());
        //             _writingToConsole = false;
        //         }
        //
        //         InvalidateTop();
        //     }
        //     else {
        //         if (current!.Traces.Count > 2) {
        //             current.FormatForParent(parent);
        //         }
        //
        //         InvalidateTop();
        //     }
        //
        //     // if (_current >= 0) {
        //     //     // for (var i = _current; i >= 1; i--) {
        //     //     //     Pool[i].FormatForParent(Pool[i - 1]);
        //     //     // }
        //     //     //
        //     //     // writer?.WriteLine(Pool[0].Format());
        //     //     // Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, formatErr);
        //     // }
        // });
    }

    static void Log(LogType type, string text) {
        if (!TracingEnabled) {
            return;
        }

        var newString = new string('\t', Math.Max(0, _current)) + Prepend(type, text);
        if (_previousString == newString) {
            _collapseNumber++;
            return;
        }

        if (_collapseNumber > 0) {
            writer?.WriteLine($"<-- {_collapseNumber} more -->");
            _collapseNumber = 0;
        }

        writer?.WriteLine(newString);
        _previousString = newString;

        // var scope = Current();
        //
        // if (scope == null) {
        //     writer?.WriteLine(text);
        //     // _writingToConsole = true;
        //     // Debug.LogFormat(level, LogOption.NoStacktrace, null, text);
        //     // _writingToConsole = false;
        //     return;
        // }
        // scope.Traces.Add(new(level, text));
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

    public static string Prepend(LogType type, string text) {
        switch (type) {
            case LogType.Error:
                return "[ERROR] " + text;
            case LogType.Assert:
                return "[ASSERT] " + text;
            case LogType.Warning:
                return "[WARN] " + text;
            case LogType.Log:
                return text;
            case LogType.Exception:
                return "[EXCEPTION] " + text;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

internal class TracingScope {
    public readonly List<(LogType, string)> Traces = new();

    public string Format() {
        var res = new List<string>();
        foreach (var (type, text) in Traces) {
            res.Add(Tracing.Prepend(type, text));
        }

        return string.Join('\n', res);
    }

    public string FormatErr() {
        var res = new List<string>();
        foreach (var (_, text) in Traces) {
            res.Add("[ERROR] " + text);
        }

        return string.Join('\n', res);
    }

    public void FormatForParent(TracingScope parent) {
        foreach (var (type, text) in Traces) {
            parent.Traces.Add(new(0, "\t" + Tracing.Prepend(type, text)));
        }
    }
}
}

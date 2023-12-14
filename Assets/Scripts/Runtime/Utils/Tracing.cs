#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BFG.Runtime {
public static class Tracing {
    static readonly bool _TRACING_ENABLED = Debug.isDebugBuild;
    static TextWriter? _writer;
    static bool _closing;
    static int _collapseNumber;
    static string _previousString = null!;

    static TextWriter? writer {
        get {
            if (_closing) {
                return null;
            }

            if (_writer == null) {
                _writer = TextWriter.Synchronized(new StreamWriter(
                    Application.dataPath
                    + Path.DirectorySeparatorChar
                    + ".."
                    + Path.DirectorySeparatorChar
                    + "Logs"
                    + Path.DirectorySeparatorChar
                    + "Tracing.log",
                    true,
                    Encoding.UTF8
                ));
                _writer.WriteLine($"\n\n\n===== RUN FROM {DateTime.Now:s}");
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
        if (!_TRACING_ENABLED) {
            return Disposable.Empty;
        }

        var method = new StackTrace().GetFrame(1).GetMethod();
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
        _currentIndentationLevel++;

        string joinedName;
        if (name.Length > 0) {
            joinedName = string.Join('.', name);
        }
        else {
            joinedName = "-anonymous-scope-";
        }

        var item2 = $"[{joinedName}]";

        Log(LogType.Log, item2);
        return Disposable.Create(() => { _currentIndentationLevel--; });
    }

    static void Log(LogType type, string text) {
        if (!_TRACING_ENABLED) {
            return;
        }

        var newString = new string('\t', Math.Max(0, _currentIndentationLevel))
                        + Prepend(type, text);
        if (_previousString == newString) {
            _collapseNumber++;
            return;
        }

        if (_collapseNumber > 0) {
            writer?.WriteLine($"<-- {_collapseNumber} more -->");
            _collapseNumber = 0;
        }

        writer?.WriteLine(newString);
        writer?.Flush();
        _previousString = newString;
    }

    static string Prepend(LogType type, string text) {
        return type switch {
            LogType.Error => "[ERROR] " + text,
            LogType.Assert => "[ASSERT] " + text,
            LogType.Warning => "[WARN] " + text,
            LogType.Log => text,
            LogType.Exception => "[EXCEPTION] " + text,
            _ => throw new NotSupportedException(),
        };
    }

    static int _currentIndentationLevel = -1;
}
}

using System.Diagnostics;

namespace csso.Common;

public static class Check {
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void True(bool condition) {
        if (!condition) throw new Exception();
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void False(bool condition) {
        if (condition) throw new Exception();
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Argument(bool condition, string argname) {
        if (!condition) throw new ArgumentException("argname");
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Fail() {
        throw new Exception();
    }
}
using System;
using System.Diagnostics;

namespace csso.Common;

internal class CheckException : Exception {
    public CheckException(string message)
        : base(message) {
    }
}

public static class Check {
    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void True(bool condition) {
        if (!condition) {
            throw new CheckException("qw4c52134");
        }
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void False(bool condition) {
        if (condition) {
            throw new CheckException("3gvu564y3");
        }
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Argument(bool condition, string argname) {
        if (!condition) {
            throw new ArgumentException(argname);
        }
    }

    [DebuggerStepThrough]
    [DebuggerHidden]
    public static void Fail() {
        throw new Exception("Fail()");
    }
}
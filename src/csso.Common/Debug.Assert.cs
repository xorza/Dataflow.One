using System;
using System.Diagnostics;

namespace csso.Common;

public static class Debug {
    static Debug() {
        IsDebug = false;
        Init();
    }

    public static bool IsDebug { get; private set; }

    [Conditional("DEBUG")]
    private static void Init() {
        IsDebug = true;
    }


    public static class Assert {
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void True(bool condition) {
            if (!condition) {
                throw new AssertionException();
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void True(Func<bool> predicate) {
            if (!predicate.Invoke()) {
                throw new AssertionException();
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void AreSame<T>(T a, T b) where T : class {
            if (!ReferenceEquals(a, b)) {
                throw new AssertionException();
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void NotNull<T>(T? o) where T : class {
            if (o == null) {
                throw new AssertionException();
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static void False() {
            throw new AssertionException();
        }
    }
}
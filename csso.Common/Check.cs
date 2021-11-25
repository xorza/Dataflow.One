using System;
using System.Diagnostics;

namespace csso.Common {
public static class Check {
    [DebuggerStepThrough]
    public static void True(bool condition) {
        if (!condition) throw new Exception();
    }
}
}
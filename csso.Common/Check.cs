using System;

namespace csso.Common {
public static class Check {
    public static void True(bool condition) {
        if (!condition) throw new Exception();
    }
}
}
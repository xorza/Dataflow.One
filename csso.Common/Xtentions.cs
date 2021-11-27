using System;
using System.Collections;
using System.Collections.Generic;

namespace csso.Common {
public static class Xtentions {
    public static IEnumerable<T> Foreach<T>(this IEnumerable<T> e, Action<T> action) {
        foreach (var item in e)
            action(item);
        return e;
    }

    public static IList Foreach<T>(this IList e, Action<T> action) {
        foreach (T item in e)
            action(item);
        return e;
    }
}
}
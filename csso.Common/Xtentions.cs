using System;
using System.Collections.Generic;

namespace csso.Common {
public static class Xtentions {
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e, Action<T> action) {
        foreach (var item in e) action(item);

        return e;
    }
}
}
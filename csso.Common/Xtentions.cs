using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T> e) {
        return e.Where(_ => !Object.ReferenceEquals(_, null));
    }
}
}
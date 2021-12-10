using System.Collections;

namespace csso.Common; 

public static class Xtentions {
    public static IEnumerable<T> Foreach<T>(this IEnumerable<T> e, Action<T> action) {
        foreach (var item in e)
            action(item);
        return e;
    }

    public static IList Foreach<T>(this IList e, Action<T> action) {
        foreach (var item in e)
            if (item is T tItem)
                action(tItem);
            else if (item == null)
                throw new Exception("wvert23b46");
            else
                throw new Exception("sergtg2v45");

        return e;
    }

    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T> e) {
        return e.Where(_ => !ReferenceEquals(_, null));
    }
}
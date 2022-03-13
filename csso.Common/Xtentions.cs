using System.Collections;

namespace csso.Common;

public static class Xtentions {
    public static IEnumerable<T> Foreach<T>(this IEnumerable<T> e, Action<T> action) {
        foreach (var item in e)
            action(item);
        return e;
    }

    public static IEnumerable Foreach<T>(this IEnumerable e, Action<T> action) {
        var enumerable = e as object[] ?? e.Cast<object>().ToArray();
        foreach (var item in enumerable)
            if (item is T tItem)
                action(tItem);
            else if (item == null)
                throw new Exception("wvert23b46");
            else
                throw new Exception("sergtg2v45");

        return enumerable;
    }

    public static IEnumerable<T> SkipNulls<T>(this IEnumerable<T?> e) {
        List<T> result = new();
        e.Foreach(_ => {
            if (_ != null)
                result.Add(_);
        });

        return result;
    }

    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        return !source.Any(predicate);
    }
    
    public static void Populate<T>(this IList<T> list, T value) {
        for (int i = 0; i < list.Count; i++) {
            list[i] = value;
        }
    }
}
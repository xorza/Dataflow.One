using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace csso.Common;

public static class Xtentions {
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e, Action<T> action) {
        var forEach = e.ToList();

        foreach (var item in forEach) action(item);

        return forEach;
    }

    public static void AddTo<T>(this IEnumerable<T> e, ICollection<T> collection) {
        foreach (var item in e) collection.Add(item);
    }

    public static void AddTo<T>(this IEnumerable<T> e, List<T> collection) {
        collection.AddRange(e);
    }

    public static IEnumerable ForEach<T>(this IEnumerable e, Action<T> action) {
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
        e.ForEach(_ => {
            if (_ != null)
                result.Add(_);
        });

        return result;
    }

    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        return !source.Any(predicate);
    }

    public static void Populate<T>(this IList<T> list, T value) {
        for (var i = 0; i < list.Count; i++) list[i] = value;
    }


    public static bool IsEmpty(this string s) {
        return s.Trim().Length == 0;
    }

    public static object? GetDefault(this Type type) {
        if (type.IsValueType) return Activator.CreateInstance(type);

        return null;
    }
}
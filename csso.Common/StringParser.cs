using System;

namespace csso.Common;

public static class StringParser {
    public static bool TryParse(string s, Type type, out object? result) {
        result = null;
        var parser = type.GetMethod("Parse", new[] {typeof(string)});
        if (parser == null)
            return false;
        if (!parser.IsStatic)
            return false;
        if (parser.ReturnType != type)
            return false;

        result = parser.Invoke(null, new object?[] {s});
        return true;
    }
}
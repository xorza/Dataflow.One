namespace csso.Common;

public static class StringParser {
    public static bool TryParse(String s, Type type, out Object? result) {
        result = null;
        var parser = type.GetMethod("Parse", new Type[] {typeof(string)});
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
using System;
using System.Linq;
using csso.Common;

namespace csso.ImageProcessing {
public enum DataType {
    Float,
    Float2,
    Float3,
    Float4,
}

internal static partial class Xtensions {
    public static T ToEnum<T>(this String s) where T : struct, Enum {
        try {
            return (T) Enum.Parse(typeof(T), s);
        }
        catch { }

        try {
            String[] names = Enum.GetNames<T>();

            Int32 index = Array.FindIndex(names, name => name.Equals(s, StringComparison.InvariantCultureIgnoreCase));
            Check.True(index >= 0);
            return Enum.GetValues<T>()[index];
        }
        catch { }

        throw new ArgumentOutOfRangeException(nameof(s));
    }
}
}
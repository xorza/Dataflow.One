using System;
using System.Diagnostics;
using csso.Common;

namespace csso.OpenCL;

public enum DataType {
    Float,
    Float2,
    Float3,
    Float4,
    UChar,
    UChar2,
    UChar3,
    UChar4,
    UShort,
    UShort2,
    UShort3,
    UShort4,
    Int,
    Int2,
    Int3,
    Int4,
    Image2D_t
}

internal static partial class Xtensions {
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
    [DebuggerHidden]
    public static T ToEnum<T>(this string s) where T : struct, Enum {
        try {
            return (T) Enum.Parse(typeof(T), s);
        }
        catch { }

        try {
            var names = Enum.GetNames<T>();

            var index = Array.FindIndex(names, name => name.Equals(s, StringComparison.InvariantCultureIgnoreCase));
            Check.True(index >= 0);
            return Enum.GetValues<T>()[index];
        }
        catch { }

        throw new ArgumentOutOfRangeException(nameof(s));
    }
}
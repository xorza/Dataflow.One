using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace csso.OpenCL;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class NameAttribute : Attribute {
    public NameAttribute(string name) {
        Name = name;
    }

    public string Name { get; }
}

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
    [Name("image2d_t")] Image2D
}

internal static partial class Xtensions {
    [DebuggerStepThrough]
    [DebuggerNonUserCode]
    [DebuggerHidden]
    public static T ToEnum<T>(this string s) where T : struct, Enum {
        var enumType = typeof(T);

        if (Enum.TryParse(enumType, s, out var result)) {
            return (T) result!;
        }

        var names = Enum.GetNames<T>();
        var values = Enum.GetValues<T>();

        var index = Array.FindIndex(names, name => name.Equals(s, StringComparison.InvariantCultureIgnoreCase));
        if (index >= 0) {
            return values[index];
        }

        var valueName = enumType
            .GetMembers()
            .Where(_ => _.DeclaringType == enumType)
            .SelectMany(member =>
                member
                    .GetCustomAttributes<NameAttribute>()
                    .Select(nameAttr => new {member, name = nameAttr.Name}))
            .SingleOrDefault(_ => _.name == s)
            ?.member.Name;

        if (valueName != null) {
            index = Array.FindIndex(names, name => name.Equals(valueName, StringComparison.InvariantCultureIgnoreCase));
            if (index >= 0) {
                return values[index];
            }
        }

        throw new ArgumentException(nameof(s));
    }
}
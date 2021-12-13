using System.ComponentModel;
using csso.NodeCore.Annotations;

namespace csso.NodeCore.Funcs;

public static class F {
    [Reactive]
    public static bool Add(
        Int32 a,
        Int32 b,
        [Output] ref Int32 result) {
        result = a + b;
        return true;
    }


    [Reactive]
    [Description("testestsetsetset")]
    public static bool DivideWhole(
        Int32 a,
        Int32 b,
        [Description("testestsetsetset1")] [Output]
        ref Int32 result,
        [Output] ref Int32 remainder
    ) {
        result = a / b;
        remainder = a % b;
        return true;
    }
}

public class OutputFunc<T> {
    public T Value { get; set; }

    public Delegate Delegate { get; }

    public OutputFunc() {
        Delegate = Func_;
    }

    [Reactive]
    private bool Func_(T arg) {
        Value = arg;
        return true;
    }
}

public class ValueFunc<T> {
    public T Value { get; set; }

    public Delegate Delegate { get; }

    public ValueFunc() {
        Delegate = Func_;
    }

    [Reactive]
    private bool Func_([Output] out T arg) {
        arg = Value;
        return true;
    }
}
using System;
using System.ComponentModel;
using System.Reflection;
using csso.NodeCore.Run;

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

public class OutputFunc<T> : Function {
    public OutputFunc() {
        Name = "Output";
        SetFunction(Func_);
    }

    public T Value { get; set; }

    [Reactive]
    private bool Func_(T arg) {
        Value = arg;
        return true;
    }
}

public abstract class ConstantFunc : StatefulFunction {
    public Type Type { get; }

    protected ConstantFunc(String name, Type type) {
        Name = name;
        Type = type;
        Behavior = FunctionBehavior.Reactive;
    }
}

public sealed class ConstantFunc<T> : ConstantFunc {
    public ConstantFunc(String name)
        : this(name, new DataCompatibility().DefaultValue<T>()) { }

    public ConstantFunc(String name, T? defaultValue) : base(name, typeof(T)) {
        SetFunction(Func_);
        TypedValue = defaultValue;
    }

    public override Function CreateInstance() {
        return new ConstantFunc<T>(Name);
    }

    public T? TypedValue { get; set; }

    private bool Func_([Output] out T? value) {
        value = TypedValue;
        return true;
    }
}

public class FrameNoFunc : Function {
    public FrameNoFunc() {
        Name = "Frame number";
        SetFunction(Func_);
    }

    public Executor? Executor { get; set; }

    private bool Func_([Output] out Int32 frameNo) {
        frameNo = Executor?.FrameNo ?? 0;
        return true;
    }
}
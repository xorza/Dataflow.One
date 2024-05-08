using System;
using System.ComponentModel;
using csso.NodeCore.Run;

namespace csso.NodeCore.Funcs;

public static class F {
    [Reactive]
    public static bool Add(
        int a,
        int b,
        [Output] ref int result) {
        result = a + b;
        return true;
    }


    [Reactive]
    [Description("testestsetsetset")]
    public static bool DivideWhole(
        int a,
        int b,
        [Description("testestsetsetset1")] [Output]
        ref int result,
        [Output] ref int remainder
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
    protected ConstantFunc(string name, Type type) {
        Name = name;
        Type = type;
        Behavior = FunctionBehavior.Reactive;
    }

    public Type Type { get; }
}

public sealed class ConstantFunc<T> : ConstantFunc {
    public ConstantFunc(string name)
        : this(name, DataCompatibility.Instance.DefaultValue<T>()) {
    }

    public ConstantFunc(string name, T? defaultValue) : base(name, typeof(T)) {
        SetFunction(Func_);
        TypedValue = defaultValue;
    }

    public T? TypedValue { get; set; }

    public override Function CreateInstance() {
        return new ConstantFunc<T>(Name);
    }

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

    private bool Func_([Output] out int frameNo) {
        frameNo = Executor?.FrameNo ?? 0;
        return true;
    }
}
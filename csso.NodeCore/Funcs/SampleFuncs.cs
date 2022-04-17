using System.ComponentModel;
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
        Refresh("Output", Func_);
    }

    public T Value { get; set; }

    [Reactive]
    private bool Func_(T arg) {
        Value = arg;
        return true;
    }
}

public class ValueFunc<T> : Function {
    public ValueFunc() {
        Refresh("Const", Func_);
    }

    public T Value { get; set; }

    private bool Func_([Output] out T arg) {
        arg = Value;
        return true;
    }
}

public class FrameNoFunc : Function {
    public FrameNoFunc() {
        Refresh("Frame number", Func_);
    }

    public Executor? Executor { get; set; }

    private bool Func_([Output] out Int32 frameNo) {
        frameNo = Executor?.FrameNo ?? 0;
        return true;
    }
}
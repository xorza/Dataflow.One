using System;

namespace csso.NodeCore {
public enum PutType {
    In,
    Out
}

public abstract class FunctionArg {
    protected FunctionArg() { }

    protected FunctionArg(string name, Type type) {
        Name = name;
        Type = type;
    }

    public Type Type { get; set; } = typeof(void);
    public string Name { get; set; } = "";
    public abstract PutType PutType { get; }
}

public class FunctionInput : FunctionArg {
    public FunctionInput() { }
    public FunctionInput(string name, Type type) : base(name, type) { }

    public override PutType PutType => PutType.In;
}

public class FunctionOutput : FunctionArg {
    public FunctionOutput() { }
    public FunctionOutput(string name, Type type) : base(name, type) { }

    public override PutType PutType => PutType.Out;
}
}
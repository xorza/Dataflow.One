using csso.Common;

namespace csso.NodeCore;

public enum ArgType {
    In,
    Out,
    Config
}

public abstract class FunctionArg {
    protected FunctionArg(string name, Type type, Int32 argumentIndex) {
        Name = name;
        Type = type;
        ArgumentIndex = argumentIndex;
    }

    public Type Type { get; }
    public string Name { get; }
    public string FullName => Name + ":" + Type.Name;
    public abstract ArgType ArgType { get; }
    public Int32 ArgumentIndex { get; }
}

public class FunctionInput : FunctionArg {
    public FunctionInput(string name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public override ArgType ArgType => ArgType.In;
    
}

public class FunctionOutput : FunctionArg {
    public FunctionOutput(string name, Type type, Int32 argumentIndex) : base(name, type, argumentIndex) { }

    public override ArgType ArgType => ArgType.Out;
}
using csso.Common;

namespace csso.NodeCore;

public enum ArgType {
    In,
    Out
}

public  class FunctionArg {
    internal FunctionArg(string name,  ArgType argType, Type type, Int32 argumentIndex) {
        Name = name;
        Type = type;
        ArgumentIndex = argumentIndex;
        ArgType = argType;
    }

    public Type Type { get; }
    public string Name { get; }
    public string FullName => Name + ":" + Type.Name;
    public  ArgType ArgType { get; }
    public Int32 ArgumentIndex { get; }
    public Function Function { get; internal set; }
}

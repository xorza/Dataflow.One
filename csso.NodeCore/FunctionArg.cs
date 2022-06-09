using csso.Common;

namespace csso.NodeCore;

public enum ArgDirection {
    In,
    Out
}

public  class FunctionArg {
    internal FunctionArg(string name,  ArgDirection argDirection, Type type, Int32 argumentIndex) {
        Name = name;
        Type = type;
        ArgumentIndex = argumentIndex;
        ArgDirection = argDirection;
    }

    public Type Type { get; }
    public string Name { get; }
    public string FullName => Name + ":" + Type.Name;
    public  ArgDirection ArgDirection { get; }
    public Int32 ArgumentIndex { get; }
    public Function Function { get; internal set; }
}

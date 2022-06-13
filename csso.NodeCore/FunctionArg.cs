using System;

namespace csso.NodeCore;

public enum ArgDirection {
    In,
    Out
}

public class FunctionArg {
    internal FunctionArg(string name, ArgDirection argDirection, Type type, int argumentIndex) {
        Name = name;
        Type = type;
        ArgumentIndex = argumentIndex;
        ArgDirection = argDirection;
    }

    public Type Type { get; }
    public string Name { get; }
    public string FullName => Name + ":" + Type.Name;
    public ArgDirection ArgDirection { get; }
    public int ArgumentIndex { get; }
    public Function Function { get; internal set; }
}
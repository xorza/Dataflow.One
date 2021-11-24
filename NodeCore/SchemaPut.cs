using System;

namespace csso.NodeCore {
public enum PutType {
    In,
    Out
}

public abstract class SchemaPut {
    public SchemaPut() { }

    public SchemaPut(string name, Type type) {
        Name = name;
        Type = type;
    }

    public Type Type { get; set; } = typeof(void);
    public string Name { get; set; } = "";
    public abstract PutType PutType { get; }
}
}